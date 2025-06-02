import React, { useState } from 'react';
import logo from './logo.svg';
import './App.css';
import {
  getProductsByEanFromApi,
  getProductsFromApi,
  postProductToApi,
  Product,
  ProductDto
} from './api';

function App() {
  const [products, setProducts] = useState<Product[]>([]);
  const [newProduct, setNewProduct] = useState<ProductDto>({
    productName: '',
    ean: '',
    image: '',
    description: '',
    shoppingPrice: 0,
    sellingPrice: 0,
    category: ''
  });

  const [eanSearch, setEanSearch] = useState('');
  const [message, setMessage] = useState('');

  const handleEanSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!eanSearch) return;
    await fetchProductWithEan(eanSearch);
  };

  const loadProducts = async () => {
    try {
      const responseBody = await getProductsFromApi();
      if (Array.isArray(responseBody)) {
        setProducts(responseBody);
        console.log(responseBody);
      } else {
        console.error("Expected an array of products, but received:", responseBody);
      }
    } catch (error) {
      console.error("Error while loading products:", error);
    }
  };

  const addProduct = async (e: any, product: ProductDto) => {
    e.preventDefault();
    try {
      const result = await postProductToApi(product);
      if (result != null) {
        setMessage("Produkt został dodany pomyślnie!");
        console.log(result);
      } else {
        console.error("Expected a product, but received:", result);
      }
    } catch (err) {
      console.error("Error while adding product", err);
    }
  };

  const fetchProductWithEan = async (ean: string) => {
    try {
      const product = await getProductsByEanFromApi(ean);
      if (!product) {
        console.log("Produkt o podanym EAN nie został znaleziony.");
        return;
      }
      const { productId, ...productDto } = product;
      setNewProduct(productDto);
    } catch (error) {
      console.error("Błąd podczas pobierania produktu:", error);
    }
  };

    const handleChange = (e:any) =>{
    const {name, value}= e.target;
    setNewProduct(prev=>({...prev, [name]: value}))
  };

  return (
    <div className="App">
      {products.length > 0 ? (
        products.map((p) => (
          <li key={p.productId}>
            <h2>{p.productName}</h2>
            <h3>{p.description}</h3>
            <p>{p.sellingPrice}$</p>
          </li>
        ))
      ) : (
        <h2>No products</h2>
      )}

      <button onClick={loadProducts}>load products</button>

      <div>
        <form onSubmit={(e) => addProduct(e, newProduct)}>
          <input name="productName" value={newProduct.productName} onChange={handleChange} placeholder="Product Name" />
          <input name="ean" value={newProduct.ean} onChange={handleChange} placeholder="EAN" />
          <input name="image" value={newProduct.image} onChange={handleChange} placeholder="Image" />
          <input name="description" value={newProduct.description} onChange={handleChange} placeholder="Description" />
          <input
            type="number"
            name="shoppingPrice"
            value={newProduct.shoppingPrice}
            onChange={handleChange}
            placeholder="Shopping Price"
          />
          <input
            type="number"
            name="sellingPrice"
            value={newProduct.sellingPrice}
            onChange={handleChange}
            placeholder="Selling Price"
          />
          <input name="category" value={newProduct.category} onChange={handleChange} placeholder="Category" />
          <button type="submit">Add Product</button>
        </form>
        {message && <p style={{ color: 'green' }}>{message}</p>}
      </div>

      {newProduct.productName && (
        <div style={{ marginTop: "20px" }}>
          <h2>Product Preview</h2>
          <p><strong>Name:</strong> {newProduct.productName}</p>
          <p><strong>Description:</strong> {newProduct.description}</p>
          <p><strong>Category:</strong> {newProduct.category}</p>
          <p><strong>Shopping Price:</strong> {newProduct.shoppingPrice}</p>
          <p><strong>Selling Price:</strong> {newProduct.sellingPrice}</p>
        </div>
      )}

      <div>
        <form onSubmit={handleEanSubmit}>
          <input
            type="text"
            value={eanSearch}
            onChange={(e) => setEanSearch(e.target.value)}
            placeholder='Enter EAN'
          />
          <button type="submit">Search by EAN</button>
        </form>
      </div>
    </div>
  );
}

export default App;
