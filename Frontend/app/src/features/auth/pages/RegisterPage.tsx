import React from "react";
import * as Yup from "yup";
import "../../../shared/utils/yupExtensions";
import { useForm } from "react-hook-form";
import { ApiError, registerFromApi, RegisterRequest } from "../api/auth";
import { yupResolver } from "@hookform/resolvers/yup";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

const validation = Yup.object().shape({
  username: Yup.string()
    .username("Invalid Username")
    .required("Username is required"),

  email: Yup.string()
    .email("Invalid email")
    .required("Email is required"),

  password: Yup.string().required("Password is required"),

  companyNIP: Yup.string()
    .companyNIP("Invalid company NIP")
    .required("NIP is required"),
});

const RegisterPage: React.FC = () => {
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting }, 
  } = useForm<RegisterRequest>({
    resolver: yupResolver(validation),
  });


const onSubmit = async (data: RegisterRequest) => {
  try {
    const response = await registerFromApi(data);
    toast.success("Account created! Check your e-mail for confirmation code.");
    navigate("/verify-email");
  } catch (err: any) {
    const res = err?.response;
    const status = res?.status as number | undefined;
    const apiError = res?.data as ApiError | undefined;

    const backendMessage =
      apiError?.detail ||
      apiError?.title ||
      "An error occurred during registration";

    if (status === 404) {
      setError("companyNIP", {
        type: "server",
        message: backendMessage,
      });
      return;
    }

    toast.error(backendMessage);
  }
};

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-100">
      <div className="w-full max-w-md bg-white rounded-xl shadow-md p-6 border border-slate-200">
        <h1 className="text-2xl font-semibold text-slate-800 mb-4">
          Create account
        </h1>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Username
            </label>
            <input
              {...register("username")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.username && (
              <p className="mt-1 text-xs text-red-500">
                {errors.username.message}
              </p>
            )}
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Email
            </label>
            <input
              type="email"
              {...register("email")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.email && (
              <p className="mt-1 text-xs text-red-500">
                {errors.email.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Password
            </label>
            <input
              type="password"
              {...register("password")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.password && (
              <p className="mt-1 text-xs text-red-500">
                {errors.password.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">
              Company NIP
            </label>
            <input
              {...register("companyNIP")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.companyNIP && (
              <p className="mt-1 text-xs text-red-500">
                {errors.companyNIP.message}
              </p>
            )}
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full mt-2 inline-flex justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {isSubmitting ? "Creating account..." : "Register"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default RegisterPage;
