import React, { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "react-toastify";

import { CreateOrderDto, postOrderFromApi } from "../api/OrderApi";
import { ApiError } from "../../auth/api/accountApi";
import { CompanyProductWithCompanyDto } from "../../products/api/companyProducts";

type ProductLine = { ean: string; quantity: number };

type Props = {
  onCreated?: () => void;
  initialOffer?: CompanyProductWithCompanyDto | null;
};

const CreateOrderPanel: React.FC<Props> = ({ onCreated, initialOffer }) => {
  const {
    register,
    handleSubmit,
    formState: { isSubmitting },
    reset,
  } = useForm<CreateOrderDto>({
    defaultValues: {
      sellerName: "",
      sellerNIP: "",
      address: "",
      email: "",
      phone: "",
      currency: "PLN",
      productsEANWithQuantity: {},
    },
  });

  const [productLine, setProductLines] = useState<ProductLine[]>([
    { ean: "", quantity: 1 },
  ]);

  useEffect(() => {
    if (!initialOffer) return;

    reset({
      sellerName: initialOffer.company?.name ?? "",
      sellerNIP: initialOffer.company?.nip ?? "",
      address: initialOffer.company?.address ?? "",
      email: initialOffer.company?.email ?? "",
      phone: initialOffer.company?.phone ?? "",
      currency: initialOffer.price?.currency?.code ?? "PLN",
      productsEANWithQuantity: {},
    }); 

    setProductLines([
      { ean: initialOffer.ean ?? "", quantity: 1 }
    ]);
  }, [initialOffer, reset])


  const addProductLine = () =>
    setProductLines((prev) => [...prev, { ean: "", quantity: 1 }]);

  const updateProductLine = (
    index: number,
    field: keyof ProductLine,
    value: string | number
  ) => {
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
    if (!/^\d{10}$/.test(sellerNIP))
      return toast.error("Seller NIP must have exactly 10 digits"), false;

    if (!address) return toast.error("Address is required"), false;

    if (!email) return toast.error("Email is required"), false;
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
      return toast.error("Invalid email format"), false;

    if (!phone) return toast.error("Phone number is required"), false;
    if (!/^\d{9}$/.test(phone))
      return toast.error("Invalid phone number"), false;

    if (!currency || currency.length !== 3)
      return toast.error("Currency must have 3 characters (e.g. PLN)"), false;

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
      currency: (data.currency ?? "").trim().toUpperCase(),
      productsEANWithQuantity: productsMap,
    };

    try {
      await postOrderFromApi(payload);
      toast.success("Order created!");
      onCreated?.();

      reset();
      setProductLines([{ ean: "", quantity: 1 }]);
    } catch (err: any) {
      const apiError = err?.response?.data as ApiError | undefined;
      toast.error(apiError?.detail || apiError?.title || "Create failed");
    }

    
  };

  return (
    <div className="flex justify-center">
      <div className="w-full max-w-3xl">
        <div className="bg-white rounded-2xl shadow-md border border-slate-200 p-8">
          <div className="text-center mb-7">

            <h2 className="text-2xl font-semibold text-slate-800">
              Create order
            </h2>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="rounded-xl border border-slate-200 p-5">
              <div className="text-sm font-semibold text-slate-800 mb-4">
                Seller details
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Seller name
                  </label>
                  <input
                    {...register("sellerName")}
                    placeholder="Company name"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Seller NIP
                  </label>
                  <input
                    {...register("sellerNIP")}
                    placeholder="10 digits"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Email
                  </label>
                  <input
                    {...register("email")}
                    placeholder="email@company.com"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Phone
                  </label>
                  <input
                    {...register("phone")}
                    placeholder="9 digits"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Currency
                  </label>
                  <input
                    {...register("currency")}
                    placeholder="PLN"
                    maxLength={3}
                    className="w-full uppercase rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                </div>

                <div className="md:col-span-2">
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Address
                  </label>
                  <input
                    {...register("address")}
                    placeholder="Street, city, postal code"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                </div>
              </div>
            </div>

            <div className="rounded-xl border border-slate-200 p-5">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <div className="text-sm font-semibold text-slate-800">
                    Products
                  </div>
                  <div className="text-xs text-slate-500">
                    Add at least one product line (EAN + quantity).
                  </div>
                </div>

                <button
                  type="button"
                  onClick={addProductLine}
                  className="rounded-md bg-slate-900 px-3 py-2 text-xs font-medium text-white hover:bg-slate-800"
                >
                  + Add line
                </button>
              </div>

              <div className="space-y-2">
                {productLine.map((line, index) => (
                  <div
                    key={index}
                    className="rounded-lg border border-slate-200 bg-slate-50 p-3"
                  >
                    <div className="flex gap-3 items-center">
                      <div className="flex-1">
                        <label className="block text-[11px] font-medium text-slate-600 mb-1">
                          EAN
                        </label>
                        <input
                          className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                          placeholder="13 digits"
                          value={line.ean}
                          onChange={(e) =>
                            updateProductLine(index, "ean", e.target.value)
                          }
                        />
                      </div>

                      <div className="w-36">
                        <label className="block text-[11px] font-medium text-slate-600 mb-1">
                          Quantity
                        </label>
                        <input
                          className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                          type="number"
                          min={1}
                          value={line.quantity}
                          onChange={(e) =>
                            updateProductLine(
                              index,
                              "quantity",
                              Number(e.target.value)
                            )
                          }
                        />
                      </div>

                      <button
                        type="button"
                        onClick={() => removeProductLine(index)}
                        className="mt-6 text-xs font-medium text-rose-600 hover:underline"
                        disabled={productLine.length === 1}
                        title={
                          productLine.length === 1
                            ? "At least one line is required"
                            : "Remove"
                        }
                      >
                        Remove
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-lg bg-blue-600 py-3 text-sm font-semibold text-white hover:bg-blue-700 transition disabled:opacity-60"
            >
              {isSubmitting ? "Creating..." : "Create order"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default CreateOrderPanel;
