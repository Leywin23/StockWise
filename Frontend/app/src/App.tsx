import React, { useState, useEffect } from 'react';
import './App.css';
import {
  ProductWithCategory,
  getProductsFromApi,
} from './api';
import signalRConnection from './signalrClient';

import ProductList from './Components/ProductList/ProductList';
import AddProductForm from './Components/AddProductForm/AddProductFrom';
import EanSearchForm from './Components/EanSearchForm/EanSearchForm';
import InventoryMovementForm from './Components/InventoryMovementForm/InventoryMovementForm';

function App() {
  const [productsWithCategory, setProductsWithCategory] = useState<ProductWithCategory[]>([]);

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

  useEffect(() => {
    signalRConnection.start()
      .then(() => console.log('SignalR connected'))
      .catch((err: any) => console.error('SignalR connection error:', err));

    signalRConnection.on('StockUpdated', (productId: number, stock: number) => {
      console.log(`Stock updated: Product ${productId} has now ${stock} items`);
      loadProducts();
    });

    return () => {
      signalRConnection.off('StockUpdated');
    };
  }, []);

  return (
    <div className="App" style={{ padding: '20px' }}>
      <h1>Product Manager</h1>

      <button onClick={loadProducts}>Load Products</button>

      <ProductList productsWithCategory={productsWithCategory} onProductsChange={loadProducts} />
      <AddProductForm onProductAdded={loadProducts} />
      <EanSearchForm onProductAdded={loadProducts} />
      <InventoryMovementForm />
    </div>
  );
}

export default App;
