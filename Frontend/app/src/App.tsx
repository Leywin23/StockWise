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
  ProductWithCategory,
  UpdateProductApi
} from './api';

function App() {
  const [products, setProducts] = useState<Product[]>([]);
  const [productsWithCategory, setProductsWithCategory] = useState<ProductWithCategory[]>([])
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
  const[eanProduct, setEanProduct] = useState<ProductDto>({
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
      setProductsWithCategory(responseBody); // OK: ProductWithCategory[]
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

    const { productId, category, ...rest } = product;

    const productDto: ProductDto = {
      ...rest,
      category: category.name 
    };

    setEanProduct(productDto);
  } catch (error) {
    console.error("Błąd podczas pobierania produktu:", error);
  }
};

    const handleChange = (e:any) =>{
    const {name, value}= e.target;
    setNewProduct(prev=>({...prev, [name]: value}))
  };

    const handleEanChange = (e:any) =>{
      const {name, value} = e.target;
      setEanProduct(prev => ({...prev, [name]: value}))
    }

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
  <div className="App" style={{ padding: '20px', fontFamily: 'Arial' }}>
    <h1>Product Manager</h1>

    <section>
      <h2>Available Products</h2>
      {productsWithCategory.length > 0 ? (
        <ul style={{ listStyle: 'none', padding: 0 }}>
          {productsWithCategory.map((p) => (
            <li key={p.product.productId} style={{ border: '1px solid #ccc', padding: '10px', marginBottom: '10px', borderRadius: '8px' }}>
              <h3>{p.product.productName}</h3>
              <p>{p.product.description}</p>
              <p><strong>Price:</strong> ${p.product.sellingPrice}</p>
              <p><strong>Category:</strong> {p.categoryString}</p>

              <button onClick={() => handleDelete(p.product.productId)}>Delete</button>
              <button onClick={() => handleEdit(p.product)} style={{ marginLeft: '10px' }}>Edit</button>

              {isEdited && editedProduct?.productId === p.product.productId && (
              <form
                onSubmit={(e) => {
                  editProduct(e, editedProduct);
                  setIsEdited(false);
                  setEditedProduct(null);
                }}
                style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxWidth: '400px', marginTop: '10px' }}
              >
                <input name="productName" value={editedProduct.productName} onChange={handleEditChange} placeholder="Product Name" />
                <input name="ean" value={editedProduct.ean} onChange={handleEditChange} placeholder="EAN" />
                <input name="image" value={editedProduct.image} onChange={handleEditChange} placeholder="Image" />
                <input name="description" value={editedProduct.description} onChange={handleEditChange} placeholder="Description" />
                <input type="number" name="shoppingPrice" value={editedProduct.shoppingPrice} onChange={handleEditChange} placeholder="Shopping Price" />
                <input type="number" name="sellingPrice" value={editedProduct.sellingPrice} onChange={handleEditChange} placeholder="Selling Price" />
                <input name="category" value={editedProduct.category?.name || ''} onChange={handleEditChange} placeholder="Category" />
                <button type="submit">Save Changes</button>
              </form>
              )}
            </li>
          ))}
        </ul>
      ) : (
        <h3>No products</h3>
      )}
      <button onClick={loadProducts}>Load Products</button>
    </section>

    <section style={{ marginTop: '40px' }}>
      <h2>Add Product</h2>
      <form onSubmit={(e) => addProduct(e, newProduct)} style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxWidth: '400px' }}>
        <input name="productName" value={newProduct.productName} onChange={handleChange} placeholder="Product Name" />
        <input name="ean" value={newProduct.ean} onChange={handleChange} placeholder="EAN" />
        <input name="image" value={newProduct.image} onChange={handleChange} placeholder="Image URL" />
        <input name="description" value={newProduct.description} onChange={handleChange} placeholder="Description" />
        <input type="number" name="shoppingPrice" value={newProduct.shoppingPrice} onChange={handleChange} placeholder="Shopping Price" />
        <input type="number" name="sellingPrice" value={newProduct.sellingPrice} onChange={handleChange} placeholder="Selling Price" />
        <input name="category" value={newProduct.category} onChange={handleChange} placeholder="Category Name" />
        <button type="submit">Add Product</button>
      </form>

      {message && <p style={{ color: 'green' }}>{message}</p>}
    </section>

    {newProduct.productName && (
      <section style={{ marginTop: '40px' }}>
        <h2>Product Preview</h2>
        <div style={{ background: '#f9f9f9', padding: '10px', borderRadius: '8px' }}>
          <p><strong>Name:</strong> {newProduct.productName}</p>
          <p><strong>Description:</strong> {newProduct.description}</p>
          <p><strong>Category:</strong> {newProduct.category}</p>
          <p><strong>Shopping Price:</strong> {newProduct.shoppingPrice}</p>
          <p><strong>Selling Price:</strong> {newProduct.sellingPrice}</p>
        </div>
      </section>
    )}

    <section style={{ marginTop: '40px' }}>
      <h2>Search by EAN</h2>
      <form
        onSubmit={handleEanSubmit}
        style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxWidth: '400px', marginTop: '20px' }}
      >
        <input
          type="text"
          value={eanSearch}
          onChange={(e) => setEanSearch(e.target.value)}
          placeholder='Enter EAN'
        />
        <button type="submit">Search by EAN</button>
      </form>

      {eanProduct && (
        <form onSubmit={(e) => addProduct(e, eanProduct)} style={{ display: 'flex', flexDirection: 'column', gap: '10px', maxWidth: '400px' }}>
          <fieldset>
            <legend>Import Product</legend>
            <input name="productName" value={eanProduct.productName} onChange={handleEanChange} placeholder="Product Name" />
            <input name="ean" value={eanProduct.ean} onChange={handleEanChange} placeholder="EAN" />
            <input name="image" value={eanProduct.image} onChange={handleEanChange} placeholder="Image" />
            <input name="description" value={eanProduct.description} onChange={handleEanChange} placeholder="Description" />
            <input type="number" name="shoppingPrice" value={eanProduct.shoppingPrice} onChange={handleEanChange} placeholder="Shopping Price" />
            <input type="number" name="sellingPrice" value={eanProduct.sellingPrice} onChange={handleEanChange} placeholder="Selling Price" />
            <input name="category" value={eanProduct.category} onChange={handleEanChange} placeholder="Category" />
            <button type="submit">Add Product</button>
          </fieldset>
        </form>
      )}
    </section>
  </div>
);

}

export default App;
