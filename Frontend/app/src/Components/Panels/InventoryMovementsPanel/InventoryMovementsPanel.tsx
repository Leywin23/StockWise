import { useEffect, useMemo, useState } from "react";
import { getInventoryMovementsFromApi, InventoryMovementDto, MovementType } from "../../../api/inventoryManagment";
import { toast } from "react-toastify";

type Props = {
    productId: number | null;
    productName : string | null;
    onClose?: () => void;
};

const InventoryMovementsPanel : React.FC<Props> = ({productId, productName, onClose}) =>{
    const [ items, setItems] = useState<InventoryMovementDto[]>([]);
    const [ loading, setLoading ] = useState(false);

    const load = async () => {
        if(!productId){
            setItems([]);
            return;
        }
        try{
            setLoading(true);
            const res = await getInventoryMovementsFromApi(productId);
            setItems(Array.isArray(res) ? res : []);
        }catch(err:any){
            console.error(err?.response?.data ?? err);
            toast.error("Failed to load inventory movements");
            setItems([]);
        }finally{
            setLoading(false);
        }
    };

    useEffect(()=> {
        load();
    }, [productId])

    const header = useMemo(()=>{
        if(!productId && !productName) return "Movements";
        return `Movements for product #${productId} ${productName}`;
    },[productId]);

      return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="p-4 border-b border-slate-200 flex items-center justify-between">
            <div>
            <div className="text-lg font-semibold text-slate-800">{header}</div>
            </div>

            <div className="flex gap-2">
            <button
                onClick={load}
                className="rounded-md bg-slate-900 px-3 py-2 text-xs font-medium text-white hover:bg-slate-800"
            >
                Refresh
            </button>
            {onClose && (
                <button
                onClick={onClose}
                className="rounded-md bg-slate-200 px-3 py-2 text-xs font-medium hover:bg-slate-300"
                >
                Close
                </button>
            )}
            </div>
        </div>

        {!productId ? (
            <div className="p-6 text-sm text-slate-500">
            Select a product to see its movement history.
            </div>
        ) : loading ? (
            <div className="p-6 text-sm text-slate-500">Loading…</div>
        ) : items.length === 0 ? (
            <div className="p-6 text-sm text-slate-500">No movements.</div>
        ) : (
            <div className="overflow-auto">
            <table className="min-w-full text-sm">
                <thead>
                <tr className="bg-slate-50 border-b border-slate-200 text-[11px] uppercase tracking-wide text-slate-500">
                    <th className="px-2 py-2 text-left">Type</th>
                    <th className="px-2 py-2 text-left">Date</th>
                    <th className="px-2 py-2 text-left">Qty</th>
                    <th className="px-2 py-2 text-left">Comment</th>
                </tr>
                </thead>
                <tbody>
                {items.map((m, idx) => {
                    const key = `${m.companyProductId}-${new Date(m.date).toISOString()}-${m.type}-${m.quantity}-${m.comment ?? ""}`;
                    return (
                    <tr key={key || idx} className="border-t border-slate-100 hover:bg-slate-50">
                        <td className="px-1 py-2 text-left" >
                        {m.type === MovementType.Inbound ? (
                            <span className="inline-flex items-center rounded-full bg-emerald-100 text-emerald-700 px-2 py-0.5 text-xs font-medium">
                            Inbound
                            </span>
                        ) : (
                            <span className="inline-flex items-center rounded-full bg-rose-100 text-rose-700 px-2 py-0.5 text-xs font-medium">
                            Outbound
                            </span>
                        )}
                        </td>
                        <td className="px-3 py-2 text-slate-700 text-left">
                        {new Date(m.date).toLocaleString()}
                        </td>
                        <td className="px-4 py-2 font-medium text-slate-800 text-left">
                        {m.quantity}
                        </td>
                        <td className="px-4 py-2 text-slate-600 text-left">{m.comment ?? "—"}</td>
                    </tr>
                    );
                })}
                </tbody>
            </table>
            </div>
        )}
        </div>
    );
};

export default InventoryMovementsPanel;
