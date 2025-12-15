import React, { useEffect, useState } from "react";
import {
  companyProductDto,
  getCompanyProductFromApi,
  CompanyProductQueryParams,
  PageResult,
} from "../api/companyProducts";
import { useAuth } from "../context/AuthContext";
import { toast } from "react-toastify";

import CompanySidebar from "../Components/Company/CompanySidebar";
import ProductFiltersBar from "../Components/Company/ProductFiltersBar";
import ProductsTable from "../Components/Company/ProductsTable";
import CreateProductForm from "../Components/Company/CreateProductForm";
import OrdersPanel from "../Components/Panels/CreateOrderPanel";
import { CompanyView } from "../Components/Company/CompanyView";
import OrdersListPanel from "../Components/Panels/OrdersListPanel";
import CreateOrderPanel from "../Components/Panels/CreateOrderPanel";



const CompanyProductsPage: React.FC = () => {
  const { isLoggedIn } = useAuth(); 

  const [activeView, setActiveView] = useState<CompanyView>("products");

  const [companyProducts, setCompanyProducts] = useState<companyProductDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [stockFilter, setStockFilter] = useState<number | undefined>(undefined);
  const [availableOnly, setAvailableOnly] = useState<boolean | undefined>(
    undefined
  );
  const [minTotal, setMinTotal] = useState<number | undefined>(undefined);
  const [maxTotal, setMaxTotal] = useState<number | undefined>(undefined);
  const [sortedBy, setSortedBy] = useState<string>("stock");
  const [sortDir, setSortDir] = useState<number>(1); 

  const loadCompanyProducts = async () => {
    try {
      const query: CompanyProductQueryParams = {
        page,
        pageSize,
        stock: stockFilter,
        isAvailableForOrder: availableOnly,
        minTotal,
        maxTotal,
        sortedBy,
        sortDir,
      };

      const pageResult: PageResult<companyProductDto> | null =
        await getCompanyProductFromApi(query);

      if (!pageResult) {
        setCompanyProducts([]);
        setTotalCount(0);
        return;
      }

      setCompanyProducts(pageResult.items);
      setTotalCount(pageResult.totalCount);
    } catch (error: any) {
      console.error("Error loading company products:", error);
      toast.error("Failed to load products");
    }
  };

  useEffect(() => {
    if (!isLoggedIn) return;
    loadCompanyProducts();
  }, [
    isLoggedIn,
    page,
    pageSize,
    stockFilter,
    availableOnly,
    minTotal,
    maxTotal,
    sortedBy,
    sortDir,
  ]);

  if (!isLoggedIn) {
    return null;
  }

  const totalPages =
    totalCount === 0 ? 1 : Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <div className="min-h-screen bg-slate-100">
      <div className="w-full px-6 py-6 flex gap-6">
        <CompanySidebar activeView={activeView} onChangeView={setActiveView} />

        <main className="flex-1">
          {activeView === "products" && (
            <>
              <div className="flex items-center justify-between mb-4">
                <div>
                  <div className="text-xs text-slate-500 mb-1">
                    Katalog / Produkty
                  </div>
                  <h1 className="text-2xl font-semibold text-slate-800">
                    Products
                  </h1>
                </div>

                <button
                  onClick={() => setActiveView("create")}
                  className="inline-flex items-center rounded-md bg-emerald-500 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600"
                >
                  + New product
                </button>
              </div>

              <ProductFiltersBar
                minTotal={minTotal}
                maxTotal={maxTotal}
                stockFilter={stockFilter}
                availableOnly={availableOnly}
                sortedBy={sortedBy}
                sortDir={sortDir}
                pageSize={pageSize}
                setMinTotal={setMinTotal}
                setMaxTotal={setMaxTotal}
                setStockFilter={setStockFilter}
                setAvailableOnly={setAvailableOnly}
                setSortedBy={setSortedBy}
                setSortDir={setSortDir}
                setPageSize={setPageSize}
                setPage={setPage}
              />

              <ProductsTable
                products={companyProducts}
                page={page}
                pageSize={pageSize}
                totalCount={totalCount}
                onPrevPage={() => setPage((p) => Math.max(1, p - 1))}
                onNextPage={() =>
                  setPage((p) => (p < totalPages ? p + 1 : p))
                }
                onReload={loadCompanyProducts}
              />
            </>
          )}

          {activeView === "create" && (
            <CreateProductForm
              onCreated={() => {
                setActiveView("products");
                setPage(1);
                loadCompanyProducts();
              }}
            />
          )}

          {activeView === "orderlist" && (
            <OrdersListPanel/>
          )}
          {activeView === "creteOrders" && (
            <CreateOrderPanel/>
          )}

          {activeView === "history" && (
            <div className="bg-white rounded-xl shadow-sm p-6 border border-slate-200 text-sm text-slate-700">
              History view – tu możesz dodać historię, logi itd.
            </div>
          )}
        </main>
      </div>
    </div>
  );
};

export default CompanyProductsPage;
