import React, { useState } from 'react';
import { CreateProductDto, postProductToApi } from '../../api';

interface Props {
  onProductAdded: () => void;
}

export default function AddProductForm({ onProductAdded }: Props) {
  const [newProduct, setNewProduct] = useState<CreateProductDto>({
    productName: '',
    ean: '',
    image: '',
    description: '',
    shoppingPrice: 0,
    sellingPrice: 0,
    category: ''
  });

  const [message, setMessage] = useState('');

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setNewProduct(prev => ({ ...prev, [name]: value }));
  };

  const addProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const result = await postProductToApi(newProduct);
      if (result) {
        setMessage("Product added successfully!");
        setNewProduct({
          productName: '',
          ean: '',
          image: '',
          description: '',
          shoppingPrice: 0,
          sellingPrice: 0,
          category: ''
        });
        onProductAdded();
      }
    } catch (err) {
      console.error("Error adding product", err);
    }
  };

  return (
    <section>
      <h2>Add Product</h2>
      <form onSubmit={addProduct}>
        <input name="productName" value={newProduct.productName} onChange={handleChange} placeholder="Product Name" />
        <input name="ean" value={newProduct.ean} onChange={handleChange} placeholder="EAN" />
        <input name="image" value={newProduct.image} onChange={handleChange} placeholder="Image URL" />
        <input name="description" value={newProduct.description} onChange={handleChange} placeholder="Description" />
        <input type="number" name="shoppingPrice" value={newProduct.shoppingPrice} onChange={handleChange} placeholder="Shopping Price" />
        <input type="number" name="sellingPrice" value={newProduct.sellingPrice} onChange={handleChange} placeholder="Selling Price" />
        <input name="category" value={newProduct.category} onChange={handleChange} placeholder="Category" />
        <button type="submit">Add</button>
      </form>
      {message && <p>{message}</p>}
    </section>
  );
}
