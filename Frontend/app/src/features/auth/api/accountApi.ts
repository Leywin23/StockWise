import apiClient from "../../../app/core/api/axiosClient";

export type LoginRequest = {
  email: string;
  password: string;
};

export type LoginResponse = {
  userName: string;
  email: string;
  token: string;
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

export const getAllCompanyWorkers = async() : Promise<WorkerDto[]>=>{
  const res = await apiClient.get<WorkerDto[]>('Account/CompanyWorkers');
  return res.data;
};