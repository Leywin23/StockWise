import apiClient from "./axiosClient";

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