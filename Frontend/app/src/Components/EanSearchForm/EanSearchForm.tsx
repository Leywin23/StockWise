import React, { useState } from 'react';
import { CreateProductDto, getProductsByEanFromApi, postProductToApi} from '../../api'; 

export default function EanSearchForm({ onProductAdded }: { onProductAdded: () => void }) {
  const [eanSearch, setEanSearch] = useState('');
  const [eanProduct, setEanProduct] = useState<CreateProductDto>({
    productName: '',
    ean: '',
    image: '',
    description: '',
    shoppingPrice: 0,
    sellingPrice: 0,
    category: ''
  });

  const handleEanChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setEanProduct(prev => ({ ...prev, [name]: value }));
  };

  const handleEanSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!eanSearch) return;
    try {
      const product = await getProductsByEanFromApi(eanSearch);
      if (!product || !product.category) return;

      const { productId, category, ...rest } = product;
      const productDto: CreateProductDto = { ...rest, category: category.name };
      setEanProduct(productDto);
    } catch (err) {
      console.error("Error fetching EAN product:", err);
    }
  };

  const addProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const result = await postProductToApi(eanProduct);
      if (result) {
        alert("Product added successfully!");
        setEanProduct({
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
      <h2>Search by EAN</h2>
      <form onSubmit={handleEanSubmit}>
        <input type="text" value={eanSearch} onChange={(e) => setEanSearch(e.target.value)} placeholder="Enter EAN" />
        <button type="submit">Search</button>
      </form>

      {eanProduct.productName && (
        <form onSubmit={addProduct}>
          <fieldset>
            <legend>Import Product</legend>
            <input name="productName" value={eanProduct.productName} onChange={handleEanChange} />
            <input name="ean" value={eanProduct.ean} onChange={handleEanChange} />
            <input name="image" value={eanProduct.image} onChange={handleEanChange} />
            <input name="description" value={eanProduct.description} onChange={handleEanChange} />
            <input type="number" name="shoppingPrice" value={eanProduct.shoppingPrice} onChange={handleEanChange} />
            <input type="number" name="sellingPrice" value={eanProduct.sellingPrice} onChange={handleEanChange} />
            <input name="category" value={eanProduct.category} onChange={handleEanChange} />
            <button type="submit">Add Product</button>
          </fieldset>
        </form>
      )}
    </section>
  );
}
