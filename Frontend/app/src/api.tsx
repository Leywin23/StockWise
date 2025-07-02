import axios from 'axios';

export type Category = {
  categoryId: number;
  name: string;
  parent?: Category | null;
}

export type Product = {
  productId: number;
  productName: string;
  ean: string;
  image?: string;
  description: string;
  shoppingPrice: number;
  sellingPrice: number;
  categoryId: number;
  category?: Category;

};

export type ProductDto = {
  productName: string;
  ean: string;
  image?: string;
  description: string;
  shoppingPrice: number;
  sellingPrice: number;
  category: string;
};

export type ProductWithCategory = {
  product: Product;
  categoryString: string;
};

type ProductsApiResponse = {
  $id: string;
  $values: ProductWithCategory[];
};

export const getProductsFromApi = async (): Promise<ProductWithCategory[]> => {
  try {
    const response = await axios.get<ProductsApiResponse>('https://localhost:7178/api/Product');
    return response.data.$values;
  } catch (err) {
    console.error(err);
    return [];
  }
};

export const postProductToApi = async (product: ProductDto) => {
  try {
    const response = await axios.post<Product>('https://localhost:7178/api/Product', product);
    return response.data;
  } catch (err) {
    console.log(err);
  }
};

export const getProductsByEanFromApi = async (ean: string) => {
  try {
    const response = await axios.get<Product>(`https://localhost:7178/api/Ean/${ean}`);
    return response.data;
  } catch (err) {
    console.log(err);
  }
};

export const DeleteProductApi = async (id: number) => {
  try {
    const response = await axios.delete(`https://localhost:7178/api/product/${id}`);
    return response.data;
  } catch (err) {
    console.log(err);
  }
};

export type UpdateProductDto = {
  productId: number;
  productName: string;
  ean: string;
  image?: string;
  description: string;
  shoppingPrice: number;
  sellingPrice: number;
  categoryId: number;
};

export const UpdateProductApi = async (product: UpdateProductDto) => {
  try {
    const result = await axios.put("https://localhost:7178/api/product", product);
    return result.data;
  } catch (err: any) {
    if (err.response && err.response.data) {
      console.error("Backend returned 400:", err.response.data);
      console.error("Validation errors:", err.response.data.errors);
    }
    throw err;
  }
};


export type InventoryMovementDto = {
  Date:Date;
  Type:string;
  ProductId: number;
  Quantity: number;
  Comment: string;

}

export const AddMovementFromApi = async(movement: InventoryMovementDto)=>{
  try{
    const result = await axios.post("https://localhost:7178/api/inventorymovement", movement);
    return result.data;
  }
  catch(err){
    console.log(`Error with fetching Api ${err}`);
  }
}