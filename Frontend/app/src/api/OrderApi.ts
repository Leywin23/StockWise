import { Product } from "../api";
import apiClient from "./axiosClient";
import { Money } from "./companyProducts";

export enum OrderStatus {
    Pending = 10,
    Accepted = 20,
    Rejected = 30,
    Cancelled = 40,
    Completed = 50
}

export type CompanyMiniDto = {
    id:number;
    name: string;
    nip: string;
}

export type CompanyProductMiniDto = {
    id:number;
    companyProductName: string;
    ean: string;
    price: Money
}

export type CreateOrderDto = {
  sellerName: string;
  sellerNIP: string;
  address: string;
  email?: string | null;
  phone?: string | null;
  currency: string;
  productsEANWithQuantity: Record<string, number>;
};

type ProductWithQuantityDto = {
    product: CompanyProductMiniDto;
    quantity: number;
}

export type OrderListDto = {
    id:number;
    status: OrderStatus;
    createdAt: Date;
    seller: CompanyMiniDto;
    buyer: CompanyMiniDto;
    userNameWhoMadeOrder: string;
    productsWithQuantity: ProductWithQuantityDto[];
    totalPrice: Money;
}

export const postOrderFromApi = async (
  dto: CreateOrderDto
): Promise<OrderListDto> => {
  const result = await apiClient.post<OrderListDto>('/order', dto);
  return result.data;
};
export const getOrdersFromApi = async (): Promise<OrderListDto[]> => {
  const res = await apiClient.get<OrderListDto[]>('/order');
  return res.data;
};