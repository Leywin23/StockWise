import React, { useState, useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import "./App.css";

import {
  ProductDto,
} from "./api";
import signalRConnection from "./signalrClient";

import AddProductForm from "./Components/AddProductForm/AddProductFrom";
import EanSearchForm from "./Components/EanSearchForm/EanSearchForm";
import InventoryMovementForm from "./Components/InventoryMovementForm/InventoryMovementForm";

import CreateCompanyWithAccount from "./Components/CreateCompanyWithAccount/CreateCompanyWithAccount";
import VerifyEmail from "./Components/VerifyEmail/VerifyEmail";
import LoginPage from "./Pages/LoginPage";       
import { useAuth } from "./context/AuthContext";  
import CompanyProductsPage from "./Pages/CompanyProductsPage";

function App() {
  const [products, setProducts] = useState<ProductDto[]>([]);
  const { isLoggedIn } = useAuth(); 

  /*const loadProducts = async () => {
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
  };*/

  useEffect(() => {
    if (!isLoggedIn()) return;

    const startConnection = async () => {
      if (signalRConnection.state === HubConnectionState.Disconnected) {
        try {
          await signalRConnection.start();
          console.log("SignalR connected");

          signalRConnection.on(
            "StockUpdated",
            (productId: number, stock: number) => {
              console.log(
                `Stock updated: Product ${productId} has now ${stock} items`
              );
              
            }
          );
        } catch (err) {
          console.error("SignalR connection error:", err);
        }
      } else {
        console.log("SignalR already connected or connecting");
      }
    };

    startConnection();

    return () => {
      signalRConnection.off("StockUpdated");
    };
  }, [isLoggedIn]); 

  if (!isLoggedIn()) {
    return (
      <div className="App" style={{ padding: "20px" }}>
        {}
        <h1>Authentication</h1>
        <LoginPage />
        <CreateCompanyWithAccount />
        <VerifyEmail />
      </div>
    );
  }


  return (
    <div className="App" style={{ padding: "20px" }}>
      <h1>Product Manager</h1>


      <CompanyProductsPage />
    </div>
  );
}

export default App;
