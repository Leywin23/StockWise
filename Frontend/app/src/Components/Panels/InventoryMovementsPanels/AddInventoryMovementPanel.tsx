import React, { useEffect, useMemo, useState } from "react";
import {
  InventoryMovementDto,
  MovementType,
  postInventoryMovementFromApi,
} from "../../../api/inventoryManagment";
import { toast } from "react-toastify";

type Props = {
  defaultProductId?: number | null;
  productName?: string | null;
  onCreated?: (created: InventoryMovementDto) => void;

  open?: boolean;
  onClose?: () => void;
};


const toLocalInputValue = (d: Date) => {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(
    d.getHours()
  )}:${pad(d.getMinutes())}`;
};

const AddInventoryMovementPanel: React.FC<Props> = ({
  defaultProductId,
  onCreated,
}) => {
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState<InventoryMovementDto>({
    companyProductId: defaultProductId ?? 0,
    date: new Date(),
    type: MovementType.Inbound,
    quantity: 1,
    comment: null,
  });

  useEffect(() => {
    if (defaultProductId) {
      setForm((prev) => ({ ...prev, companyProductId: defaultProductId }));
    }
  }, [defaultProductId]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setForm((prev) => ({
      ...prev,
      [name]:
        name === "companyProductId" ||
        name === "quantity" ||
        name === "type"
          ? Number(value)
          : value,
    }));
  };

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!form.companyProductId || form.companyProductId <= 0) {
      toast.error("Product is required");
      return;
    }

    if (form.quantity <= 0) {
      toast.error("Quantity must be greater than 0");
      return;
    }

    try {
      setSaving(true);
      const created = await postInventoryMovementFromApi(form);
      toast.success("Inventory movement added");
      onCreated?.(created);

      setForm((prev) => ({
        ...prev,
        quantity: 1,
        comment: null,
        date: new Date(),
      }));
    } catch (err: any) {
      toast.error(err?.response?.data?.detail || "Failed to add movement");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="flex justify-center">
      <div className="w-full max-w-xl">
        <div className="bg-white rounded-2xl shadow-md border border-slate-200 p-8">
          <div className="mb-6 text-center">
            <div className="text-xs uppercase tracking-wide text-slate-400">
              Inventory
            </div>
            <h2 className="text-2xl font-semibold text-slate-800">
              Add movement
            </h2>
            <p className="text-sm text-slate-500 mt-1">
              Register inbound or outbound stock change
            </p>
          </div>

          <form onSubmit={submit} className="space-y-5">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Product ID
              </label>
              <input
                name="companyProductId"
                type="number"
                min={1}
                value={form.companyProductId}
                onChange={handleChange}
                className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">
                  Movement type
                </label>
                <select
                  name="type"
                  value={form.type}
                  onChange={handleChange}
                  className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                >
                  <option value={MovementType.Inbound}>Inbound</option>
                  <option value={MovementType.Outbound}>Outbound</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">
                  Quantity
                </label>
                <input
                  name="quantity"
                  type="number"
                  min={1}
                  value={form.quantity}
                  onChange={handleChange}
                  className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Date
              </label>
              <input
                type="datetime-local"
                value={toLocalInputValue(new Date(form.date))}
                onChange={(e) =>
                  setForm((prev) => ({
                    ...prev,
                    date: new Date(e.target.value),
                  }))
                }
                className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Comment
              </label>
              <input
                name="comment"
                value={form.comment ?? ""}
                onChange={handleChange}
                placeholder="Optional"
                className="w-full rounded-lg border border-slate-300 px-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none"
              />
            </div>
            <button
              type="submit"
              disabled={saving}
              className="w-full rounded-lg bg-blue-600 py-3 text-sm font-semibold text-white hover:bg-blue-700 transition disabled:opacity-60"
            >
              {saving ? "Saving..." : "Add movement"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
export default AddInventoryMovementPanel;