import React, { useEffect, useMemo, useState } from "react";
import {
  CompanyProductsAvailableQueryParams,
  CompanyProductWithCompanyDto,
  CompanyProductSortBy,
  getAllAvailableCompanyProductsFromApi,
  PageResult,
} from "../../../api/companyProducts";
import { useAuth } from "../../../context/AuthContext";
import { toast } from "react-toastify";

type Props = {
  onCreateOrder: (offer: CompanyProductWithCompanyDto) => void;
};

const AvailableForOdresProductsListPanel: React.FC<Props> = ({ onCreateOrder }) => {
  const { isLoggedIn } = useAuth();

  const [availableProductList, setAvailableProductList] = useState<CompanyProductWithCompanyDto[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  const [stockFilter, setStockFilter] = useState<number | undefined>(undefined);
  const [minTotal, setMinTotal] = useState<number | undefined>(undefined);
  const [maxTotal, setMaxTotal] = useState<number | undefined>(undefined);

  const [search, setSearch] = useState("");

  // ✅ enum zgodny z kontraktem
  const [sortedBy, setSortedBy] = useState<CompanyProductSortBy>(CompanyProductSortBy.Stock);

  // ✅ backend przyjmuje tylko 0/1 (NIE -1)
  // przyjmujemy: 0 = Desc, 1 = Asc (tak jak miałeś wcześniej)
  const [sortDir, setSortDir] = useState<number>(0);

  const loadAvailableProducts = async () => {
    try {
      const query: CompanyProductsAvailableQueryParams = {
        page,
        pageSize,
        stock: stockFilter,
        minTotal,
        maxTotal,
        sortedBy,
        sortDir, // ✅ 0/1
      };

      const pageResult: PageResult<CompanyProductWithCompanyDto> | null =
        await getAllAvailableCompanyProductsFromApi(query);

      if (!pageResult) {
        setAvailableProductList([]);
        setTotalCount(0);
        return;
      }

      setAvailableProductList(pageResult.items ?? []);
      setTotalCount(pageResult.totalCount ?? 0);
    } catch (error: any) {
      console.error("Error loading available products:", error);
      const apiError = error?.response?.data;
      toast.error(apiError?.detail || apiError?.title || "Failed to load offers");
      setAvailableProductList([]);
      setTotalCount(0);
    }
  };

  useEffect(() => {
    if (!isLoggedIn) return;
    loadAvailableProducts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isLoggedIn, page, pageSize, stockFilter, minTotal, maxTotal, sortedBy, sortDir]);

  const totalPages = totalCount === 0 ? 1 : Math.max(1, Math.ceil(totalCount / pageSize));

  const onChangeNumber =
    (setter: (v: number | undefined) => void) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const raw = e.target.value;
      if (!raw) {
        setter(undefined);
        setPage(1);
        return;
      }
      const n = Number(raw);
      setter(Number.isFinite(n) ? n : undefined);
      setPage(1);
    };

  const onChangePageSize = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setPageSize(Number(e.target.value));
    setPage(1);
  };

  // ✅ lokalny filtr: productName + companyName + ean
  const filteredOffers = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return availableProductList;

    return (availableProductList ?? []).filter((p) => {
      const productName = (p.companyProductName ?? "").toLowerCase();
      const companyName = (p.company?.name ?? "").toLowerCase();
      const ean = (p.ean ?? "").toLowerCase();
      return productName.includes(q) || companyName.includes(q) || ean.includes(q);
    });
  }, [availableProductList, search]);

  if (!isLoggedIn) return null;

  return (
    <div className="min-h-screen bg-slate-100">
      <div className="w-full px-6 py-6">
        {/* Header */}
        <div className="flex items-start justify-between gap-4 mb-4">
          <div>
            <div className="text-xs text-slate-500 mb-1">Market / Offers</div>
            <h1 className="text-2xl font-semibold text-slate-800">Available products</h1>
            <div className="text-xs text-slate-500 mt-1">
              Browse offers from other companies
            </div>
          </div>

          <button
            onClick={loadAvailableProducts}
            className="rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800"
          >
            Refresh
          </button>
        </div>

        {/* Filters + Search */}
        <div className="bg-white rounded-md border border-slate-200 shadow-sm p-4 mb-4">
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-3 items-end">
            {/* Search */}
            <div className="lg:col-span-3">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">
                Search (product / company / EAN)
              </label>
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </div>

            <div className="lg:col-span-2">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">Min total</label>
              <input
                type="number"
                step="0.01"
                value={minTotal ?? ""}
                onChange={onChangeNumber(setMinTotal)}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </div>

            <div className="lg:col-span-2">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">Max total</label>
              <input
                type="number"
                step="0.01"
                value={maxTotal ?? ""}
                onChange={onChangeNumber(setMaxTotal)}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </div>

            <div className="lg:col-span-2">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">Min stock</label>
              <input
                type="number"
                value={stockFilter ?? ""}
                onChange={onChangeNumber(setStockFilter)}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </div>

            <div className="lg:col-span-2">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">Sorted by</label>
              <select
                value={sortedBy}
                onChange={(e) => {
                  setSortedBy(Number(e.target.value));
                  setPage(1);
                }}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              >
                <option value={CompanyProductSortBy.Stock}>Stock</option>
                <option value={CompanyProductSortBy.Price}>Price</option>
                <option value={CompanyProductSortBy.CompanyName}>Company</option>
                <option value={CompanyProductSortBy.CategoryName}>Category</option>
              </select>
            </div>

            <div className="lg:col-span-1">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">Dir</label>
              <select
                value={sortDir}
                onChange={(e) => {
                  setSortDir(Number(e.target.value));
                  setPage(1);
                }}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              >
                <option value={0}>Desc</option>
                <option value={1}>Asc</option>
              </select>
            </div>

            <div className="lg:col-span-1">
              <label className="block text-[11px] font-medium text-slate-600 mb-1">Page size</label>
              <select
                value={pageSize}
                onChange={onChangePageSize}
                className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              >
                <option value={5}>5</option>
                <option value={10}>10</option>
                <option value={20}>20</option>
              </select>
            </div>
          </div>
        </div>

        {/* Table */}
        <div className="bg-white rounded-md border border-slate-200 overflow-hidden shadow-sm">
          <table className="min-w-full text-xs">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-500">
                <th className="px-3 py-2 w-16 text-left">Image</th>
                <th className="px-3 py-2 text-center">Name</th>
                <th className="px-3 py-2 text-center">EAN</th>
                <th className="px-3 py-2 text-center">Category</th>
                <th className="px-3 py-2 text-center">Price</th>
                <th className="px-3 py-2 text-center">Stock</th>
                <th className="px-3 py-2 text-center">Seller</th>
                <th className="px-3 py-2 text-center w-28">Actions</th>
              </tr>
            </thead>

            <tbody>
              {filteredOffers.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-3 py-8 text-center text-slate-500 text-sm">
                    No offers.
                  </td>
                </tr>
              ) : (
                filteredOffers.map((p) => (
                  <tr
                    key={p.companyProductId}
                    className="border-t border-slate-100 hover:bg-slate-50"
                  >
                    <td className="px-2 py-2">
                      <div className="w-10 h-10 rounded bg-slate-200 overflow-hidden flex items-center justify-center">
                        {p.image ? (
                          <img
                            src={p.image}
                            alt={p.companyProductName}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <span className="text-[10px] text-slate-500">none</span>
                        )}
                      </div>
                    </td>

                    <td className="px-3 py-2">
                      <div className="text-slate-800 text-xs font-medium">{p.companyProductName}</div>
                      {p.description && (
                        <div className="text-[11px] text-slate-500 truncate max-w-xs mx-auto">
                            {p.description}
                        </div>

                      )}
                    </td>

                    <td className="px-3 py-2 text-center text-slate-600">{p.ean}</td>

                    <td className="px-3 py-2 text-center text-slate-600">
                      {p.categoryName ?? p.category?.name ?? "—"}
                    </td>

                    <td className="px-3 py-2 text-center text-slate-700">
                      {p.price?.amount} {p.price?.currency?.code ?? ""}
                    </td>

                    <td className="px-3 py-2 text-center text-slate-700">{p.stock}</td>

                    <td className="px-3 py-2 text-center text-slate-600">
                      {p.company?.name ?? "—"}
                    </td>

                    <td className="px-3 py-2 text-right">
                      <div className="inline-flex gap-1">
                        <button
                          type="button"
                          onClick={() => onCreateOrder(p)}
                          className="px-2 py-1 rounded border border-blue-300 text-[11px] text-blue-700 hover:bg-blue-50"
                          title="Create order from this offer"
                        >
                          🧾
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        <div className="mt-3 flex items-center justify-between text-xs text-slate-600">
          <div>
            Page {page} • {totalCount} items • Showing {filteredOffers.length}
          </div>
          <div className="flex gap-2">
            <button
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              className="px-3 py-1 rounded-md border border-slate-300 disabled:opacity-40"
            >
              Prev
            </button>
            <button
              disabled={page >= totalPages}
              onClick={() => setPage((p) => (p < totalPages ? p + 1 : p))}
              className="px-3 py-1 rounded-md border border-slate-300 disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AvailableForOdresProductsListPanel;
