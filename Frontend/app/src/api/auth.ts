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
