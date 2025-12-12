import React, { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import "./App.css";

import signalRConnection from "./signalrClient";

import { useAuth } from "./context/AuthContext";

import LoginPage from "./Pages/LoginPage";
import RegisterPage from "./Pages/RegisterPage";
import CompanyProductsPage from "./Pages/CompanyProductsPage";
import CreateCompanyWithAccount from "./Components/CreateCompanyWithAccount/CreateCompanyWithAccount";
import VerifyEmail from "./Components/VerifyEmail/VerifyEmail";

import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "./Components/Navbar/Navbar";
import VerifyEmailPage from "./Pages/VerifyEmailPage";
import CreateCompanyWithAccountPage from "./Pages/CreateCompanyWithAccountPage";
import OrderPage from "./Pages/OrderPage";

function App() {
  const { isLoggedIn } = useAuth();


  useEffect(() => {
    if (!isLoggedIn) return;

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

  return (
    <div className="App" style={{ padding: "20px" }}>
      <Navbar/>
      <Routes>
        <Route
          path="/"
          element={
            isLoggedIn ? (
              <Navigate to="/company-products" replace />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />

          <Route
          path="/OrderProduct"
          element={
            isLoggedIn ? (
              <OrderPage />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />
        
        <Route
          path="/create-account-with-company"
          element={
            isLoggedIn ? (
              <Navigate to="/company-products" replace />
            ) : (
              <CreateCompanyWithAccountPage />
            )
          }
        />
         <Route
          path="/verify-email"
          element={
            isLoggedIn ? (
              <Navigate to="/company-products" replace />
            ) : (
              <VerifyEmailPage />
            )
          }
        />
        <Route
          path="/login"
          element={
            isLoggedIn ? <Navigate to="/company-products" replace /> : <LoginPage />
          }
        />
        <Route
          path="/register"
          element={
            isLoggedIn ? (
              <Navigate to="/company-products" replace />
            ) : (
              <RegisterPage />
            )
          }
        />

        <Route
          path="/create-company"
          element={
            isLoggedIn ? (
              <Navigate to="/company-products" replace />
            ) : (
              <CreateCompanyWithAccount />
            )
          }
        />

        <Route path="/verify-email" element={<VerifyEmail />} />

        <Route
          path="/company-products"
          element={
            isLoggedIn ? (
              <CompanyProductsPage />
            ) : (
              <Navigate to="/login" replace />
            )
          }
        />

        <Route path="*" element={<div>Not found</div>} />
      </Routes>
    </div>
  );
}

export default App;
