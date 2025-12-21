import React, { createContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { loginFromApi, LoginResponse } from "../../../features/auth/api/auth";
import { CompanyDto, getMyCompanyFromApi } from "../../../features/company/api/companyApi";

export type UserProfile = {
  userName: string;
  email: string;
};

type UserContextType = {
  user: UserProfile | null;
  token: string | null;
  company: CompanyDto | null;
  isLoggedIn: boolean;
  loginUser: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

export const UserContext = createContext<UserContextType>({
  user: null,
  token: null,
  company: null,
  isLoggedIn: false,
  loginUser: async () => {},
  logout: () => {},
});

export const UserProvider = ({ children }: { children: React.ReactNode }) => {
  const navigate = useNavigate();

  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<UserProfile | null>(null);
  const [ready, setReady] = useState(false);
  const [company, setCompany] = useState<CompanyDto | null>(null);


  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    const storedUser = localStorage.getItem("user");   
    const storedCompany = localStorage.getItem("company");

    if (storedToken) setToken(storedToken);
    if (storedUser) setUser(JSON.parse(storedUser));
    if (storedCompany) setCompany(JSON.parse(storedCompany));

    setReady(true);
  }, []);

  useEffect(()=>{
    const loadCompany = async () => {
      if(!token){
        setCompany(null);
        return;
      }
      try{
        const c = await getMyCompanyFromApi();
        setCompany(c);
        localStorage.setItem("company", JSON.stringify(c));
      }catch{
        setCompany(null);
      }
    };

    loadCompany();
  }, [token]);

  const loginUser = async (email: string, password: string) => {
    try {
      const res: LoginResponse = await loginFromApi({ email, password });

      const userObj: UserProfile = {
        userName: res.userName,
        email: res.email,
      };

      localStorage.setItem("token", res.token);
      localStorage.setItem("user", JSON.stringify(userObj));

      setToken(res.token);
      setUser(userObj);

      try {
        const c = await getMyCompanyFromApi();
        setCompany(c);
        localStorage.setItem("company", JSON.stringify(c));
      } catch {
        setCompany(null);
      }

      toast.success("Login success!");
      navigate("/company-products");
    } catch (e: any) {
      const msg =
        e?.response?.data?.detail ||
        e?.response?.data?.title ||
        e?.message ||
        "Login failed";

      toast.error(msg);
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    localStorage.removeItem("company");
    
    setToken(null);
    setUser(null);
    setCompany(null);

    navigate("/login");
  };

  const isLoggedIn = !!token;

  return (
    <UserContext.Provider value={{ user, token, company, isLoggedIn, loginUser, logout }}>
      {ready ? children : null}
    </UserContext.Provider>
  );
};

export const useAuth = () => React.useContext(UserContext);