import React from "react";

type Props = {
  minTotal?: number;
  maxTotal?: number;
  stockFilter?: number;
  availableOnly?: boolean;
  sortedBy: string;
  sortDir: number;
  pageSize: number;

  setMinTotal: (v: number | undefined) => void;
  setMaxTotal: (v: number | undefined) => void;
  setStockFilter: (v: number | undefined) => void;
  setAvailableOnly: (v: boolean | undefined) => void;
  setSortedBy: (v: string) => void;
  setSortDir: (v: number) => void;
  setPageSize: (v: number) => void;
  setPage: (v: number) => void;
};

const ProductFiltersBar: React.FC<Props> = ({
  minTotal,
  maxTotal,
  stockFilter,
  availableOnly,
  sortedBy,
  sortDir,
  pageSize,
  setMinTotal,
  setMaxTotal,
  setStockFilter,
  setAvailableOnly,
  setSortedBy,
  setSortDir,
  setPageSize,
  setPage,
}) => {
  return (
    <div className="mb-4 flex flex-wrap items-end gap-4 text-xs bg-white p-3 rounded-md border border-slate-200">
      <div>
        <label className="block font-medium text-slate-600 mb-1">
          Min total
        </label>
        <input
          type="number"
          value={minTotal ?? ""}
          onChange={(e) =>
            setMinTotal(e.target.value ? Number(e.target.value) : undefined)
          }
          className="w-24 rounded-md border border-slate-300 px-2 py-1"
        />
      </div>

      <div>
        <label className="block font-medium text-slate-600 mb-1">
          Max total
        </label>
        <input
          type="number"
          value={maxTotal ?? ""}
          onChange={(e) =>
            setMaxTotal(e.target.value ? Number(e.target.value) : undefined)
          }
          className="w-24 rounded-md border border-slate-300 px-2 py-1"
        />
      </div>

      <div>
        <label className="block font-medium text-slate-600 mb-1">
          Min stock
        </label>
        <input
          type="number"
          value={stockFilter ?? ""}
          onChange={(e) =>
            setStockFilter(e.target.value ? Number(e.target.value) : undefined)
          }
          className="w-24 rounded-md border border-slate-300 px-2 py-1"
        />
      </div>

      <div className="flex items-center gap-2 mt-4 sm:mt-0">
        <input
          id="availableOnly"
          type="checkbox"
          checked={!!availableOnly}
          onChange={(e) => setAvailableOnly(e.target.checked ? true : undefined)}
          className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
        />
        <label htmlFor="availableOnly" className="font-medium text-slate-600">
          Available only
        </label>
      </div>

      <div>
        <label className="block font-medium text-slate-600 mb-1">
          Sorted by
        </label>
        <select
          value={sortedBy}
          onChange={(e) => setSortedBy(e.target.value)}
          className="rounded-md border border-slate-300 px-2 py-1"
        >
          <option value="stock">Stock</option>
          <option value="price">Price</option>
          <option value="id">Id</option>
        </select>
      </div>

      <div>
        <label className="block font-medium text-slate-600 mb-1">
          Direction
        </label>
        <select
          value={sortDir}
          onChange={(e) => setSortDir(Number(e.target.value))}
          className="rounded-md border border-slate-300 px-2 py-1"
        >
          <option value={0}>Desc</option>
          <option value={1}>Asc</option>
        </select>
      </div>

      <div>
        <label className="block font-medium text-slate-600 mb-1">
          Page size
        </label>
        <select
          value={pageSize}
          onChange={(e) => {
            const newSize = Number(e.target.value);
            setPageSize(newSize);
            setPage(1);
          }}
          className="rounded-md border border-slate-300 px-2 py-1"
        >
          <option value={5}>5</option>
          <option value={10}>10</option>
          <option value={20}>20</option>
        </select>
      </div>
    </div>
  );
};

export default ProductFiltersBar;