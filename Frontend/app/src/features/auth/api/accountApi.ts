import { SortDirection } from "@mui/material";
import apiClient from "../../../app/core/api/axiosClient";
import { PageResult } from "../../products/api/companyProducts";

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  userName: string;
  email: string;
  token: string;
  role:string;
};

export const loginFromApi = async (
  dto: LoginRequest
): Promise<LoginResponse> => {
  const response = await apiClient.post<LoginResponse>("/Account/login", dto);
  return response.data;
};

export type RegisterRequest = {
  username: string;
  email: string;
  password: string;
  companyNIP: string;
};

export type RegisterResponse = {
  username: string;
  email: string;
  companyName: string;
};

export type ApiError = {
  status: number;
  title: string;
  detail: string;
  traceId: string;
  errors?: Record<string, string[]>;
};

export type NewUserDto = {
  userName: string;
  email: string;
  companyName: string;
};

export enum CompanyMembershipStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Suspended = 3,
};

export type WorkerDto = {
  id: string;
  name: string;
  email: string;
  role : string;
  companyMembershipStatus: CompanyMembershipStatus;
};

export enum CompanyRole {
  Manager = 0,
  Worker = 1,
}

export enum WorkersSortBy {
  Name = 0,
  Email = 1,
  Role = 2,
  CompanyMembershipStatus = 3,
}

export enum sortDir{
  Desc,
  Asc
}

export type WorkerQueryParams = {
  page:number;
  pageSize: number
  sortedBy: WorkersSortBy;
  sortDir: sortDir
};

export const registerFromApi = async(
  data: RegisterRequest
): Promise<NewUserDto> => {
  const res = await apiClient.post<NewUserDto>("/Account/register", data);
  return res.data;
}

export const logoutFromApi = async() => {
  const res = await apiClient.post("/Account/logout", null);
  return res.data;
};

export const getAllCompanyWorkers = async(q: WorkerQueryParams) : Promise<PageResult<WorkerDto> | null>=>{
  try{
    const res = await apiClient.get<PageResult<WorkerDto>>('Account/CompanyWorkers', {
      params:{
        page: q.page,
        pageSize: q.pageSize,
        sortedBy: q.sortedBy,
        sortDir: q.sortDir
      }
    });
    return res.data;
  }catch(err) {
    console.error("GET /Account/CompanyWorkers error:", (err as any)?.response?.data ?? err);
    return null;
  }
};

export const ApproveWorkerFromApi = async(userId: string) : Promise<string>=>{
  const res = await apiClient.post<string>(`Account/approve-user/${userId}`);
  return res.data;
};

export const SuspendWorkerFromCompany = async(userId: string) : Promise<string> => {
  const res = await apiClient.post<string>(`Account/companies/suspend/${userId}`);
  return res.data;
};

export const UnsuspendWorkerFromApi = async(userId: string) : Promise<string> => {
  const res = await apiClient.post<string>(`Account/companies/unsuspend/${userId}`);
  return res.data;
};