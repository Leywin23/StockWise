import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import App from "./App";
import reportWebVitals from "./reportWebVitals";

import { BrowserRouter } from "react-router-dom";
import { UserProvider } from "./context/AuthContext";  
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";          

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);

root.render(
  <React.StrictMode>
    <BrowserRouter>
      <UserProvider>
        <App />
        <ToastContainer />
      </UserProvider>
    </BrowserRouter>
  </React.StrictMode>
);

reportWebVitals();
