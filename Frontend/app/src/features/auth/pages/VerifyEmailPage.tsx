import React from 'react'
import { useForm } from 'react-hook-form'
import { verifyEmailDto, verifyEmailFromApi } from '../../../api'
import * as Yup from "yup";
import { yupResolver } from '@hookform/resolvers/yup';
import { data } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useNavigate } from "react-router-dom";

type Props = {}
const validation = Yup.object().shape({
    email : Yup.string()
    .email("Invalid email")
    .required("Email is required"),

    code: Yup.string()
    .required("Code is required")
})

const VerifyEmailPage = (props: Props) => {
    const navigate = useNavigate();
    
    const {
        register,
        handleSubmit,
        setError,
        formState: {errors, isSubmitting},
    } = useForm<verifyEmailDto>({
        resolver: yupResolver(validation),
    });

    const onSubmit = async (data: verifyEmailDto) => {
    try {
      await verifyEmailFromApi(data); 
      toast.success("Code correct, email address verified successfully");
      navigate("/login");
    } catch (err: any) {
      const apiError = err?.response?.data;
      const msg =
        apiError?.detail ||
        apiError?.title ||
        err?.message ||
        "Email verification failed";

      toast.error(msg);
    }
  };
  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-100">
      <div className="w-full max-w-md bg-white rounded-xl shadow-md p-6 border border-slate-200">
        <h1 className="text-2xl font-semibold text-slate-800 mb-2">
          Verify your email
        </h1>
        <p className="text-sm text-slate-600 mb-6">
          Enter your email and the verification code we sent you.
        </p>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
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
              Verification code
            </label>
            <input
              {...register("code")}
              className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 tracking-widest"

            />
            {errors.code && (
              <p className="mt-1 text-xs text-red-500">
                {errors.code.message}
              </p>
            )}
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full mt-2 inline-flex justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-60"
          >
            {isSubmitting ? "Verifying..." : "Verify email"}
          </button>
        </form>
      </div>
    </div>
  );
};


export default VerifyEmailPage