import React from 'react'
import { logoutFromApi } from '../../features/auth/api/accountApi'
import { useAuth } from '../../app/core/context/AuthContext';
import { useNavigate } from "react-router-dom";
import { toast } from 'react-toastify';

type Props = {}

const Navbar = (props: Props) => {
const {isLoggedIn ,logout, user} = useAuth();
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

      {isLoggedIn && (
        <div className="flex gap-4 items-center">
          {user && (
            <span>
              {user.userName} ({user.email}) [{user.role}]
            </span>
          )}

          <button
            onClick={handleLogout}
            className="bg-red-500 hover:bg-red-600 px-4 py-1 rounded"
          >
            Logout
          </button>
        </div>
      )}
    </div>
  );
}

export default Navbar