import React, { useEffect, useMemo, useState } from "react";
import {
  ApproveWorkerFromApi,
  CompanyMembershipStatus,
  CompanyRole,
  getAllCompanyWorkers,
  SuspendWorkerFromCompany,
  UnsuspendWorkerFromApi,
  WorkerDto,
  WorkerQueryParams,
  WorkersSortBy,
} from "../api/accountApi";
import { toast } from "react-toastify";
import { useAuth } from "../../../app/core/context/AuthContext";
import { PageResult } from "../../products/api/companyProducts";

type Props = {};

const ManagerPanel: React.FC<Props> = () => {
  const { isLoggedIn } = useAuth();

  const [companyWorkers, setCompanyWorkers] = useState<WorkerDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [sortedBy, setSortedBy] = useState<WorkersSortBy>(
    WorkersSortBy.CompanyMembershipStatus
  );
  const [sortDir, setSortDir] = useState<number>(1); 

  const totalPages = useMemo(() => {
    return totalCount === 0 ? 1 : Math.max(1, Math.ceil(totalCount / pageSize));
  }, [totalCount, pageSize]);

  const loadWorkers = async () => {
    try {
      const query: WorkerQueryParams = {
        page,
        pageSize,
        sortedBy,
        sortDir,
      };

      const pageResult: PageResult<WorkerDto> | null = await getAllCompanyWorkers(
        query
      );

      if (!pageResult) {
        setCompanyWorkers([]);
        setTotalCount(0);
        return;
      }

      setCompanyWorkers(pageResult.items);
      setTotalCount(pageResult.totalCount);
    } catch (error: any) {
      console.error("Error loading workers", error);
      toast.error("Failed to load workers");
    }
  };

  useEffect(() => {
    if (!isLoggedIn) return;
    loadWorkers();
  }, [isLoggedIn, page, pageSize, sortedBy, sortDir]);

  const handleApproveWorker = async (userId: string) => {
    try {
      const approveWorkerName = await ApproveWorkerFromApi(userId);
      await loadWorkers();
      toast.success(`Worker ${approveWorkerName} has been approved`);
    } catch (err: any) {
      console.log(err, "Problem with approving worker occured");
      toast.error("Problem with approving worker");
    }
  };

  const handleSuspendWorker = async (userId: string) => {
    try {
      const suspendedWorker = await SuspendWorkerFromCompany(userId);
      await loadWorkers();
      toast.success(`${suspendedWorker}`);
    } catch (err: any) {
      console.log(err, "Problem with suspending worker occured");
      toast.error("Problem with suspending worker");
    }
  };

  const handleUnsuspendWorker = async (userId: string) => {
    try {
      const unsuspendedWorker = await UnsuspendWorkerFromApi(userId);
      await loadWorkers();
      toast.success(`${unsuspendedWorker}`);
    } catch (err: any) {
      console.log(err, "Problem with unsuspending user occured");
      toast.error("Problem with unsuspending worker");
    }
  };

  if (!isLoggedIn) return null;

  return (
    <div className="bg-white rounded-md border border-slate-200 overflow-hidden shadow-sm">
      <div className="mb-4 flex flex-wrap items-end gap-4 text-xs bg-white p-3 border-b border-slate-200">
        <div>
          <label className="block font-medium text-slate-600 mb-1">
            Sorted by
          </label>
          <select
            value={sortedBy}
            onChange={(e) => {
              setSortedBy(Number(e.target.value) as WorkersSortBy);
              setPage(1);
            }}
            className="rounded-md border border-slate-300 px-2 py-1"
          >
            {Object.keys(WorkersSortBy)
              .filter((k) => isNaN(Number(k)))
              .map((k) => (
                <option key={k} value={(WorkersSortBy as any)[k]}>
                  {k}
                </option>
              ))}
          </select>
        </div>

        <div>
          <label className="block font-medium text-slate-600 mb-1">
            Direction
          </label>
          <select
            value={sortDir}
            onChange={(e) => {
              setSortDir(Number(e.target.value));
              setPage(1);
            }}
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

        <div className="ml-auto flex items-center gap-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-50 disabled:opacity-50"
          >
            Prev
          </button>
          <span className="text-[11px] text-slate-600">
            Page {page} / {totalPages} • {totalCount} workers
          </span>
          <button
            onClick={() => setPage((p) => (p < totalPages ? p + 1 : p))}
            disabled={page >= totalPages}
            className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-50 disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>

      <table className="min-w-full text-xs">
        <thead>
          <tr className="bg-slate-50 border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-500">
            <th className="px-3 py-2 text-left">Name</th>
            <th className="px-3 py-2 text-left">Email</th>
            <th className="px-3 py-2 text-left">Role</th>
            <th className="px-3 py-2 text-left">Company membership</th>
            <th className="px-3 py-2 text-center w-28">Actions</th>
          </tr>
        </thead>

        <tbody>
          {(companyWorkers ?? []).length === 0 ? (
            <tr>
              <td colSpan={5} className="px-3 py-6 text-center text-slate-500">
                No workers
              </td>
            </tr>
          ) : (
            companyWorkers.map((w) => (
              <tr key={w.id} className="border-t border-slate-100 hover:bg-slate-50">
                <td className="px-3 py-2">{w.name}</td>
                <td className="px-3 py-2">{w.email}</td>
                <td className="px-3 py-2">{w.role}</td>
                <td className="px-3 py-2">
                  {CompanyMembershipStatus[w.companyMembershipStatus]}
                </td>

                <td className="px-3 py-2">
                  <div className="flex justify-center gap-2">
                    {w.companyMembershipStatus === CompanyMembershipStatus.Pending && (
                      <button
                        title="Approve worker"
                        onClick={() => handleApproveWorker(w.id)}
                        className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100"
                      >
                        ✓
                      </button>
                    )}

                    {w.role !== "Manager" &&
                      w.companyMembershipStatus === CompanyMembershipStatus.Approved && (
                        <button
                          title="Suspend worker"
                          onClick={() => handleSuspendWorker(w.id)}
                          className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100"
                        >
                          ❌
                        </button>
                      )}

                    {w.companyMembershipStatus === CompanyMembershipStatus.Suspended && (
                      <button
                        title="Unsuspend worker"
                        onClick={() => handleUnsuspendWorker(w.id)}
                        className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100"
                      >
                        🠉
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
};

export default ManagerPanel;
