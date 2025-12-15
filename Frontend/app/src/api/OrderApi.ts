import { Product } from "../api";
import apiClient from "./axiosClient";
import { Money } from "./companyProducts";

export enum OrderStatus {
    Pending = 10,
    Accepted = 20,
    Rejected = 30,
    Canceled = 40,
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

export type OrderProductDto = {
  companyProductId:number;
  productName: string;
  ean: string;
  price: number;
  quantity : number;
}

export type OrderDto = {
  seller : CompanyMiniDto;
  buyer : CompanyMiniDto;
  status : OrderStatus;
  createdAt : Date;
  userNameWhoMakeOrder : string;
  products : OrderProductDto[];
  totalPrice : Money;
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

export const deleteOrderFromApi = async (id:number) : Promise<OrderListDto> => {
    const res = await apiClient.delete<OrderListDto>(`/order/${id}`);
    return res.data;
}

export type UpdateOrderProductLineDto = {
  ean: string;
  quantity: number;
};

export type UpdateOrderDto = {
  currency: string;
  productsEANWithQuantity: Record<string, number>;
};

export const putOrderFromApi = async (id:number ,dto: UpdateOrderDto) : Promise<OrderListDto> =>{
    const res = await apiClient.put<OrderListDto>(`/order/${id}`, dto);
    return res.data;
}

export const acceptOrRejectOrderFromApi = async (
  id: number,
  status: OrderStatus
): Promise<OrderDto> => {
  const res = await apiClient.put<OrderDto>(
    `/order/AcceptOrRejectOrder/${id}`,
    status, 
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return res.data;
};

export const cancelOrConfirmFromApi = async (
  id: number,
  status: OrderStatus
): Promise<OrderDto> => {
  const res = await apiClient.put<OrderDto>(
    `/order/CancelOrCorfirm/${id}`,
    status, 
    {
      headers: { "Content-Type": "application/json" },
    }
  );
  return res.data;
};