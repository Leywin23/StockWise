import React from "react";
import { useForm } from "react-hook-form";
import * as Yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import {
  createCompanyProductDto,
  postCompanyProductFromApi,
} from "../../api/companyProducts";
import { toast } from "react-toastify";
import type { Resolver } from "react-hook-form";

type FormInputs = {
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

  const onSubmit = async (data: FormInputs) => {
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

  return (
    <div className="bg-white rounded-xl shadow-sm p-6 border border-slate-200">
      <h2 className="text-xl font-semibold mb-4 text-slate-800">
        Create new product
      </h2>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">
            Product name
          </label>
          <input
            {...register("companyProductName")}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          {errors.companyProductName && (
            <p className="mt-1 text-xs text-red-500">
              {errors.companyProductName.message}
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">
            EAN
          </label>
          <input
            {...register("ean")}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          {errors.ean && (
            <p className="mt-1 text-xs text-red-500">
              {errors.ean.message}
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">
            Category
          </label>
          <input
            {...register("category")}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          {errors.category && (
            <p className="mt-1 text-xs text-red-500">
              {errors.category.message}
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">
            Image file
          </label>
          <input
            type="file"
            accept="image/*"
            {...register("imageFile")}
            className="w-full text-sm"
          />
          {errors.imageFile && (
            <p className="mt-1 text-xs text-red-500">
              {(errors.imageFile as any).message}
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">
            Description
          </label>
          <textarea
            {...register("description")}
            className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={3}
          />
          {errors.description && (
            <p className="mt-1 text-xs text-red-500">
              {errors.description.message}
            </p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Price
            </label>
            <input
              type="number"
              step="0.01"
              {...register("amount")}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.amount && (
              <p className="mt-1 text-xs text-red-500">
                {errors.amount.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Currency
            </label>
            <input
              {...register("currency")}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.currency && (
              <p className="mt-1 text-xs text-red-500">
                {errors.currency.message}
              </p>
            )}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Stock
            </label>
            <input
              type="number"
              {...register("stock")}
              className="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.stock && (
              <p className="mt-1 text-xs text-red-500">
                {errors.stock.message}
              </p>
            )}
          </div>

          <div className="flex items-center mt-6">
            <label className="inline-flex items-center text-sm text-slate-700">
              <input
                type="checkbox"
                {...register("isAvailableForOrder")}
                className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="ml-2">Available for order</span>
            </label>
          </div>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full mt-2 inline-flex justify-center rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-60"
        >
          {isSubmitting ? "Saving..." : "Create product"}
        </button>
      </form>
    </div>
  );
};

export default CreateProductForm;