import React from 'react'
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
     category: Category;
}

export type ProductDto = {
    productName: string;
    ean: string;
    image?: string;
    description: string;
    shoppingPrice: number;
    sellingPrice: number;
    category: string;
}
type ProductsApiResponse = {
  $id: string;
  $values: ProductWithCategory[]; 
};

export type ProductWithCategory={
    product: Product;
    categoryString: string;
}
export const getProductsFromApi = async (): Promise<ProductWithCategory[]> => {
  try {
    const response = await axios.get<ProductsApiResponse>('https://localhost:7178/api/Product');
    return response.data.$values;
  } catch (err) {
    console.error(err);
    return []; 
  }
};



export const postProductToApi = async(product: ProductDto)=>{
    try{
        const response = await axios.post<Product>('https://localhost:7178/api/Product', product);
        return response.data;
    }catch(err){
        console.log(err);
    }
}

export const getProductsByEanFromApi = async(ean: string) => {
    try{
        const response = await axios.get<Product>(`https://localhost:7178/api/Ean/${ean}`);
        return response.data
    }catch(err){
        console.log(err);
    }
}

export const DeleteProductApi = async(id:number)=>{
    try{
        const response = await axios.delete(`https://localhost:7178/api/product/${id}`);
        return response.data;
    }catch(err){
        console.log(err);
    }
}

export const UpdateProductApi = async(product:Product)=>{
    try{
    const result = await axios.put("https://localhost:7178/api/product", product);
    return result.data;
    }catch(err){
        console.log(err);
    }
}