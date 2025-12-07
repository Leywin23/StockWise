import React, { createContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { loginFromApi, LoginResponse } from "../api/auth";

export type UserProfile = {
  userName: string;
  email: string;
};

type UserContextType = {
  user: UserProfile | null;
  token: string | null;
  loginUser: (email: string, password: string) => Promise<void>;
  logout: () => void;
  isLoggedIn: () => boolean;
};

type Props = { children: React.ReactNode };

export const UserContext = createContext<UserContextType>({
  user: null,
  token: null,
  loginUser: async () => {},
  logout: () => {},
  isLoggedIn: () => false,
});

export const UserProvider = ({ children }: Props) => {
  const navigate = useNavigate();
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isReady, setIsReady] = useState(false);

  //
  // INITIAL LOAD (restore session)
  //
  useEffect(() => {
    const storedUser = sessionStorage.getItem("user");
    const storedToken = sessionStorage.getItem("token");

    if (storedUser && storedToken) {
      setUser(JSON.parse(storedUser));
      setToken(storedToken);
    }

    setIsReady(true);
  }, []);

  //
  // LOGIN
  //
  const loginUser = async (email: string, password: string) => {
    try {
      const res: LoginResponse = await loginFromApi({ email, password });

      const userObj: UserProfile = {
        userName: res.userName,
        email: res.email,
      };

      // store in session (auto-clears after closing browser)
      sessionStorage.setItem("token", res.token);
      sessionStorage.setItem("user", JSON.stringify(userObj));

      setToken(res.token);
      setUser(userObj);

      toast.success("Login success!");
      navigate("/search");
    } catch (e: any) {
      const msg =
        e?.response?.data?.message ||
        e?.response?.data?.title ||
        e?.message ||
        "Login failed";

      toast.error(msg);
    }
  };

  //
  // LOGOUT
  //
  const logout = () => {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("user");

    setToken(null);
    setUser(null);

    navigate("/login");
  };

  //
  // IS LOGGED IN
  //
  const isLoggedIn = () => !!token;

  return (
    <UserContext.Provider value={{ user, token, loginUser, logout, isLoggedIn }}>
      {isReady ? children : null}
    </UserContext.Provider>
  );
};

export const useAuth = () => React.useContext(UserContext);
