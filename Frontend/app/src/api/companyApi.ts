import apiClient from "./axiosClient";

export type CompanyDto = {
  name: string;
  nip: string;
  address: string;
  email: string;
  phone: string;
};



export const getMyCompanyFromApi = async (): Promise<CompanyDto> => {
  const res = await apiClient.get<CompanyDto>("/company/me");
  return res.data;
};