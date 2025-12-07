import React, { useEffect, useState } from "react";
import {
  companyProductDto,
  convertToAnotherCurrencyFromApi,
  createCompanyProductDto,
  deleteCompanyProductFromApi,
  getCompanyProductFromApi,
  Money,
  postCompanyProductFromApi,
  putCompanyProductFromApi,
  updateCompanyProductDto,
} from "../api/companyProducts";
import { useAuth } from "../context/AuthContext";
import { useNavigate } from "react-router-dom";

import { useForm } from "react-hook-form";
import * as Yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { toast } from "react-toastify";
import type { Resolver } from "react-hook-form";

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

const CompanyProductsPage: React.FC = () => {
  const [companyProducts, setCompanyProducts] = useState<companyProductDto[]>([]);
  const { isLoggedIn } = useAuth();
  const [editedProductId, setEditedProductId] = useState<number | null>(null);
  const [convertProductPrice, setConvertProductPrice] = useState<number | null>(null);
  const [convertCurrencyCode, setConvertCurrencyCode] = useState<string>("");
  const [toCode, setToCode] = useState<Money>({
    amount: 0,
    currency: {
      code: "",
    },
  });

  const navigate = useNavigate();

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

  const loadCompanyProducts = async () => {
    try {
      const items = await getCompanyProductFromApi();
      setCompanyProducts(items);
    } catch (error) {
      console.error("Error loading company products:", error);
    }
  };

  useEffect(() => {
    if (!isLoggedIn()) {
      navigate("/login");
      return;
    }

    loadCompanyProducts();
  }, [isLoggedIn, navigate]);

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
      loadCompanyProducts();
    } catch (err: any) {
      toast.error(err?.message || "Failed to create product");
    }
  };

  const handleDelete = async (productId: number) => {
    if (!window.confirm("Are you sure you want to delete this product?")) {
      return;
    }
    try {
      await deleteCompanyProductFromApi(productId);
      toast.success("Product deleted");
      await loadCompanyProducts();
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.message ||
        "Failed to delete product";
      toast.error(msg);
    }
  };

  const handleEdit = (productId: number) => {
    setEditedProductId(productId);

    const product = companyProducts.find((p) => p.companyProductId === productId);
    if (!product) return;

    reset({
      companyProductName: product.companyProductName,
      ean: product.ean,
      category: product.categoryName,
      imageFile: null,
      description: product.description,
      amount: product.price.amount,
      currency: product.price.currency.code,
      stock: product.stock,
      isAvailableForOrder: product.isAvailableForOrder,
    });
  };

  const handleUpdate = async (productId: number, data: CompanyProductFormInputs) => {
    const dto: updateCompanyProductDto = {
      companyProductName: data.companyProductName,
      description: data.description,
      price: data.amount,
      currency: data.currency,
      categoryName: data.category,
      imageFile: data.imageFile?.[0] ?? null,
      stock: data.stock,
      isAvailableForOrder: data.isAvailableForOrder,
    };

    try {
      await putCompanyProductFromApi(productId, dto);

      toast.success("Product updated!");
      setEditedProductId(null);
      reset();
      loadCompanyProducts();
    } catch (err: any) {
      toast.error(err?.response?.data?.message || "Failed to update product");
    }
  };

  const handleConvertCurrency = async (productId: number, currency: string) => {
  try {
    const converted = await convertToAnotherCurrencyFromApi(productId, currency);
    setToCode(converted);
  } catch (err: any) {
    toast.error(err?.response?.data?.message || "Failed to convert to another currency");
  }
};

