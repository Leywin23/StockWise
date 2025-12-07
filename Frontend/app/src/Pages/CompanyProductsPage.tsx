import React, { useEffect, useState } from "react";
import {
  companyProductDto,
  createCompanyProductDto,
  getCompanyProductFromApi,
  postCompanyProductFromApi,
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
      price: data.amount,          // <- tylko liczba
      currency: data.currency,     // <- np. "PLN"
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

  return (
    <div style={{ padding: 20 }}>
      <h1>Company Products</h1>

      <form
        onSubmit={handleSubmit(onSubmit)}
        className="space-y-4 max-w-md"
        style={{ marginBottom: 30 }}
      >
        <div>
          <label>Product name</label>
          <input
            {...register("companyProductName")}
            className="border rounded w-full p-2"
          />
          {errors.companyProductName && (
            <p className="text-red-500">{errors.companyProductName.message}</p>
          )}
        </div>

        <div>
          <label>EAN</label>
          <input
            {...register("ean")}
            className="border rounded w-full p-2"
          />
          {errors.ean && <p className="text-red-500">{errors.ean.message}</p>}
        </div>

        <div>
          <label>Category</label>
          <input
            {...register("category")}
            className="border rounded w-full p-2"
          />
          {errors.category && (
            <p className="text-red-500">{errors.category.message}</p>
          )}
        </div>

        <div>
          <label>Image file</label>
          <input
            type="file"
            accept="image/*"
            {...register("imageFile")}
          />
          {errors.imageFile && (
            <p className="text-red-500">
              {(errors.imageFile as any).message}
            </p>
          )}
        </div>

        <div>
          <label>Description</label>
          <textarea
            {...register("description")}
            className="border rounded w-full p-2"
          />
          {errors.description && (
            <p className="text-red-500">{errors.description.message}</p>
          )}
        </div>

        <div>
          <label>Price</label>
          <input
            type="number"
            step="0.01"
            {...register("amount")}
            className="border rounded w-full p-2"
          />
          {errors.amount && (
            <p className="text-red-500">{errors.amount.message}</p>
          )}
        </div>

        <div>
          <label>Currency</label>
          <input
            {...register("currency")}
            className="border rounded w-full p-2"
          />
          {errors.currency && (
            <p className="text-red-500">{errors.currency.message}</p>
          )}
        </div>

        <div>
          <label>Stock</label>
          <input
            type="number"
            {...register("stock")}
            className="border rounded w-full p-2"
          />
          {errors.stock && (
            <p className="text-red-500">{errors.stock.message}</p>
          )}
        </div>

        <div>
          <label>
            <input type="checkbox" {...register("isAvailableForOrder")} />
            {" "}Available for order
          </label>
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="bg-blue-600 text-white px-4 py-2 rounded"
        >
          {isSubmitting ? "Saving..." : "Create product"}
        </button>
      </form>

      <h2>Existing products</h2>
      <ul>
        {companyProducts.map((p) => (
          <li key={p.ean}>
            {p.companyProductName} — {p.price.amount} {p.price.currency.code} (stock:{" "}
            {p.stock})
          </li>
        ))}
      </ul>
    </div>
  );
};

export default CompanyProductsPage;
