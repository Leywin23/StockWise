import React, { useState } from 'react';
import logo from './logo.svg';
import './App.css';
import {
  DeleteProductApi,
  getProductsByEanFromApi,
  getProductsFromApi,
  postProductToApi,
  Product,
  ProductDto,
  UpdateProductApi
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
  const[isEdited, setIsEdited] = useState<boolean>(false);
  const[eanProduct, setEanProduct] = useState<ProductDto>();

  const [eanSearch, setEanSearch] = useState('');
  const [message, setMessage] = useState('');

  const [editedProduct, setEditedProduct] = useState<Product | null>(null);


  

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
      setEanProduct(productDto);
    } catch (error) {
      console.error("Błąd podczas pobierania produktu:", error);
    }
  };

    const handleChange = (e:any) =>{
    const {name, value}= e.target;
    setNewProduct(prev=>({...prev, [name]: value}))
  };

  const handleDelete = async(id:number) =>{
    try{
      const result = await DeleteProductApi(id);
      alert("The product has been removed.");
      await loadProducts();
    }catch(err){
      console.log(`Error while deleting ${err}`)
    }

  };

  const handleEdit = async(product: Product)=>{
    setIsEdited(true);
    setEditedProduct(product);
  };

  const handleEditChange = (e:any)=>{
    const {name, value} = e.target;
    setEditedProduct(prev =>
      prev ? { ...prev, [name]: value } : null
    );
  }
  const editProduct = async(e:any,product: Product)=>{
    e.preventDefault();
    try{
      await UpdateProductApi(product);
      loadProducts();
    }catch(err){
      console.log(err);
    }
  }
  return (
    <div className="App">
      {products.length > 0 ? (
        products.map((p) => (
          <li key={p.productId}>
            <h2>{p.productName}</h2>
            <h3>{p.description}</h3>
            <p>{p.sellingPrice}$</p>
            <button onClick={()=>handleDelete(p.productId)}>Delete</button>
            <button onClick={()=>handleEdit(p)}>Edit</button>
             {isEdited && editedProduct?.productId === p.productId && (
        <form onSubmit={(e) => {
            editProduct(e, editedProduct);
            setIsEdited(false);
            setEditedProduct(null);
          }}>
          <input name="productName" value={editedProduct.productName} onChange={handleEditChange} placeholder="Product Name" />
          <input name="ean" value={editedProduct.ean} onChange={handleEditChange} placeholder="EAN" />
          <input name="image" value={editedProduct.image} onChange={handleEditChange} placeholder="Image" />
          <input name="description" value={editedProduct.description} onChange={handleEditChange} placeholder="Description" />
          <input
            type="number"
            name="shoppingPrice"
            value={editedProduct.shoppingPrice}
            onChange={handleEditChange}
            placeholder="Shopping Price"
          />
          <input
            type="number"
            name="sellingPrice"
            value={editedProduct.sellingPrice}
            onChange={handleEditChange}
            placeholder="Selling Price"
          />
          <input
            name="category"
            value={editedProduct.category}
            onChange={handleEditChange}
            placeholder="Category"
          />
          <button type="submit">Save Changes</button>
        </form>
      )}
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

        {eanProduct &&
          <form onSubmit={(e) => addProduct(e, newProduct)}>
            <input name="productName" value={eanProduct.productName} onChange={handleChange} placeholder="Product Name" />
            <input name="ean" value={eanProduct.ean} onChange={handleChange} placeholder="EAN" />
            <input name="image" value={eanProduct.image} onChange={handleChange} placeholder="Image" />
            <input name="description" value={eanProduct.description} onChange={handleChange} placeholder="Description" />
            <input
              type="number"
              name="shoppingPrice"
              value={eanProduct.shoppingPrice}
              onChange={handleChange}
              placeholder="Shopping Price"
            />
            <input
              type="number"
              name="sellingPrice"
              value={eanProduct.sellingPrice}
              onChange={handleChange}
              placeholder="Selling Price"
            />
            <input name="category" value={eanProduct.category} onChange={handleChange} placeholder="Category" />
            <button type="submit">Add Product</button>
          </form>
        }
      </div>
    </div>
  );
}

export default App;
