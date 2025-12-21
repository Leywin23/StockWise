import React from "react";
import { useForm } from "react-hook-form";
import * as Yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import {
  createCompanyProductDto,
  postCompanyProductFromApi,
} from "../api/companyProducts";
import { toast } from "react-toastify";
import type { Resolver } from "react-hook-form";

type Props = {
  onCreated: () => void;
};

type CompanyProductFormInputs = {
  companyProductName: string;
  ean: string;
  category: string;
  imageFile: FileList | null;
  description: string;
  amount: number;
  currency: string;
  stock: number;
  isAvailableForOrder: boolean;
};

const validationSchema = Yup.object().shape({
  companyProductName: Yup.string()
    .required("Product name is required")
    .max(100, "Max 100 characters"),
  ean: Yup.string()
    .required("EAN is required")
    .length(13, "EAN must be 13 characters"),
  category: Yup.string()
    .required("Category is required")
    .max(100, "Max 100 characters"),
  imageFile: Yup.mixed().nullable(),
  description: Yup.string().max(500, "Max 500 characters"),
  amount: Yup.number()
    .typeError("Price must be a number")
    .required("Price is required")
    .min(0, "Price must be >= 0"),
  currency: Yup.string()
    .required("Currency is required")
    .max(3, "Max 3 characters (e.g. PLN)"),
  stock: Yup.number()
    .typeError("Stock must be a number")
    .required("Stock is required")
    .min(0, "Stock must be >= 0")
    .integer("Stock must be an integer"),
  isAvailableForOrder: Yup.boolean().required(),
});

const CreateProductForm: React.FC<Props> = ({ onCreated }) => {
  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<CompanyProductFormInputs>({
    resolver: yupResolver(validationSchema) as Resolver<CompanyProductFormInputs>,
    defaultValues: {
      companyProductName: "",
      ean: "",
      category: "",
      imageFile: null,
      description: "",
      amount: 0,
      currency: "PLN",
      stock: 0,
      isAvailableForOrder: true,
    },
  });

  const imageFile = watch("imageFile");
  const imageName = imageFile?.[0]?.name ?? "";

  const onSubmit = async (data: CompanyProductFormInputs) => {
    const file = data.imageFile?.[0] ?? null;

    const dto: createCompanyProductDto = {
      companyProductName: data.companyProductName,
      ean: data.ean,
      category: data.category,
      imageFile: file,
      description: data.description || "",
      price: data.amount,
      currency: data.currency,
      stock: data.stock,
      isAvailableForOrder: data.isAvailableForOrder,
    };

    try {
      await postCompanyProductFromApi(dto);
      toast.success("Product created!");
      reset();
      onCreated();
    } catch (err: any) {
      toast.error(err?.response?.data?.detail || "Failed to create product");
    }
  };

  const FieldError = ({ msg }: { msg?: string }) =>
    msg ? <p className="mt-1 text-xs text-rose-600">{msg}</p> : null;

  return (
    <div className="flex justify-center">
      <div className="w-full max-w-3xl">
        <div className="bg-white rounded-2xl shadow-md border border-slate-200 p-8">
          <div className="text-center mb-7">

            <h2 className="text-2xl font-semibold text-slate-800">
              Create new product
            </h2>
            <p className="text-sm text-slate-500 mt-1">
              Add a product to your company catalog and make it available for orders.
            </p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">

            <div className="rounded-xl border border-slate-200 p-5">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Product name
                  </label>
                  <input
                    {...register("companyProductName")}
                    placeholder="e.g. Premium Coffee Beans"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                  <FieldError msg={errors.companyProductName?.message} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Category
                  </label>
                  <input
                    {...register("category")}
                    placeholder="e.g. Beverages"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                  <FieldError msg={errors.category?.message} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    EAN
                  </label>
                  <input
                    {...register("ean")}
                    placeholder="13 digits"
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                  <FieldError msg={errors.ean?.message} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Image (optional)
                  </label>

                  <div className="rounded-lg border border-slate-300 bg-slate-50 px-4 py-3">
                    <div className="flex items-center justify-between gap-3">
                      <div className="min-w-0">
                        <div className="text-sm text-slate-700">
                          {imageName ? (
                            <span className="font-medium">{imageName}</span>
                          ) : (
                            <span className="text-slate-500">No file selected</span>
                          )}
                        </div>
                        <div className="text-xs text-slate-500 mt-0.5">
                          PNG/JPG, best square image.
                        </div>
                      </div>

                      <label className="shrink-0 cursor-pointer rounded-md bg-slate-900 px-3 py-2 text-xs font-medium text-white hover:bg-slate-800">
                        Choose file
                        <input
                          type="file"
                          accept="image/*"
                          {...register("imageFile")}
                          className="hidden"
                        />
                      </label>
                    </div>
                  </div>

                  <FieldError msg={(errors.imageFile as any)?.message} />
                </div>
              </div>

              <div className="mt-4">
                <label className="block text-sm font-medium text-slate-700 mb-1">
                  Description (optional)
                </label>
                <textarea
                  {...register("description")}
                  rows={3}
                  placeholder="Short description visible in your catalog..."
                  className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                />
                <FieldError msg={errors.description?.message} />
              </div>
            </div>

            <div className="rounded-xl border border-slate-200 p-5">
              <div className="text-sm font-semibold text-slate-800 mb-4">
                Pricing & stock
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Price
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    {...register("amount")}
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                  <FieldError msg={errors.amount?.message} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Currency
                  </label>
                  <input
                    {...register("currency")}
                    maxLength={3}
                    className="w-full uppercase rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                  <FieldError msg={errors.currency?.message} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-slate-700 mb-1">
                    Stock
                  </label>
                  <input
                    type="number"
                    {...register("stock")}
                    className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  />
                  <FieldError msg={errors.stock?.message} />
                </div>
              </div>

              <div className="mt-5 flex items-center justify-between rounded-lg border border-slate-200 px-4 py-3">
                <div>
                  <div className="text-sm font-medium text-slate-800">
                    Available for order
                  </div>
                  <div className="text-xs text-slate-500">
                    If disabled, product stays in catalog but cannot be ordered.
                  </div>
                </div>

                <label className="inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    {...register("isAvailableForOrder")}
                    className="sr-only peer"
                  />
                  <div className="relative w-11 h-6 bg-slate-300 peer-focus:outline-none rounded-full peer peer-checked:bg-blue-600 transition">
                    <div className="absolute top-0.5 left-0.5 h-5 w-5 bg-white rounded-full transition peer-checked:translate-x-5" />
                  </div>
                </label>
              </div>
            </div>

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full rounded-lg bg-blue-600 py-3 text-sm font-semibold text-white hover:bg-blue-700 transition disabled:opacity-60"
            >
              {isSubmitting ? "Saving..." : "Create product"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default CreateProductForm;
