import React from 'react'
import * as Yup from "yup";
import { data, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { ApiError, registerFromApi, RegisterRequest } from '../api/auth';
import { yupResolver } from '@hookform/resolvers/yup';
import { CreateCompanyWithAccountDto, createCompanyWithAccountFromApi } from '../api';
import { toast } from 'react-toastify';


const validation = Yup.object({
    userName: Yup.string()

        .required("Username is required"),

    email: Yup.string()
        .email("Invalid email")
        .required("Email is required"),

    password: Yup.string().required("Password is required"),

    companyName: Yup.string()
        .required("Company name is required"),

    nip: Yup.string()

        .required("NIP is required"),

    companyEmail: Yup.string()
        .email("Invalid company email")
        .required(),
    address: Yup.string().required(),
    phone: Yup.string().required()

});

const CreateCompanyWithAccountPage: React.FC = () => {
    const navigate = useNavigate();

    const {
        register,
        handleSubmit,
        setError,
        formState: { errors, isSubmitting }, 
      } = useForm<CreateCompanyWithAccountDto>({
        resolver: yupResolver(validation),
      });

    const onSubmit = async (data: CreateCompanyWithAccountDto) => {
        try{
            const response = await createCompanyWithAccountFromApi(data);
            toast.success("Account with company created! Check your e-mail for confirmation code.");
            navigate("/verify-email")
        }catch (err: any) {
            const res = err?.response;
            const status = res?.status as number | undefined;
            const apiError = res?.data as ApiError | undefined;
        
            const backendMessage =
              apiError?.detail ||
              apiError?.title ||
              "An error occurred during registration";
        
            if (status === 404) {
              setError("nip", {
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
      <div className="w-full max-w-lg bg-white shadow-md rounded-xl p-8 border border-slate-200">

        <h1 className="text-2xl font-bold text-slate-800 mb-6">
          Create Company & Account
        </h1>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">

          {/* USERNAME */}
          <div>
            <label className="block text-sm mb-1 font-medium text-slate-700">
              Username
            </label>
            <input
              {...register("userName")}
              className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
            />
            {errors.userName && (
              <p className="text-xs text-red-500 mt-1">
                {errors.userName.message}
              </p>
            )}
          </div>

          {/* EMAIL */}
          <div>
            <label className="block text-sm mb-1 font-medium text-slate-700">
              Email
            </label>
            <input
              {...register("email")}
              type="email"
              className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
            />
            {errors.email && (
              <p className="text-xs text-red-500 mt-1">
                {errors.email.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm mb-1 font-medium text-slate-700">
              Password
            </label>
            <input
              {...register("password")}
              type="password"
              className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
            />
            {errors.password && (
              <p className="text-xs text-red-500 mt-1">
                {errors.password.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm mb-1 font-medium text-slate-700">
              NIP
            </label>
            <input
              {...register("nip")}
              className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
            />
            {errors.nip && (
              <p className="text-xs text-red-500 mt-1">
                {errors.nip.message}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm mb-1 font-medium text-slate-700">
              Company Name
            </label>
            <input
              {...register("companyName")}
              className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
            />
            {errors.companyName && (
              <p className="text-xs text-red-500 mt-1">
                {errors.companyName.message}
              </p>
            )}
          </div>

          {/* OPTIONAL FIELDS */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm mb-1 font-medium text-slate-700">
                Company Email (optional)
              </label>
              <input
                {...register("companyEmail")}
                type="email"
                className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
              />
              {errors.companyEmail && (
                <p className="text-xs text-red-500 mt-1">
                  {errors.companyEmail.message}
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm mb-1 font-medium text-slate-700">
                Phone (optional)
              </label>
              <input
                {...register("phone")}
                className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
              />
            </div>
          </div>

          {/* ADDRESS */}
          <div>
            <label className="block text-sm mb-1 font-medium text-slate-700">
              Address (optional)
            </label>
            <input
              {...register("address")}
              className="w-full rounded-md border px-3 py-2 text-sm border-slate-300 focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* BUTTON */}
          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-blue-600 hover:bg-blue-700 text-white rounded-md py-2 text-sm font-medium disabled:opacity-60">
            {isSubmitting ? "Creating..." : "Create Account & Company"}
          </button>

        </form>
      </div>
    </div>
  );
};
export default CreateCompanyWithAccountPage