import React, { useState } from "react";
import {
  companyProductDto,
  deleteCompanyProductFromApi,
  putCompanyProductFromApi,
  convertToAnotherCurrencyFromApi,
  updateCompanyProductDto,
  Money,
} from "../api/companyProducts";
import { toast } from "react-toastify";

type Props = {
  products: companyProductDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  onPrevPage: () => void;
  onNextPage: () => void;
  onReload: () => void;
  onOpenHistory: (productId: number, productName: string) => void;
};

type EditForm = {
  companyProductName: string;
  category: string;
  description: string;
  amount: number;
  currency: string;
  stock: number;
  isAvailableForOrder: boolean;
  imageFile: File | null;
};

const ProductsTable: React.FC<Props> = ({
  products,
  page,
  pageSize,
  totalCount,
  onPrevPage,
  onNextPage,
  onReload,
  onOpenHistory,
}) => {
  const [editedId, setEditedId] = useState<number | null>(null);
  const [editForm, setEditForm] = useState<EditForm | null>(null);

  const [convertRowId, setConvertRowId] = useState<number | null>(null);
  const [convertCode, setConvertCode] = useState("");
  const [convertResult, setConvertResult] = useState<Money | null>(null);

  const startEdit = (p: companyProductDto) => {
    setEditedId(p.companyProductId);
    setEditForm({
      companyProductName: p.companyProductName,
      category: p.category?.name ?? "",
      description: p.description,
      amount: p.price.amount,
      currency: p.price.currency.code,
      stock: p.stock,
      isAvailableForOrder: p.isAvailableForOrder,
      imageFile: null,
    });
  };

  const handleEditChange = (
    field: keyof EditForm,
    value: string | number | boolean | File | null
  ) => {
    setEditForm((prev) =>
      prev
        ? {
            ...prev,
            [field]: value,
          }
        : prev
    );
  };

  const saveEdit = async (id: number) => {
    if (!editForm) return;

    const dto: updateCompanyProductDto = {
      companyProductName: editForm.companyProductName,
      description: editForm.description,
      price: editForm.amount,
      currency: editForm.currency,
      categoryName: editForm.category,
      imageFile: editForm.imageFile,
      stock: editForm.stock,
      isAvailableForOrder: editForm.isAvailableForOrder,
    };

    try {
      await putCompanyProductFromApi(id, dto);
      toast.success("Product updated");
      setEditedId(null);
      setEditForm(null);
      onReload();
    } catch (err: any) {
      toast.error(err?.response?.data?.detail || "Failed to update product");
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("Are you sure you want to delete this product?")) {
      return;
    }

    try {
      await deleteCompanyProductFromApi(id);
      toast.success("Product deleted");
      onReload();
    } catch (err: any) {
      toast.error(err?.response?.data?.detail || "Failed to delete product");
    }
  };

  const handleConvert = async (productId: number) => {
    const code = convertCode.trim().toUpperCase();
    if (!code) {
      toast.error("Enter currency code (e.g. USD)");
      return;
    }

    try {
      const res = await convertToAnotherCurrencyFromApi(productId, code);
      setConvertResult(res);
    } catch (err: any) {
      toast.error(
        err?.response?.data?.detail || "Failed to convert currency"
      );
    }
  };

  const totalPages =
    totalCount === 0 ? 1 : Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <>
      <div className="bg-white rounded-md border border-slate-200 overflow-hidden shadow-sm">
        <table className="min-w-full text-xs">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-500">
              <th className="px-3 py-2 w-6">
                <input type="checkbox" />
              </th>
              <th className="px-3 py-2 w-10 text-center">ID</th>
              <th className="px-3 py-2 w-16 text-left">Image</th>
              <th className="px-3 py-2 text-center">Name</th>
              <th className="px-3 py-2 text-center">EAN</th>
              <th className="px-3 py-2 text-center">Category</th>
              <th className="px-3 py-2 text-center">Price</th>
              <th className="px-3 py-2 text-center">Stock</th>
              <th className="px-3 py-2 text-center">IsAvailableForOrder</th>
              <th className="px-3 py-2 text-center w-28">Actions</th>
            </tr>
          </thead>

          <tbody>
            {(products ?? []).length === 0 ? (
              <tr>
                <td
                  colSpan={10}
                  className="px-3 py-6 text-center text-slate-500 text-sm"
                > 
                    no products.
                </td>
              </tr>
            ) : (
              products.map((p) => {
                const isEditing = editedId === p.companyProductId;
                const isConverting = convertRowId === p.companyProductId;

                return (
                  <React.Fragment key={p.companyProductId}>
                    <tr className="border-t border-slate-100 hover:bg-slate-50">
                      <td className="px-3 py-2">
                        <input type="checkbox" />
                      </td>
                      <td className="px-2 py-2 text-slate-700">
                        {p.companyProductId}
                      </td>
                      <td className="px-2 py-2">
                        <div className="w-10 h-10 rounded bg-slate-200 overflow-hidden flex items-center justify-center">
                          {p.image ? (
                            <img
                              src={p.image}
                              alt={p.companyProductName}
                              className="w-full h-full object-cover"
                            />
                          ) : (
                            <span className="text-[10px] text-slate-500">
                              none
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="px-3 py-2">
                        <div className="text-slate-800 text-xs font-medium">
                          {p.companyProductName}
                        </div>
                        {p.description && (
                          <div className="text-[11px] text-slate-500 truncate max-w-xs">
                            {p.description}
                          </div>
                        )}
                      </td>
                      <td className="px-3 py-2 text-slate-600">{p.ean}</td>
                      <td className="px-3 py-2 text-slate-600">
                        {p.category?.name}
                      </td>
                      <td className="px-3 py-2 text-right text-slate-700">
                        {p.price.amount} {p.price.currency.code}
                      </td>
                      <td className="px-3 py-2 text-right text-slate-700">
                        {p.stock}
                      </td>
                      <td className="px-3 py-2 text-center">
                        {p.isAvailableForOrder ? (
                          <span className="inline-flex items-center rounded-full bg-emerald-100 text-emerald-700 px-2 py-0.5 text-[10px] font-medium">
                            ✓
                          </span>
                        ) : (
                          <span className="inline-flex items-center rounded-full bg-red-100 text-red-700 px-2 py-0.5 text-[10px] font-medium">
                            ✕
                          </span>
                        )}
                      </td>
                      <td className="px-3 py-2 text-right">
                        <div className="inline-flex gap-1">
                          <button
                            onClick={() => startEdit(p)}
                            className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100"
                          >
                            ✏️
                          </button>
                          <button
                            onClick={() => handleDelete(p.companyProductId)}
                            className="px-2 py-1 rounded border border-red-300 text-[11px] text-red-700 hover:bg-red-50"
                          >
                            🗑
                          </button>
                          <button
                            onClick={() => {
                              setConvertRowId(p.companyProductId);
                              setConvertResult(null);
                              setConvertCode("");
                            }}
                            className="px-2 py-1 rounded border border-blue-300 text-[11px] text-blue-700 hover:bg-blue-50"
                          >
                            ⇄
                          </button>
                          <button
                            type="button"
                            onClick={() => onOpenHistory(p.companyProductId, p.companyProductName)}
                            className="px-2 py-1 rounded border border-slate-300 text-[11px] text-slate-700 hover:bg-slate-100"
                            title="Inventory movements"
                          >
                            📜
                          </button>
                        </div>
                      </td>
                    </tr>

                    {isEditing && editForm && (
                      <tr className="bg-white border-t border-slate-200">
                        <td colSpan={10} className="px-4 py-3">
                          <div className="space-y-2 text-xs">
                            <div className="grid grid-cols-3 gap-3">
                              <div>
                                <label className="block mb-1 font-medium text-slate-600">
                                  Product name
                                </label>
                                <input
                                  value={editForm.companyProductName}
                                  onChange={(e) =>
                                    handleEditChange(
                                      "companyProductName",
                                      e.target.value
                                    )
                                  }
                                  className="w-full rounded-md border border-slate-300 px-2 py-1"
                                />
                              </div>
                              <div>
                                <label className="block mb-1 font-medium text-slate-600">
                                  Category
                                </label>
                                <input
                                  value={editForm.category}
                                  onChange={(e) =>
                                    handleEditChange("category", e.target.value)
                                  }
                                  className="w-full rounded-md border border-slate-300 px-2 py-1"
                                />
                              </div>
                              <div>
                                <label className="block mb-1 font-medium text-slate-600">
                                  Stock
                                </label>
                                <input
                                  type="number"
                                  value={editForm.stock}
                                  onChange={(e) =>
                                    handleEditChange(
                                      "stock",
                                      Number(e.target.value)
                                    )
                                  }
                                  className="w-full rounded-md border border-slate-300 px-2 py-1"
                                />
                              </div>
                            </div>

                            <div className="grid grid-cols-3 gap-3">
                              <div>
                                <label className="block mb-1 font-medium text-slate-600">
                                  Price
                                </label>
                                <input
                                  type="number"
                                  step="0.01"
                                  value={editForm.amount}
                                  onChange={(e) =>
                                    handleEditChange(
                                      "amount",
                                      Number(e.target.value)
                                    )
                                  }
                                  className="w-full rounded-md border border-slate-300 px-2 py-1"
                                />
                              </div>
                              <div>
                                <label className="block mb-1 font-medium text-slate-600">
                                  Currency
                                </label>
                                <input
                                  value={editForm.currency}
                                  onChange={(e) =>
                                    handleEditChange("currency", e.target.value)
                                  }
                                  className="w-full rounded-md border border-slate-300 px-2 py-1"
                                />
                              </div>
                              <div>
                                <label className="block mb-1 font-medium text-slate-600">
                                  Image file
                                </label>
                                <input
                                  type="file"
                                  accept="image/*"
                                  onChange={(e) =>
                                    handleEditChange(
                                      "imageFile",
                                      e.target.files?.[0] ?? null
                                    )
                                  }
                                  className="w-full text-xs"
                                />
                              </div>
                            </div>

                            <div>
                              <label className="block mb-1 font-medium text-slate-600">
                                Description
                              </label>
                              <input
                                value={editForm.description}
                                onChange={(e) =>
                                  handleEditChange(
                                    "description",
                                    e.target.value
                                  )
                                }
                                className="w-full rounded-md border border-slate-300 px-2 py-1"
                              />
                            </div>

                            <div className="flex items-center gap-3">
                              <label className="inline-flex items-center">
                                <input
                                  type="checkbox"
                                  checked={editForm.isAvailableForOrder}
                                  onChange={(e) =>
                                    handleEditChange(
                                      "isAvailableForOrder",
                                      e.target.checked
                                    )
                                  }
                                  className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
                                />
                                <span className="ml-2">
                                  Available for order
                                </span>
                              </label>

                              <div className="ml-auto flex gap-2">
                                <button
                                  onClick={() => saveEdit(p.companyProductId)}
                                  className="px-3 py-1 rounded-md bg-blue-600 text-white text-xs font-medium hover:bg-blue-700"
                                >
                                  Save
                                </button>
                                <button
                                  onClick={() => {
                                    setEditedId(null);
                                    setEditForm(null);
                                  }}
                                  className="px-3 py-1 rounded-md bg-slate-300 text-slate-800 text-xs font-medium hover:bg-slate-400"
                                >
                                  Cancel
                                </button>
                              </div>
                            </div>
                          </div>
                        </td>
                      </tr>
                    )}

                    {isConverting && (
                      <tr className="bg-slate-50 border-t border-slate-200">
                        <td colSpan={10} className="px-4 py-3">
                          <div className="flex items-end gap-3 text-xs">
                            <div>
                              <label className="block mb-1 font-medium text-slate-600">
                                To currency code
                              </label>
                              <input
                                type="text"
                                value={convertCode}
                                onChange={(e) =>
                                  setConvertCode(e.target.value)
                                }
                                maxLength={3}
                                className="w-24 rounded-md border border-slate-300 px-2 py-1 uppercase"
                                placeholder="USD"
                              />
                            </div>

                            <button
                              onClick={() => handleConvert(p.companyProductId)}
                              className="px-3 py-1 rounded-md bg-blue-600 text-white text-xs font-medium hover:bg-blue-700"
                            >
                              Convert
                            </button>

                            {convertResult && (
                              <div className="text-xs text-slate-700 ml-2">
                                {p.price.amount} {p.price.currency.code} →{" "}
                                <span className="font-semibold">
                                  {convertResult.amount}{" "}
                                  {convertResult.currency.code}
                                </span>
                              </div>
                            )}
                          </div>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      <div className="mt-3 flex items-center justify-between text-xs text-slate-600">
        <div>
          Page {page} • {totalCount} items
        </div>
        <div className="flex gap-2">
          <button
            disabled={page <= 1}
            onClick={onPrevPage}
            className="px-3 py-1 rounded-md border border-slate-300 disabled:opacity-40"
          >
            Prev
          </button>
          <button
            disabled={page >= totalPages}
            onClick={onNextPage}
            className="px-3 py-1 rounded-md border border-slate-300 disabled:opacity-40"
          >
            Next
          </button>
        </div>
      </div>
    </>
  );
};

export default ProductsTable;