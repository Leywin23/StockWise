import React from "react";
import { CompanyView } from "../../../Components/Company/CompanyView";

type Props = {
  activeView: CompanyView;
  onChangeView: (view: CompanyView) => void;
};

const CompanySidebar: React.FC<Props> = ({ activeView, onChangeView }) => {
  return (
    <aside className="w-64 bg-white border border-slate-200 rounded-lg shadow-sm flex flex-col">
      <div className="px-4 py-3 border-b border-slate-200">
        <div className="text-xs text-slate-500 mb-1">Panel</div>
        <div className="text-sm font-semibold text-slate-800">Company</div>
      </div>

      <nav className="flex-1 py-2">
        <button
          onClick={() => onChangeView("products")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "products"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>📦</span>
          <span>Products</span>
        </button>

        <button
          onClick={() => onChangeView("create")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "create"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>➕</span>
          <span>Add new product</span>
        </button>

        <button
          onClick={() => onChangeView("available")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "available"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>🔍</span>
          <span>Check available products to order</span>
        </button>

        <button
          onClick={() => onChangeView("orderlist")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "orderlist"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>📑</span>
          <span>Orders</span>
        </button>

        <button
          onClick={() => onChangeView("creteOrders")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "creteOrders"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>🛒</span>
          <span>Create order</span>
        </button>

        <button
          onClick={() => onChangeView("addMovement")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "addMovement"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>🕓</span>
          <span>Add new movement</span>
        </button>
      </nav>
    </aside>
  );
};

export default CompanySidebar;
