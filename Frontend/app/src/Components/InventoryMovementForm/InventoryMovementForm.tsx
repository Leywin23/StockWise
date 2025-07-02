import React, { useState } from 'react';
import { InventoryMovementDto, AddMovementFromApi } from '../../api'; ;

export default function InventoryMovementForm() {
  const [movement, setMovement] = useState<InventoryMovementDto>({
    Date: new Date(),
    Type: "",
    ProductId: 0,
    Quantity: 0,
    Comment: ""
  });

  const handleMovementChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setMovement(prev => ({
      ...prev,
      [name]: name === 'ProductId' || name === 'Quantity' ? Number(value) : value
    }));
  };

  const handleAddMovement = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await AddMovementFromApi(movement);
      if (response) {
        alert("Movement Added Successfully");
        setMovement({
          Date: new Date(),
          Type: "",
          ProductId: 0,
          Quantity: 0,
          Comment: ""
        });
      }
    } catch (err) {
      console.error("Error adding movement", err);
    }
  };

  return (
    <section>
      <h2>Add Inventory Movement</h2>
      <form onSubmit={handleAddMovement}>
        <label>
          Product ID:
          <input
            type="number"
            name="ProductId"
            value={movement.ProductId}
            onChange={handleMovementChange}
            required
          />
        </label>
        <label>
          Type:
          <select name="Type" value={movement.Type} onChange={handleMovementChange}>
            <option value="">--Select Type--</option>
            <option value="inbound">Inbound</option>
            <option value="outbound">Outbound</option>
            <option value="adjustment">Adjustment</option>
          </select>
        </label>
        <label>
          Quantity:
          <input
            type="number"
            name="Quantity"
            value={movement.Quantity}
            onChange={handleMovementChange}
            required
          />
        </label>
        <label>
          Comment:
          <input
            type="text"
            name="Comment"
            value={movement.Comment}
            onChange={handleMovementChange}
          />
        </label>
        <button type="submit">Add Movement</button>
      </form>
    </section>
  );
}
