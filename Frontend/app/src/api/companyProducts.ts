import { number, string } from "yup";
import apiClient from "./axiosClient";

export type Money = {
  amount: number;
  currency: {
    code: string;
  };
};


export type companyProductDto = {
  companyProductId: number;
  companyProductName: string;
  ean: string;

  categoryName?: string;


  category?: {
    categoryId: number;
    name: string;
    parentId: number | null;
    parent: any | null;
    children: any[] | null;
  } | null;

  image: string | null;
  description: string;
  price: Money;
  stock: number;
  isAvailableForOrder: boolean;
};


export type createCompanyProductDto = {
  companyProductName: string;
  ean: string;
  category: string;
  imageFile?: File | null;
  description?: string | null;
  price: number;       
  currency: string;    
  stock: number;
  isAvailableForOrder: boolean;
};


export type PageResult<T> = {
  pageSize: number;
  page: number;
  totalCount: number;
  sortDir: number;          
  sortBy: string;
  items: T[];
};

export type CompanyProductQueryParams = {
  page?: number;
  pageSize?: number;
  stock?: number;
  isAvailableForOrder?: boolean;
  minTotal?: number;
  maxTotal?: number;
  sortedBy?: string;
  sortDir?: number; 
};

export type ValidationDetails = Record<string, string[]>;

export const getCompanyProductFromApi = async (
  query: CompanyProductQueryParams
): Promise<PageResult<companyProductDto> | null> => {
  try {
    const response = await apiClient.get<PageResult<companyProductDto>>("/CompanyProduct", {
      params: {
        page: query.page,
        pageSize: query.pageSize,
        stock: query.stock,
        isAvailableForOrder: query.isAvailableForOrder,
        minTotal: query.minTotal,
        maxTotal: query.maxTotal,
        sortedBy: query.sortedBy,
        sortDir: query.sortDir,
      },
    });

    console.log("GET /CompanyProduct response:", response.data);
    return response.data;
  } catch (err) {
    console.error("GET /CompanyProduct error:", (err as any)?.response?.data ?? err);
    return null;
  }
};

export const getCompanyProductDetailsFromApi = async () => {
  
}

export const postCompanyProductFromApi = async (
  dto: createCompanyProductDto
): Promise<companyProductDto> => {
  const formData = new FormData();

  formData.append("CompanyProductName", dto.companyProductName);
  formData.append("EAN", dto.ean);
  formData.append("Category", dto.category);
  formData.append("Description", dto.description || "");

  const priceStr = dto.price.toLocaleString("pl-PL", {
    useGrouping: false,         
    minimumFractionDigits: 0,    
    maximumFractionDigits: 2,    
  });
  formData.append("Price", priceStr);
  formData.append("Currency", dto.currency);    

  formData.append("Stock", String(dto.stock));
  formData.append("IsAvailableForOrder", String(dto.isAvailableForOrder));

  if (dto.imageFile instanceof File) {
    formData.append("ImageFile", dto.imageFile);
  }

  const response = await apiClient.post<companyProductDto>(
    "/CompanyProduct",
    formData
  );

  return response.data;
};

export const deleteCompanyProductFromApi = async (productId : number) : Promise<companyProductDto> => {
  const response = await apiClient.delete<companyProductDto>(`/CompanyProduct/${productId}`);
  return response.data;
}

export type updateCompanyProductDto = {
  companyProductName: string;
  description: string;
  price: number;
  currency: string;
  categoryName: string;
  imageFile: File | null;
  stock: number;
  isAvailableForOrder: boolean;
};

export const putCompanyProductFromApi = async (
  productId: number,
  dto: updateCompanyProductDto
): Promise<companyProductDto> => {
  const formData = new FormData();

  formData.append("CompanyProductName", dto.companyProductName);
  formData.append("Description", dto.description || "");
  formData.append("Category", dto.categoryName);

  const priceStr = dto.price.toLocaleString("pl-PL", {
    useGrouping: false,
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  });
  formData.append("Price", priceStr);
  formData.append("Currency", dto.currency.toUpperCase()); 

  formData.append("Stock", String(dto.stock));
  formData.append("IsAvailableForOrder", String(dto.isAvailableForOrder));

  if (dto.imageFile instanceof File) {
    formData.append("ImageFile", dto.imageFile);
  }

  const response = await apiClient.put<companyProductDto>(
    `/CompanyProduct/${productId}`,
    formData
  );
  return response.data;


};
export const convertToAnotherCurrencyFromApi = async (
  productId: number,
  toCode: string
): Promise<Money> => {
  const response = await apiClient.get<Money>(
    `/CompanyProduct/${productId}/convert?toCode=${toCode}`
  );

  console.log("CONVERT RESPONSE:", response.data); 

  return response.data;
};



