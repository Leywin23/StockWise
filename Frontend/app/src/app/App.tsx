import React, { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import "../App.css";

import signalRConnection from "../signalrClient";

import { useAuth } from "./core/context/AuthContext";

import LoginPage from "../features/auth/pages/LoginPage";
import RegisterPage from "../features/auth/pages/RegisterPage";
import CompanyProductsPage from "../features/company/Pages/CompanyProductsPage";
import CreateCompanyWithAccount from "../features/company/components/CreateCompanyWithAccount";
import VerifyEmail from "../features/auth/pages/VerifyEmail";

import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "../shared/ui/Navbar";
import VerifyEmailPage from "../features/auth/pages/VerifyEmailPage";
import CreateCompanyWithAccountPage from "../features/auth/pages/CreateCompanyWithAccountPage";
import AppRoutes from "./routes/AppRoutes";


export default function App() {
  const { isLoggedIn } = useAuth();

  useEffect(() => {
    if (!isLoggedIn) return;

    const startConnection = async () => {
      if (signalRConnection.state === HubConnectionState.Disconnected) {
        try {
          await signalRConnection.start();
          console.log("SignalR connected");

          signalRConnection.on("StockUpdated", (productId: number, stock: number) => {
            console.log(`Stock updated: Product ${productId} has now ${stock} items`);
          });
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
      <Navbar />
      <AppRoutes />
    </div>
  );
}
