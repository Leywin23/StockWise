import React, { useState } from 'react';
import './App.css';
import {
  DeleteProductApi,
  getProductsByEanFromApi,
  getProductsFromApi,
  postProductToApi,
  UpdateProductApi,
  Product,
  ProductDto,
  ProductWithCategory,
} from './api';

function App() {
  const [productsWithCategory, setProductsWithCategory] = useState<ProductWithCategory[]>([]);
  const [newProduct, setNewProduct] = useState<ProductDto>({
    productName: '',
    ean: '',
    image: '',
    description: '',
    shoppingPrice: 0,
    sellingPrice: 0,
    category: ''
  });

  const [eanProduct, setEanProduct] = useState<ProductDto>({
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
  const [isEdited, setIsEdited] = useState(false);
  const [editedProduct, setEditedProduct] = useState<Product | null>(null);

  const loadProducts = async () => {
    try {
      const responseBody = await getProductsFromApi();
      if (Array.isArray(responseBody)) {
        setProductsWithCategory(responseBody);
      } else {
        console.error("Expected array, got:", responseBody);
      }
    } catch (error) {
      console.error("Error loading products:", error);
    }
  };

  const addProduct = async (e: any, product: ProductDto) => {
    e.preventDefault();
    try {
      const result = await postProductToApi(product);
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
        await loadProducts();
      }
    } catch (err) {
      console.error("Error adding product", err);
    }
  };

  const handleChange = (e: any) => {
    const { name, value } = e.target;
    setNewProduct(prev => ({ ...prev, [name]: value }));
  };

  const handleEanChange = (e: any) => {
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
      const productDto: ProductDto = { ...rest, category: category.name };
      setEanProduct(productDto);
    } catch (err) {
      console.error("Error fetching EAN product:", err);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await DeleteProductApi(id);
      await loadProducts();
    } catch (err) {
      console.error("Error deleting product", err);
    }
  };

  const handleEdit = (product: Product) => {
    setIsEdited(true);
    setEditedProduct(product);
  };

  const handleEditChange = (e: any) => {
    const { name, value } = e.target;
    setEditedProduct(prev =>
      prev ? { ...prev, [name]: value } : null
    );
  };

  const editProduct = async (e: any, product: Product) => {
    e.preventDefault();
    if (!product || !product.category || !product.category.categoryId) {
      alert("Missing category ID for update.");
      return;
    }

    const payload = {
      productId: product.productId,
      productName: product.productName,
      ean: product.ean,
      image: product.image,
      description: product.description,
      shoppingPrice: product.shoppingPrice,
      sellingPrice: product.sellingPrice,
      categoryId: product.category.categoryId
    };

    try {
      await UpdateProductApi(payload);
      setIsEdited(false);
      setEditedProduct(null);
      await loadProducts();
    } catch (err) {
      console.error("Error updating product:", err);
    }
  };

  return (
    <div className="App" style={{ padding: '20px' }}>
      <h1>Product Manager</h1>

      <section>
        <h2>Available Products</h2>
        <button onClick={loadProducts}>Load Products</button>
        <ul>
          {productsWithCategory.map(p => (
            <li key={p.product.productId}>
              <strong>{p.product.productName}</strong> - {p.categoryString}
              <br />
              <button onClick={() => handleDelete(p.product.productId)}>Delete</button>
              <button onClick={() => handleEdit(p.product)}>Edit</button>

              {isEdited && editedProduct?.productId === p.product.productId && (
                <form onSubmit={(e) => editProduct(e, editedProduct)}>
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

      <section>
        <h2>Add Product</h2>
        <form onSubmit={(e) => addProduct(e, newProduct)}>
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

      <section>
        <h2>Search by EAN</h2>
        <form onSubmit={handleEanSubmit}>
          <input type="text" value={eanSearch} onChange={(e) => setEanSearch(e.target.value)} placeholder="Enter EAN" />
          <button type="submit">Search</button>
        </form>

        {eanProduct.productName && (
          <form onSubmit={(e) => addProduct(e, eanProduct)}>
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
    </div>
  );
}

export default App;
