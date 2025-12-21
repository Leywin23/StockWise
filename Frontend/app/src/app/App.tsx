import React, { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import "../App.css";

import signalRConnection from "../signalrClient";

import { useAuth } from "./core/context/AuthContext";

import LoginPage from "../features/auth/pages/LoginPage";
import RegisterPage from "../features/auth/pages/RegisterPage";
import CompanyProductsPage from "../features/products/pages/CompanyProductsPage";
import CreateCompanyWithAccount from "../Components/CreateCompanyWithAccount/CreateCompanyWithAccount";
import VerifyEmail from "../Components/VerifyEmail/VerifyEmail";

import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "../shared/ui/Navbar";
import VerifyEmailPage from "../features/auth/pages/VerifyEmailPage";
import CreateCompanyWithAccountPage from "../features/auth/pages/CreateCompanyWithAccountPage";


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
