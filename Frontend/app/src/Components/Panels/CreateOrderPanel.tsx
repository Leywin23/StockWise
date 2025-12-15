import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "react-toastify";

import { CreateOrderDto, postOrderFromApi } from "../../api/OrderApi";
import { ApiError } from "../../api/auth";

type ProductLine = { ean: string; quantity: number };

type Props = {
  onCreated?: () => void; // np. żeby odświeżyć listę po dodaniu
};

const CreateOrderPanel: React.FC<Props> = ({ onCreated }) => {
  const { register, handleSubmit, formState: { isSubmitting } } =
    useForm<CreateOrderDto>({
      defaultValues: {
        sellerName: "",
        sellerNIP: "",
        address: "",
        email: "",
        phone: "",
        currency: "",
        productsEANWithQuantity: {},
      },
    });

  const [productLine, setProductLines] = useState<ProductLine[]>([
    { ean: "", quantity: 1 },
  ]);

  const addProductLine = () =>
    setProductLines((prev) => [...prev, { ean: "", quantity: 1 }]);

  const updateProductLine = (index: number, field: keyof ProductLine, value: string | number) => {
    setProductLines((prev) =>
      prev.map((line, i) => (i === index ? { ...line, [field]: value } : line))
    );
  };

  const removeProductLine = (index: number) =>
    setProductLines((prev) => prev.filter((_, i) => i !== index));

  const validateCreate = (data: CreateOrderDto): boolean => {
    const sellerName = (data.sellerName ?? "").trim();
    const sellerNIP = (data.sellerNIP ?? "").trim();
    const address = (data.address ?? "").trim();
    const email = (data.email ?? "").trim();
    const phone = (data.phone ?? "").trim();
    const currency = (data.currency ?? "").trim().toUpperCase();

    if (!sellerName) return toast.error("Seller name is required"), false;
    if (!sellerNIP) return toast.error("Seller NIP is required"), false;
    if (!/^\d{10}$/.test(sellerNIP)) return toast.error("Seller NIP must have exactly 10 digits"), false;
    if (!address) return toast.error("Address is required"), false;

    if (!email) return toast.error("Email is required"), false;
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return toast.error("Invalid email format"), false;

    if (!phone) return toast.error("Phone number is required"), false;
    if (!/^\d{9}$/.test(phone)) return toast.error("Invalid phone number"), false;

    if (!currency || currency.length !== 3) return toast.error("Currency must have 3 characters (e.g. PLN)"), false;

    return true;
  };

  const buildProductsMap = (lines: ProductLine[]): Record<string, number> => {
    const map: Record<string, number> = {};
    for (const line of lines) {
      const ean = line.ean.trim();
      const qty = Number(line.quantity);
      if (!ean) continue;
      if (!Number.isFinite(qty) || qty < 1) continue;
      map[ean] = qty;
    }
    return map;
  };

  const onSubmit = async (data: CreateOrderDto) => {
    if (!validateCreate(data)) return;

    const productsMap = buildProductsMap(productLine);
    if (Object.keys(productsMap).length === 0) {
      toast.error("Add at least one product");
      return;
    }

    const payload: CreateOrderDto = {
      ...data,
      productsEANWithQuantity: productsMap,
    };

    try {
      await postOrderFromApi(payload);
      toast.success("Order created!");
      onCreated?.();
    } catch (err: any) {
      const apiError = err?.response?.data as ApiError | undefined;
      toast.error(apiError?.detail || apiError?.title || "Create failed");
    }
  };

  return (
    <div className="bg-white rounded-xl shadow-sm p-6 border border-slate-200">
      <div className="mb-4">
        <div className="text-xs text-slate-500 mb-1">Orders</div>
        <h2 className="text-xl font-semibold text-slate-800">Create order</h2>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-3">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <input {...register("sellerName")} className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Seller name" />
          <input {...register("sellerNIP")} className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Seller NIP (10 digits)" />
          <input {...register("email")} className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Email" />
          <input {...register("phone")} className="rounded-md border border-slate-300 px-3 py-2 text-sm" placeholder="Phone (9 digits)" />
          <input {...register("currency")} className="rounded-md border border-slate-300 px-3 py-2 text-sm uppercase" placeholder="Currency (PLN)" maxLength={3} />
          <input {...register("address")} className="rounded-md border border-slate-300 px-3 py-2 text-sm md:col-span-2" placeholder="Address" />
        </div>

        <div className="border-t border-slate-200 pt-3">
          <div className="flex items-center justify-between mb-2">
            <div className="text-xs font-semibold text-slate-700">Products</div>
            <button type="button" onClick={addProductLine} className="rounded-md bg-emerald-600 px-3 py-1 text-xs text-white">
              + Add
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
                <button type="button" onClick={() => removeProductLine(index)} className="text-xs text-red-600 hover:underline">
                  Remove
                </button>
              </div>
            ))}
          </div>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {isSubmitting ? "Creating..." : "Create order"}
        </button>
      </form>
    </div>
  );
};

export default CreateOrderPanel;
