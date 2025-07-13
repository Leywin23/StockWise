import React, { useState, useEffect } from 'react';
import { HubConnectionState } from "@microsoft/signalr"; 
import './App.css';
import {
  Product,
  ProductDto,
  getProductsFromApi,
} from './api';
import signalRConnection from './signalrClient';

import ProductList from './Components/ProductList/ProductList';
import AddProductForm from './Components/AddProductForm/AddProductFrom';
import EanSearchForm from './Components/EanSearchForm/EanSearchForm';
import InventoryMovementForm from './Components/InventoryMovementForm/InventoryMovementForm';

function App() {
  const [products, setProducts] = useState<ProductDto[]>([]);

  const loadProducts = async () => {
    try {
      const responseBody = await getProductsFromApi();
      if (Array.isArray(responseBody)) {
        setProducts(responseBody);
      } else {
        console.error("Expected array, got:", responseBody);
      }
    } catch (error) {
      console.error("Error loading products:", error);
    }
  };

 useEffect(() => {
  const startConnection = async () => {
    if (signalRConnection.state === HubConnectionState.Disconnected) {
      try {
        await signalRConnection.start();
        console.log('SignalR connected');

        signalRConnection.on('StockUpdated', (productId: number, stock: number) => {
          console.log(`Stock updated: Product ${productId} has now ${stock} items`);
          loadProducts();
        });
      } catch (err) {
        console.error('SignalR connection error:', err);
      }
    } else {
      console.log('SignalR already connected or connecting');
    }
  };

  startConnection();

  return () => {
    signalRConnection.off('StockUpdated');
  };
}, []);

  return (
    <div className="App" style={{ padding: '20px' }}>
      <h1>Product Manager</h1>

      <button onClick={loadProducts}>Load Products</button>

      <ProductList products={products} onProductsChange={loadProducts} />
      <AddProductForm onProductAdded={loadProducts} />
      <EanSearchForm onProductAdded={loadProducts} />
      <InventoryMovementForm />
    </div>
  );
}

export default App;
