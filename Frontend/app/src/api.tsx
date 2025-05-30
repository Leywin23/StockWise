import React from 'react'
import axios from 'axios';

export type Product = {
    productId: number;
    productName: string;
    ean: string;
    image?: string;
    description: string;
    shoppingPrice: number;
    sellingPrice: number;
    category: string;
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

export const getProductsFromApi = async()=>{
    try{
        const response = await axios.get<Product[]>('https://localhost:7178/api/Product');
        return response.data
    }catch(err){
        console.log(err);
    }
}

export const postProductToApi = async(product: ProductDto)=>{
    try{
        const response = await axios.post<Product>('https://localhost:7178/api/Product', product);
        return response.data;
    }catch(err){
        console.log(err);
    }
}
