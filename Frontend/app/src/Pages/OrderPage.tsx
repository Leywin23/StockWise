import React, { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import * as Yup from "yup";
import { CreateCompanyWithAccountDto } from '../api';
import { yupResolver } from '@hookform/resolvers/yup';
import { CreateOrderDto, deleteOrderFromApi, getOrdersFromApi, OrderListDto, postOrderFromApi } from '../api/OrderApi';
import { toast } from 'react-toastify';
import { ApiError } from '../api/auth';
import { useAuth } from '../context/AuthContext';

const OrderPage = () => {
const navigate = useNavigate();

type ProductLine = {
    ean: string;
    quantity: number;
}

const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting }
} = useForm<CreateOrderDto>({
  defaultValues: {
    sellerName: "",
    sellerNIP: "",
    address: "",
    email: "",
    phone: "",
    currency: "",
    productsEANWithQuantity: {},
  }
});

const [productLine, setProductLines] = useState<ProductLine[]>([{ ean: "", quantity: 1},]);
const [orderList, setOrderList] = useState<OrderListDto[]>();
const { isLoggedIn } = useAuth(); 

 const addProductLine = () => {
    setProductLines((prev) => [...prev, { ean: "", quantity: 1 }]);
  };
  const updateProductLine = (
    index: number,
    field: keyof ProductLine,
    value: string | number
  ) => {
    setProductLines((prev)=>
      prev.map((line, i)=>
        i === index ? { ...line, [field]: value}:line))
  };

  const removeProductLine = (index: number)=> {
    setProductLines(prev => prev.filter((_, i)=> i !== index));
  }

 const validate = (data: CreateOrderDto): boolean => {
  const sellerName = (data.sellerName ?? "").trim();
  const sellerNIP = (data.sellerNIP ?? "").trim();
  const address = (data.address ?? "").trim();
  const email = (data.email ?? "").trim();
  const phone = (data.phone ?? "").trim();
  const currency = (data.currency ?? "").trim().toUpperCase();

  if (!sellerName) {
    toast.error("Seller name is required");
    return false;
  }

  if (!sellerNIP) {
    toast.error("Seller NIP is required");
    return false;
  }

  if (!/^\d{10}$/.test(sellerNIP)) {
    toast.error("Seller NIP must have exactly 10 digits");
    return false;
  }

  if (!address) {
    toast.error("Address is required");
    return false;
  }

  if (!email) {
    toast.error("Email is required");
    return false;
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    toast.error("Invalid email format");
    return false;
  }

  if (!phone) {
    toast.error("Phone number is required");
    return false;
  }

  if (!/^\d{9}$/.test(phone)) {
    toast.error("Invalid phone number");
    return false;
  }

  if (!currency || currency.length !== 3) {
    toast.error("Currency must have 3 characters (e.g. PLN)");
    return false;
  }

  return true;
};

const onSubmit = async (data: CreateOrderDto) => {
  if (!validate(data)) return;

  const productsMap: Record<string, number> = {};

  for (const line of productLine) {
    const ean = line.ean.trim();
    const qty = Number(line.quantity);

    if (!ean) continue;
    if (!Number.isFinite(qty) || qty < 1) continue;

    productsMap[ean] = qty;
  }

  if (Object.keys(productsMap).length === 0) {
    toast.error("Add at least one product");
    return;
  }

  const payload: CreateOrderDto = {
    ...data,
    productsEANWithQuantity: productsMap,
  };

  try {
    const createdOrder = await postOrderFromApi(payload);

    toast.success("Order created!");
    console.log("Created order:", createdOrder);

  } catch (err: any) {
    const apiError = err?.response?.data as ApiError | undefined;

    const message =
      apiError?.detail ||
      apiError?.title ||
      "An error occurred while creating order";

    toast.error(message);
    console.error("Create order error:", apiError);
  }
};
const loadOrders = async () => {
  try {
    const orders = await getOrdersFromApi();
    setOrderList(orders);
    console.log(orders);
  } catch (err: any) {
  const status = err?.response?.status;
  const data = err?.response?.data;

  console.log("STATUS:", status);
  console.log("ERROR DATA:", data);
  };
};
useEffect(()=>{
  if (!isLoggedIn) return;
  loadOrders();
},[]);

