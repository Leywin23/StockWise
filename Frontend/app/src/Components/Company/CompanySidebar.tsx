import React from "react";

type View = "products" | "create" | "orders" | "history";

type Props = {
  activeView: View;
  onChangeView: (view: View) => void;
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
          onClick={() => onChangeView("orders")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "orders"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>📑</span>
          <span>Orders</span>
        </button>

        <button
          onClick={() => onChangeView("history")}
          className={
            "w-full text-left px-4 py-2 text-sm flex items-center gap-2 " +
            (activeView === "history"
              ? "bg-slate-900 text-white"
              : "text-slate-700 hover:bg-slate-100")
          }
        >
          <span>🕓</span>
          <span>History</span>
        </button>
      </nav>
    </aside>
  );
};

export default CompanySidebar;
