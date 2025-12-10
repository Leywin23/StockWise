import React from 'react'
import { logoutFromApi } from '../../api/auth'
import { useAuth } from '../../context/AuthContext'
import { useNavigate } from "react-router-dom";
import { toast } from 'react-toastify';

type Props = {}

const Navbar = (props: Props) => {
const {isLoggedIn ,logout} = useAuth();
const navigate = useNavigate();

  const handleLogout = async () => {
  try {
    await logoutFromApi(); 
  } catch (err: any) {
    if (err?.code === "ERR_NETWORK" || !err?.response) {
      toast.error("No connection to the server");
    } else {
      toast.error(err?.response?.data?.detail || "Logout failed");
    }
  } finally {
    logout();
  }
};

  return (
    <div className="w-full bg-slate-800 text-white px-6 py-3 flex justify-between">
      <h1 className="font-semibold">StockWise</h1>

      {isLoggedIn&& (
        <button
          onClick={handleLogout}
          className="bg-red-500 hover:bg-red-600 px-4 py-1 rounded"
        >
          Logout
        </button>
      )}
    </div>
  );
}

export default Navbar