import React, { useEffect, useMemo, useState } from "react";
import { toast } from "react-toastify";
import { useAuth } from "../../../app/core/context/AuthContext";
import { 
  acceptOrRejectOrderFromApi,
  deleteOrderFromApi,
  getOrdersFromApi,
  OrderListDto,
  OrderStatus,
  putOrderFromApi,
  UpdateOrderDto, } from "../api/OrderApi";
import { ApiError } from "../../auth/api/accountApi";

type ProductLine = { ean: string; quantity: number };
type Tab = "received" | "placed";

const OrdersListPanel: React.FC = () => {
  const { isLoggedIn, company } = useAuth();
  const myNip = company?.nip ?? "";

  const [orderList, setOrderList] = useState<OrderListDto[]>([]);
  const [tab, setTab] = useState<Tab>("received");

  const [selectedId, setSelectedId] = useState<number | null>(null);

  const [editMode, setEditMode] = useState(false);
  const [editCurrency, setEditCurrency] = useState<string>("");
  const [editProducts, setEditProducts] = useState<ProductLine[]>([]);

  const loadOrders = async () => {
    try {
      const orders = await getOrdersFromApi();
      setOrderList(Array.isArray(orders) ? orders : []);
    } catch (err: any) {
      console.error(err?.response?.data ?? err);
      toast.error("Failed to load orders");
    }
  };

  useEffect(() => {
    if (!isLoggedIn) return;
    loadOrders();
  }, [isLoggedIn]);

  const receivedOrders = useMemo(
    () => orderList.filter((o) => o.seller?.nip === myNip),
    [orderList, myNip]
  );
  const placedOrders = useMemo(
    () => orderList.filter((o) => o.buyer?.nip === myNip),
    [orderList, myNip]
  );

  const activeOrders = tab === "received" ? receivedOrders : placedOrders;

  useEffect(() => {
    if (activeOrders.length === 0) {
      setSelectedId(null);
      setEditMode(false);
      return;
    }
    if (!selectedId || !activeOrders.some((o) => o.id === selectedId)) {
      setSelectedId(activeOrders[0].id);
      setEditMode(false);
    }
  }, [tab, orderList, myNip]);

  const selectedOrder = useMemo(() => {
    return activeOrders.find((o) => o.id === selectedId) ?? null;
  }, [activeOrders, selectedId]);

  const isPending = selectedOrder?.status === OrderStatus.Pending;

  const handleDelete = async (orderId: number) => {
    try {
      await deleteOrderFromApi(orderId);
      setOrderList((prev) => prev.filter((o) => o.id !== orderId));
      toast.success("Order deleted");
      if (selectedId === orderId) setSelectedId(null);
    } catch (err: any) {
      const status = err?.response?.status;
      const apiError = err?.response?.data as ApiError | undefined;
      toast.error(apiError?.detail || apiError?.title || `Delete failed (${status})`);
    }
  };

  const handleUpdate = async (orderId: number, dto: UpdateOrderDto) => {
    try {
      const edited = await putOrderFromApi(orderId, dto);
      setOrderList((prev) => prev.map((o) => (o.id === orderId ? edited : o)));
      toast.success("Order edited");
      setEditMode(false);
    } catch (err: any) {
      const status = err?.response?.status;
      const apiError = err?.response?.data as ApiError | undefined;
      toast.error(apiError?.detail || apiError?.title || `Update failed (${status})`);
    }
  };

  const handleReceivedDecision = async (orderId: number, status: OrderStatus) => {
    try {
      const updated = await acceptOrRejectOrderFromApi(orderId, status);
      setOrderList((prev) =>
        prev.map((o) => (o.id === orderId ? { ...o, status: updated.status } : o))
      );
      toast.success(status === OrderStatus.Accepted ? "Order accepted" : "Order rejected");
    } catch (err: any) {
      const apiError = err?.response?.data as ApiError | undefined;
      toast.error(apiError?.detail || apiError?.title || "Action failed");
    }
  };
  const handleCancelOrConfirm = async (
  orderId: number,
  status: OrderStatus
) => {
  try {
    const updated = await acceptOrRejectOrderFromApi(orderId, status);

    setOrderList((prev) =>
      prev.map((o) =>
        o.id === orderId ? { ...o, status: updated.status } : o
      )
    );

    toast.success(
      status === OrderStatus.Canceled
        ? "Order cancelled"
        : "Order confirmed"
    );
  } catch (err: any) {
    const apiError = err?.response?.data as ApiError | undefined;
    toast.error(apiError?.detail || apiError?.title || "Action failed");
  }
};

  const openEdit = () => {
    if (!selectedOrder) return;
    setEditMode(true);
    setEditCurrency(selectedOrder.totalPrice?.currency?.code ?? "");
    setEditProducts(
      (selectedOrder.productsWithQuantity ?? []).map((p) => ({
        ean: p.product?.ean ?? "",
        quantity: p.quantity ?? 1,
      }))
    );
  };

  const closeEdit = () => {
    setEditMode(false);
    setEditCurrency("");
    setEditProducts([]);
  };

  const addEditLine = () => setEditProducts((prev) => [...prev, { ean: "", quantity: 1 }]);
  const updateEditLine = (index: number, field: keyof ProductLine, value: string | number) => {
    setEditProducts((prev) =>
      prev.map((line, i) => (i === index ? { ...line, [field]: value } : line))
    );
  };
  const removeEditLine = (index: number) =>
    setEditProducts((prev) => prev.filter((_, i) => i !== index));

  const saveEdit = () => {
  if (!selectedOrder) return;

  const productsEANWithQuantity: Record<string, number> = {};

  for (const p of editProducts) {
    const ean = p.ean.trim();
    const qty = Number(p.quantity);

    if (!ean) continue;
    if (!Number.isFinite(qty) || qty < 1) continue;

    productsEANWithQuantity[ean] = qty;
  }

  if (Object.keys(productsEANWithQuantity).length === 0) {
    toast.error("Add at least one product");
    return;
  }

  const dto: UpdateOrderDto = {
    currency: editCurrency.trim().toUpperCase(),
    productsEANWithQuantity,
  };

  handleUpdate(selectedOrder.id, dto);
};


  if (!isLoggedIn) return null;

  return (
    <div className="min-h-screen bg-slate-100 px-4 py-8">
      <div className="mx-auto max-w-7xl grid grid-cols-1 lg:grid-cols-[360px_1fr] gap-6 items-start">

        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
          <div className="p-4 border-b border-slate-200 flex items-center justify-between">
            <div>
              <div className="text-xs text-slate-500">Orders</div>
              <div className="text-lg font-semibold text-slate-800">Browse</div>
            </div>

            <button
              onClick={loadOrders}
              className="rounded-md bg-slate-900 px-3 py-2 text-xs font-medium text-white hover:bg-slate-800"
            >
              Refresh
            </button>
          </div>

          <div className="p-3 border-b border-slate-200">
            <div className="inline-flex w-full rounded-lg bg-slate-100 p-1">
              <button
                type="button"
                onClick={() => setTab("received")}
                className={
                  "flex-1 rounded-md px-3 py-2 text-xs font-medium " +
                  (tab === "received"
                    ? "bg-white shadow text-slate-900"
                    : "text-slate-600 hover:text-slate-900")
                }
              >
                📦 Received
              </button>
              <button
                type="button"
                onClick={() => setTab("placed")}
                className={
                  "flex-1 rounded-md px-3 py-2 text-xs font-medium " +
                  (tab === "placed"
                    ? "bg-white shadow text-slate-900"
                    : "text-slate-600 hover:text-slate-900")
                }
              >
                🧾 Placed
              </button>
            </div>
          </div>

          <div className="max-h-[70vh] overflow-auto divide-y divide-slate-100">
            {activeOrders.length === 0 ? (
              <div className="p-6 text-sm text-slate-500">No orders.</div>
            ) : (
              activeOrders.map((o) => {
                const selected = o.id === selectedId;
                return (
                  <button
                    key={o.id}
                    type="button"
                    onClick={() => {
                      setSelectedId(o.id);
                      setEditMode(false);
                    }}
                    className={
                      "w-full text-left p-4 hover:bg-slate-50 " +
                      (selected ? "bg-slate-50" : "")
                    }
                  >
                    <div className="flex items-start justify-between">
                      <div>
                        <div className="text-sm font-semibold text-slate-800">
                          Order #{o.id}
                        </div>
                        <div className="text-xs text-slate-500 mt-0.5">
                          {tab === "received" ? o.buyer?.name : o.seller?.name}
                        </div>
                      </div>

                      <div className="text-right">
                        <div className="text-xs font-medium text-slate-700">
                          {OrderStatus[o.status]}
                        </div>
                        <div className="text-xs text-slate-500 mt-1">
                          {o.totalPrice?.amount} {o.totalPrice?.currency?.code ?? ""}
                        </div>
                      </div>
                    </div>

                    <div className="mt-2 text-[11px] text-slate-500">
                      {new Date(o.createdAt).toLocaleString()}
                    </div>
                  </button>
                );
              })
            )}
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-sm p-6 border border-slate-200">
          {!selectedOrder ? (
            <div className="text-sm text-slate-500">Select an order from the left.</div>
          ) : (
            <>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <div className="text-xs text-slate-500 mb-1">Order details</div>
                  <h2 className="text-xl font-semibold text-slate-800">
                    Order #{selectedOrder.id}
                  </h2>
                  <div className="text-xs text-slate-500 mt-1">
                    {new Date(selectedOrder.createdAt).toLocaleString()}
                  </div>
                </div>

                <div className="flex gap-2">
                  {tab === "placed" && (
                    <button
                      type="button"
                      onClick={() => (editMode ? closeEdit() : openEdit())}
                      className={
                        "rounded-md px-3 py-2 text-xs font-medium " +
                        (editMode
                          ? "bg-slate-200 text-slate-900"
                          : "bg-blue-600 text-white hover:bg-blue-700")
                      }
                    >
                      {editMode ? "Cancel edit" : "Edit"}
                    </button>
                  )}

                  <button
                    type="button"
                    onClick={() => handleDelete(selectedOrder.id)}
                    className="rounded-md bg-red-600 px-3 py-2 text-xs font-medium text-white hover:bg-red-700"
                  >
                    Delete
                  </button>
                </div>
              </div>

              <div className="mt-4 grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm text-slate-700">
                <div className="rounded-lg border border-slate-200 p-3">
                  <div className="text-xs text-slate-500 mb-1">Seller</div>
                  <div className="font-medium">{selectedOrder.seller?.name}</div>
                  <div className="text-xs text-slate-500">{selectedOrder.seller?.nip}</div>
                </div>
                <div className="rounded-lg border border-slate-200 p-3">
                  <div className="text-xs text-slate-500 mb-1">Buyer</div>
                  <div className="font-medium">{selectedOrder.buyer?.name}</div>
                  <div className="text-xs text-slate-500">{selectedOrder.buyer?.nip}</div>
                </div>
              </div>

              <div className="mt-3 flex items-center justify-between rounded-lg border border-slate-200 p-3">
                <div className="text-sm text-slate-700">
                  <span className="text-slate-500">Status:</span>{" "}
                  <span className="font-medium">{OrderStatus[selectedOrder.status]}</span>
                </div>
                <div className="text-sm text-slate-700">
                  <span className="text-slate-500">Total:</span>{" "}
                  <span className="font-medium">
                    {selectedOrder.totalPrice?.amount} {selectedOrder.totalPrice?.currency?.code ?? ""}
                  </span>
                </div>
              </div>

              {tab === "received" && isPending && (
                <div className="mt-4 flex gap-2">
                  <button
                    type="button"
                    onClick={() => handleReceivedDecision(selectedOrder.id, OrderStatus.Accepted)}
                    className="rounded-md bg-emerald-600 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-700"
                  >
                    Accept
                  </button>
                  <button
                    type="button"
                    onClick={() => handleReceivedDecision(selectedOrder.id, OrderStatus.Rejected)}
                    className="rounded-md bg-rose-600 px-3 py-2 text-xs font-medium text-white hover:bg-rose-700"
                  >
                    Reject
                  </button>
                </div>
              )}
                {tab === "placed" && isPending && (
                    <div className="mt-4 flex gap-2">
                        <button
                        type="button"
                        onClick={() => handleCancelOrConfirm(selectedOrder.id, OrderStatus.Canceled)}
                        className="rounded-md bg-slate-700 px-3 py-2 text-xs font-medium text-white hover:bg-slate-800"
                        >
                        🛑 Cancel
                        </button>

                        <button
                        type="button"
                        onClick={() => handleCancelOrConfirm(selectedOrder.id, OrderStatus.Completed)}
                        className="rounded-md bg-blue-600 px-3 py-2 text-xs font-medium text-white hover:bg-blue-700"
                        >
                        ✅ Confirm
                        </button>
                    </div>
                )}


              {tab === "placed" && editMode && (
                <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-4">
                  <div className="text-sm font-semibold text-slate-800 mb-3">Edit</div>

                  <label className="block text-xs font-medium text-slate-600 mb-1">
                    Currency
                  </label>
                  <input
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm uppercase"
                    maxLength={3}
                    value={editCurrency}
                    onChange={(e) => setEditCurrency(e.target.value.toUpperCase())}
                  />

                  <div className="mt-4 flex items-center justify-between">
                    <div className="text-xs font-semibold text-slate-700">Products</div>
                    <button
                      type="button"
                      onClick={addEditLine}
                      className="rounded-md bg-emerald-600 px-3 py-1 text-xs text-white hover:bg-emerald-700"
                    >
                      + Add
                    </button>
                  </div>

                  <div className="mt-2 space-y-2">
                    {editProducts.map((line, idx) => (
                      <div key={idx} className="flex gap-2 items-center">
                        <input
                          className="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm"
                          placeholder="EAN"
                          value={line.ean}
                          onChange={(e) => updateEditLine(idx, "ean", e.target.value)}
                        />
                        <input
                          className="w-28 rounded-md border border-slate-300 px-3 py-2 text-sm"
                          type="number"
                          min={1}
                          placeholder="Qty"
                          value={line.quantity}
                          onChange={(e) => updateEditLine(idx, "quantity", Number(e.target.value))}
                        />
                        <button
                          type="button"
                          onClick={() => removeEditLine(idx)}
                          className="text-xs text-red-600 hover:underline"
                        >
                          Remove
                        </button>
                      </div>
                    ))}
                  </div>

                  <div className="mt-4 flex gap-2">
                    <button
                      type="button"
                      onClick={saveEdit}
                      className="rounded-md bg-blue-600 px-3 py-2 text-xs font-medium text-white hover:bg-blue-700"
                    >
                      Save
                    </button>
                    <button
                      type="button"
                      onClick={closeEdit}
                      className="rounded-md bg-slate-200 px-3 py-2 text-xs font-medium"
                    >
                      Cancel
                    </button>
                  </div>
                </div>
              )}

              <div className="mt-5">
                <div className="text-xs font-medium text-slate-600 mb-2">Products</div>

                <div className="rounded-xl border border-slate-200 overflow-hidden">
                  <div className="grid grid-cols-[1fr_120px] bg-slate-50 px-4 py-2 text-[11px] uppercase tracking-wide text-slate-500">
                    <div>Product</div>
                    <div className="text-right">Qty</div>
                  </div>

                  <div className="divide-y divide-slate-100">
                    {(selectedOrder.productsWithQuantity ?? []).map((pwq, idx) => (
                      <div key={idx} className="grid grid-cols-[1fr_120px] px-4 py-3 text-sm">
                        <div className="text-slate-800">
                          <div className="font-medium">
                            {pwq.product?.companyProductName}
                          </div>
                          <div className="text-xs text-slate-500">
                            EAN: {pwq.product?.ean}
                          </div>
                        </div>
                        <div className="text-right text-slate-700 font-medium">
                          {pwq.quantity}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default OrdersListPanel;
