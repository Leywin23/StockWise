import React, { useEffect, useState } from 'react'
import { ApproveWorkerFromApi, CompanyMembershipStatus, getAllCompanyWorkers, SuspendWorkerFromCompany, UnsuspendWorkerFromApi, WorkerDto } from '../api/accountApi'
import { toast } from 'react-toastify';

type Props = {}

const ManagerPanel = (props: Props) => {
    const[companyWorkers, setCompanyWorkers] = useState<WorkerDto[]>([]);

    const getAllWorkers = async ()=>{
        try{
            const workers = await getAllCompanyWorkers();
            setCompanyWorkers(workers);
        }catch(err:any){
            console.log(err, "Unable to find workers in you company");
        }
    }
    useEffect(()=>{
        getAllWorkers()
    },[]);

    const handleApproveWorker = async(userId:string)=>{
        try{
            const approveWorkerName = await ApproveWorkerFromApi(userId);
            await getAllWorkers();
            toast.success(`Worker ${approveWorkerName} has been approved`);
        }catch(err:any){
            console.log(err,"Problem with approving worker occured");
        }
    };

    const handleSuspendWorker = async(userId: string)=>{
        try{
            const suspendedWorker = await SuspendWorkerFromCompany(userId);
            await getAllWorkers();
            toast.success(`Worker ${suspendedWorker} `);
        }
        catch(err:any){
            console.log(err, "Problem with suspending worker occuerd");
        }
    }

    const handleUnsuspendWorker = async(userId:string)=>{
        try{
            const unsuspendedWorker = await UnsuspendWorkerFromApi(userId);
            await getAllWorkers();
            toast.success(`Worker ${unsuspendedWorker} `);
        }catch(err: any){
            console.log(err, "Problem with unsuspending user occured");
        }
    }

  return (
    <div className="bg-white rounded-md border border-slate-200 overflow-hidden shadow-sm">
        <table className="min-w-full text-xs">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-500">
              <th className="px-3 py-2 text-center">Name</th>
              <th className="px-3 py-2 text-center">EMAIL</th>
              <th className="px-3 py-2 text-center">ROLE</th>
              <th className="px-3 py-2 text-center">COMPANY MEMBERSHIP</th>
              <th className="px-3 py-2 text-center w-28">Actions</th>
            </tr>
          </thead>
          <tbody>
            {(companyWorkers ?? []).length === 0 ? (
                <tr>
                    <td>
                        No workers
                    </td>
                </tr>
            ): companyWorkers.map((w)=>{
                return (
                    <React.Fragment key = {w.id}>
                        <tr className="border-t border-slate-100 hover:bg-slate-50">
                            <td className="px-3 py-2">
                                {w.name}
                            </td>
                            <td className="px-3 py-2">
                                {w.email}
                            </td>
                            <td className="px-3 py-2">
                                {w.role}
                            </td>
                            <td className="px-3 py-2">
                                {CompanyMembershipStatus[w.companyMembershipStatus]}
                            </td>
                            
                            <td>
                                <div>
                                    {w.companyMembershipStatus === CompanyMembershipStatus.Pending && (
                                    <button
                                        title='Approve worker'
                                        onClick={() => handleApproveWorker(w.id)}
                                        className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100"
                                    >
                                        ✓
                                    </button>
                                    )}
                                    {w.role !== 'Manager' &&  w.companyMembershipStatus === CompanyMembershipStatus.Approved &&(
                                    <button
                                        title='Suspend worker'
                                        onClick={()=> handleSuspendWorker(w.id)}
                                        className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100"
                                    >
                                        ❌
                                    </button>
                                    )}
                                    {w.companyMembershipStatus === CompanyMembershipStatus.Suspended && (
                                        <button title='Unsuspend worker'
                                        onClick={()=> handleUnsuspendWorker(w.id)}
                                        className="px-2 py-1 rounded border border-slate-300 text-[11px] hover:bg-slate-100">
                                            🠉
                                        </button>
                                    )}
                                </div>
                            </td>
                            <td>

                            </td>
                        </tr>
                    </React.Fragment>
                )
            })}
          </tbody>
        </table>
    </div>
  )
}
export default ManagerPanel;