import React, { useState } from 'react';
import { Product,ProductDto, UpdateProductApi, DeleteProductApi } from '../../api';

interface Props {
  products: ProductDto[];
  onProductsChange: () => void;
}

export default function ProductList({ products, onProductsChange }: Props) {
  const [isEdited, setIsEdited] = useState(false);
  const [editedProduct, setEditedProduct] = useState<Product | null>(null);

  const handleDelete = async (id: number) => {
    try {
      await DeleteProductApi(id);
      onProductsChange();
    } catch (err) {
      console.error("Error deleting product", err);
    }
  };

  const handleEdit = (product: Product) => {
    setIsEdited(true);
    setEditedProduct(product);
  };

  const handleEditChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setEditedProduct(prev =>
      prev ? { ...prev, [name]: value } : null
    );
  };

  const editProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editedProduct || !editedProduct.category || !editedProduct.category.categoryId) {
      alert("Missing category ID for update.");
      return;
    }

    const payload = {
      productId: editedProduct.productId,
      productName: editedProduct.productName,
      ean: editedProduct.ean,
      image: editedProduct.image,
      description: editedProduct.description,
      shoppingPrice: editedProduct.shoppingPrice,
      sellingPrice: editedProduct.sellingPrice,
      categoryId: editedProduct.category.categoryId
    };

    try {
      await UpdateProductApi(payload);
      setIsEdited(false);
      setEditedProduct(null);
      onProductsChange();
    } catch (err) {
      console.error("Error updating product:", err);
    }
  };

  return (
    <section>
      <h2>Available Products</h2>
      <ul>
        {products.map(p => (
          <li key={p.id}>
            <strong>{p.productName}</strong>
            <p>{p.ean}</p>
            <p>${p.stock}</p>
            <br />
            <button onClick={() => handleDelete(p.id)}>Delete</button>
            

            {isEdited && editedProduct?.productId === p.id && (
              <form onSubmit={editProduct}>
                <input name="productName" value={editedProduct.productName} onChange={handleEditChange} />
                <input name="ean" value={editedProduct.ean} onChange={handleEditChange} />
                <input name="image" value={editedProduct.image} onChange={handleEditChange} />
                <input name="description" value={editedProduct.description} onChange={handleEditChange} />
                <input type="number" name="shoppingPrice" value={editedProduct.shoppingPrice} onChange={handleEditChange} />
                <input type="number" name="sellingPrice" value={editedProduct.sellingPrice} onChange={handleEditChange} />
                <input name="category" value={editedProduct.category?.name ?? ''} readOnly />
                <button type="submit">Save</button>
              </form>
            )}
          </li>
        ))}
      </ul>
    </section>
  );
}
