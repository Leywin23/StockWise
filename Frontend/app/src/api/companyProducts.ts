import apiClient from "./axiosClient";

/**
 * Money value object – odpowiada strukturze po stronie backendu
 * (Price.Amount, Price.Currency).
 */
export type Money = {
  amount: number;
  currency: {
    code: string;
  };
};

/**
 * DTO produktu zwracane z backendu.
 * Powinno odpowiadać CompanyProductDto z backendu.
 */
export type companyProductDto = {
  companyProductName: string;
  ean: string;
  categoryName: string;
  image: string | null;
  description: string;
  price: Money;
  stock: number;
  isAvailableForOrder: boolean;
};

/**
 * DTO do tworzenia produktu – to, co wysyłamy z formularza.
 * Backend przyjmuje decimal Price + string Currency,
 * więc tutaj trzymamy to jako osobne pola.
 */
export type createCompanyProductDto = {
  companyProductName: string;
  ean: string;
  category: string;
  imageFile?: File | null;
  description?: string | null;
  price: number;        // tylko kwota
  currency: string;     // np. "PLN"
  stock: number;
  isAvailableForOrder: boolean;
};

/**
 * PageResult<T> – dopasowane do C# PageResult<T>.
 */
export type PageResult<T> = {
  pageSize: number;
  page: number;
  totalCount: number;
  sortDir: number;          // enum SortDir (np. 0/1)
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
  sortDir?: number; // SortDir
};

export type ServiceResult<T> = {
  isSuccess: boolean;
  value: T | null;
  error: number;
  message: string | null;
  details: any;
};

export const getCompanyProductFromApi = async (): Promise<
  companyProductDto[]
> => {
  const response = await apiClient.get<
    ServiceResult<PageResult<companyProductDto>>
  >("/CompanyProduct");

  console.log("GET /CompanyProduct response:", response.data);

  const service = response.data;

  if (!service.isSuccess || !service.value) {
    return [];
  }

  const page = service.value;

  return Array.isArray(page.items) ? page.items : [];
};

export const postCompanyProductFromApi = async (
  dto: createCompanyProductDto
): Promise<companyProductDto> => {
  const formData = new FormData();

  // Nazwy muszą zgadzać się z właściwościami CreateCompanyProductDto po stronie C#
  formData.append("CompanyProductName", dto.companyProductName);
  formData.append("EAN", dto.ean);
  formData.append("Category", dto.category);
  formData.append("Description", dto.description || "");

  // C# ma decimal Price + string Currency
  const priceStr = dto.price.toLocaleString("pl-PL", {
    useGrouping: false,          // bez spacji/tysięcy
    minimumFractionDigits: 0,    // pozwala na 10
    maximumFractionDigits: 2,    // i na 10,99
  });
  formData.append("Price", priceStr);
  formData.append("Currency", dto.currency);     // <- np. "PLN"

  formData.append("Stock", String(dto.stock));
  formData.append("IsAvailableForOrder", String(dto.isAvailableForOrder));

  // Wysyłamy ImageFile tylko, jeśli to faktycznie plik.
  if (dto.imageFile instanceof File) {
    formData.append("ImageFile", dto.imageFile);
  }

  const response = await apiClient.post<companyProductDto>(
    "/CompanyProduct",
    formData
  );

  return response.data;
};