const handleDeleteOrder = async (orderId: number)=>{
  try{
  const deleted = await deleteOrderFromApi(orderId);

  setOrderList(prev=>(prev??[]).filter(o=>o.id !== orderId))
  toast.success("Order deleted")
  } catch (err: any) {
    const status = err?.response?.status;
    const apiError = err?.response?.data as ApiError | undefined;

    toast.error(
      apiError?.detail || apiError?.title || `Delete failed (${status})`
    );
  };
}
  return (
  <div className="min-h-screen bg-slate-100 px-4 py-8">
    <div className="mx-auto max-w-6xl grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">

      <div className="bg-white rounded-xl shadow-md p-6 border border-slate-200">
        <h1 className="text-2xl font-semibold text-slate-800 mb-1">
          Create new order
        </h1>
        <p className="text-sm text-slate-500 mb-6">
          Fill in seller data and basic order information.
        </p>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Seller name</label>
            <input
              {...register("sellerName")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
              placeholder="Company XYZ Sp. z o.o."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Seller NIP</label>
            <input
              {...register("sellerNIP")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
              placeholder="1234567890"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Address</label>
            <textarea
              {...register("address")}
              rows={3}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
              placeholder="Street 1, 00-000 Warsaw"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Seller email</label>
            <input
              type="email"
              {...register("email")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
              placeholder="seller@example.com"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Currency</label>
              <input
                {...register("currency")}
                maxLength={3}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm uppercase focus:ring-2 focus:ring-blue-500"
                placeholder="PLN"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Phone number</label>
              <input
                {...register("phone")}
                minLength={9}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
                placeholder="111222333"
              />
            </div>
          </div>

          <div className="border-t border-slate-200 pt-4">
            <div className="flex items-center justify-between mb-3">
              <h2 className="text-sm font-semibold text-slate-800">Products</h2>
              <button
                type="button"
                onClick={addProductLine}
                className="inline-flex items-center rounded-md bg-emerald-500 px-3 py-1 text-xs font-medium text-white hover:bg-emerald-600"
              >
                + Add product
              </button>
            </div>

            <div className="space-y-2">
              {productLine.map((line, index) => (
                <div key={index} className="flex gap-2 items-center">
                  <input
                    className="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="EAN"
                    value={line.ean}
                    onChange={(e) => updateProductLine(index, "ean", e.target.value)}
                  />
                  <input
                    className="w-28 rounded-md border border-slate-300 px-3 py-2 text-sm"
                    type="number"
                    min={1}
                    placeholder="Qty"
                    value={line.quantity}
                    onChange={(e) => updateProductLine(index, "quantity", Number(e.target.value))}
                  />
                  <button
                    type="button"
                    onClick={() => removeProductLine(index)}
                    className="text-xs text-red-600 hover:underline"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full mt-4 inline-flex justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {isSubmitting ? "Creating..." : "Create order"}
          </button>
        </form>
      </div>

      <div className="bg-white rounded-xl shadow-md p-6 border border-slate-200">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-semibold text-slate-800">Orders</h2>
          <button
            onClick={loadOrders}
            className="rounded-md bg-slate-900 px-3 py-2 text-xs font-medium text-white hover:bg-slate-800"
          >
            Refresh
          </button>
        </div>

        {!orderList || orderList.length === 0 ? (
          <p className="text-sm text-slate-500">No orders.</p>
        ) : (
          <div className="space-y-3">
            {orderList.map((o) => (
              <div key={o.id} className="rounded-lg border border-slate-200 p-3">
                <div className="flex items-center justify-between">
                  <div className="text-sm font-semibold text-slate-800">
                    Order #{o.id}
                  </div>
                  <div className="text-xs text-slate-500">
                    {new Date(o.createdAt).toLocaleString()}
                  </div>
                </div>

                <div className="mt-1 text-sm text-slate-700">
                  <div><span className="text-slate-500">Seller:</span> {o.seller?.name} ({o.seller?.nip})</div>
                  <div><span className="text-slate-500">Buyer:</span> {o.buyer?.name} ({o.buyer?.nip})</div>
                  <div><span className="text-slate-500">Status:</span> {o.status}</div>
                  <div>
                    <span className="text-slate-500">Total:</span>{" "}
                    {o.totalPrice?.amount} {o.totalPrice?.currency?.code ?? ""}
                  </div>
                  <button onClick={() => handleDeleteOrder(o.id)}>Delete</button>
                </div>

                <div className="mt-2">
                  <div className="text-xs font-medium text-slate-600 mb-1">Products:</div>
                  <ul className="text-xs text-slate-700 list-disc pl-5 space-y-1">
                    {o.productsWithQuantity?.map((pwq, idx) => (
                      <li key={idx}>
                        {pwq.product?.companyProductName} (EAN: {pwq.product?.ean}) × {pwq.quantity}
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

    </div>
  </div>
);
};

export default OrderPage