const handleConvertFormSubmit = async (
  e: React.FormEvent,
  productId: number
) => {
  e.preventDefault();

  const code = convertCurrencyCode.trim().toUpperCase();
  if (!code) {
    toast.error("Please enter target currency code (e.g. USD)");
    return;
  }

  await handleConvertCurrency(productId, code);
};

  return (
    <div className="min-h-screen bg-slate-100 py-8">
      <div className="max-w-6xl mx-auto px-4">
        <h1 className="text-3xl font-bold text-slate-800 mb-6">
          Company Products
        </h1>

        <div className="grid md:grid-cols-2 gap-8">
          <div className="bg-white rounded-xl shadow-sm p-6 border border-slate-200">
            <h2 className="text-xl font-semibold mb-4 text-slate-800">
              Create new product
            </h2>

            <form
              onSubmit={handleSubmit(onSubmit)}
              className="space-y-4"
            >
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

          <div className="bg-white rounded-xl shadow-sm p-6 border border-slate-200">
            <h2 className="text-xl font-semibold mb-4 text-slate-800">
              Existing products
            </h2>

            {companyProducts.length === 0 ? (
              <p className="text-sm text-slate-500">No products yet.</p>
            ) : (
              <div className="grid gap-4 sm:grid-cols-1">
                {companyProducts.map((p) => (
                  <div
                    key={p.companyProductId}
                    className="border border-slate-200 rounded-lg bg-slate-50 overflow-hidden flex flex-col"
                  >
                    <div className="flex gap-4 p-4">
                      <div className="w-20 h-20 flex-shrink-0 rounded-md bg-slate-200 overflow-hidden flex items-center justify-center">
                        {p.image ? (
                          <img
                            src={p.image}
                            alt={p.companyProductName}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <span className="text-xs text-slate-500 text-center px-1">
                            No image
                          </span>
                        )}
                      </div>

                      <div className="flex-1">
                        <div className="flex items-start justify-between gap-2">
                          <div>
                            <div className="font-semibold text-slate-800">
                              {p.companyProductName}
                            </div>
                            <div className="text-xs text-slate-500">
                              ID: {p.companyProductId} • EAN: {p.ean}
                            </div>
                            <div className="text-xs text-slate-500">
                              Category: {p.categoryName}
                            </div>
                          </div>
                          <span className="inline-flex items-center rounded-full bg-slate-800 px-2 py-1 text-[10px] font-medium text-white whitespace-nowrap">
                            {p.price.amount} {p.price.currency.code}
                          </span>
                        </div>

                        <div className="mt-2 flex items-center justify-between text-xs text-slate-600">
                          <span>Stock: {p.stock}</span>
                          <span
                            className={
                              "inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-medium " +
                              (p.isAvailableForOrder
                                ? "bg-emerald-100 text-emerald-700"
                                : "bg-red-100 text-red-700")
                            }
                          >
                            {p.isAvailableForOrder ? "Available" : "Not available"}
                          </span>
                        </div>
                      </div>
                    </div>


                    {p.description && (
                      <div className="px-4 pb-2 text-xs text-slate-600">
                        {p.description}
                      </div>
                    )}

                    <div className="mt-auto border-t border-slate-200 px-4 py-2 flex justify-end gap-2">
                      <button
                        onClick={() => handleEdit(p.companyProductId)}
                        className="inline-flex justify-center rounded-md bg-slate-700 px-3 py-1 text-xs font-medium text-white hover:bg-slate-800"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(p.companyProductId)}
                        className="inline-flex justify-center rounded-md bg-red-500 px-3 py-1 text-xs font-medium text-white hover:bg-red-600"
                      >
                        Delete
                      </button>
                      <button 
                        onClick={()=> setConvertProductPrice(p.companyProductId)}
                      >Convert</button>
                    </div>


                    {editedProductId === p.companyProductId && (
                      <form
                        onSubmit={handleSubmit((data) =>
                          handleUpdate(p.companyProductId, data)
                        )}
                        className="border-t border-slate-200 p-4 space-y-2 text-sm bg-white"
                      >
                        <div className="grid grid-cols-2 gap-2">
                          <div>
                            <label className="block text-xs font-medium text-slate-600 mb-1">
                              Product name
                            </label>
                            <input
                              {...register("companyProductName")}
                              className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-slate-600 mb-1">
                              Category
                            </label>
                            <input
                              {...register("category")}
                              className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-xs font-medium text-slate-600 mb-1">
                            Description
                          </label>
                          <input
                            {...register("description")}
                            className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                          />
                        </div>

                        <div className="grid grid-cols-3 gap-2">
                          <div>
                            <label className="block text-xs font-medium text-slate-600 mb-1">
                              Price
                            </label>
                            <input
                              type="number"
                              step="0.01"
                              {...register("amount")}
                              className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-slate-600 mb-1">
                              Currency
                            </label>
                            <input
                              {...register("currency")}
                              className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-slate-600 mb-1">
                              Stock
                            </label>
                            <input
                              type="number"
                              {...register("stock")}
                              className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="block text-xs font-medium text-slate-600 mb-1">
                            Image file
                          </label>
                          <input
                            type="file"
                            accept="image/*"
                            {...register("imageFile")}
                            className="w-full text-xs"
                          />
                        </div>

                        <div className="flex items-center">
                          <label className="inline-flex items-center text-xs text-slate-700">
                            <input
                              type="checkbox"
                              {...register("isAvailableForOrder")}
                              className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
                            />
                            <span className="ml-2">Available for order</span>
                          </label>
                        </div>

                        <div className="flex gap-2 pt-1">
                          <button
                            type="submit"
                            className="inline-flex justify-center rounded-md bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                          >
                            Save
                          </button>
                          <button
                            type="button"
                            onClick={() => {
                              setEditedProductId(null);
                              reset();
                            }}
                            className="inline-flex justify-center rounded-md bg-slate-300 px-3 py-1 text-xs font-medium text-slate-800 hover:bg-slate-400"
                          >
                            Cancel
                          </button>
                        </div>
                      </form>
                    )}
                    {convertProductPrice === p.companyProductId && (
                      <form
                        onSubmit={(e) => handleConvertFormSubmit(e, p.companyProductId)}
                        className="border-t border-slate-200 p-4 flex items-end gap-3 bg-white"
                      >
                        <div>
                          <label className="block text-xs font-medium text-slate-600 mb-1">
                            To currency code
                          </label>
                          <input
                            type="text"
                            value={convertCurrencyCode}
                            onChange={(e) => setConvertCurrencyCode(e.target.value)}
                            maxLength={3}
                            className="w-24 rounded-md border border-slate-300 px-2 py-1 text-xs uppercase focus:outline-none focus:ring-1 focus:ring-blue-500"
                            placeholder="USD"
                          />
                        </div>

                        <button
                          type="submit"
                          className="inline-flex justify-center rounded-md bg-blue-600 px-3 py-1 text-xs font-medium text-white hover:bg-blue-700"
                        >
                          Convert
                        </button>

                        {toCode.currency.code && (
                          <div className="text-xs text-slate-700 ml-2">
                            {p.price.amount} {p.price.currency.code} →{" "}
                            <span className="font-semibold">
                              {toCode.amount} {toCode.currency.code}
                            </span>
                          </div>
                        )}
                      </form>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default CompanyProductsPage;
