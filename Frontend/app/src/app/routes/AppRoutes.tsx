import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "../core/context/AuthContext";

import CompanyProductsPage from "../../features/company/Pages/CompanyProductsPage";
import LoginPage from "../../features/auth/pages/LoginPage";
import RegisterPage from "../../features/auth/pages/RegisterPage";
import CreateCompanyWithAccount from "../../features/company/components/CreateCompanyWithAccount";
import CreateCompanyWithAccountPage from "../../features/auth/pages/CreateCompanyWithAccountPage";
import VerifyEmailPage from "../../features/auth/pages/VerifyEmailPage";
import VerifyEmail from "../../features/auth/pages/VerifyEmail";
import ManagerPanel from "../../features/auth/pages/ManagerPage";

type AuthGateProps = {
  whenLoggedIn: React.ReactNode;
  whenLoggedOut: React.ReactNode;
};

function AuthGate({ whenLoggedIn, whenLoggedOut }: AuthGateProps) {
  const { isLoggedIn } = useAuth();
  return <>{isLoggedIn ? whenLoggedIn : whenLoggedOut}</>;
}

export default function AppRoutes() {
  const { isLoggedIn } = useAuth();

  return (
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
          <AuthGate
            whenLoggedIn={<Navigate to="/company-products" replace />}
            whenLoggedOut={<CreateCompanyWithAccountPage />}
          />
        }
      />

      <Route
        path="/verify-email"
        element={
          <AuthGate
            whenLoggedIn={<Navigate to="/company-products" replace />}
            whenLoggedOut={<VerifyEmailPage />}
          />
        }
      />

      <Route
        path="/login"
        element={
          <AuthGate
            whenLoggedIn={<Navigate to="/company-products" replace />}
            whenLoggedOut={<LoginPage />}
          />
        }
      />

      <Route
        path="/register"
        element={
          <AuthGate
            whenLoggedIn={<Navigate to="/company-products" replace />}
            whenLoggedOut={<RegisterPage />}
          />
        }
      />

      <Route
        path="/create-company"
        element={
          <AuthGate
            whenLoggedIn={<Navigate to="/company-products" replace />}
            whenLoggedOut={<CreateCompanyWithAccount />}
          />
        }
      />

      <Route path="/verify-email" element={<VerifyEmail />} />

      <Route
        path="/company-products"
        element={
          <AuthGate
            whenLoggedIn={<CompanyProductsPage />}
            whenLoggedOut={<Navigate to="/login" replace />}
          />
        }
      />

      <Route 
        path="/manager-panel"
        element={
          <AuthGate
            whenLoggedIn={<ManagerPanel/>}
            whenLoggedOut={<Navigate to="/login" replace />}
          />
        }
      />

      <Route path="*" element={<div>Not found</div>} />
    </Routes>
  );
}
