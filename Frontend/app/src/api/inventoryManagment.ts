import apiClient from "./axiosClient";

export enum MovementType {
    Inbound = 0,
    Outbound = 1,
    Adjustment = 2,
}

export type InventoryMovementDto = {
    companyProductId: number;
    date: Date;
    type : MovementType;
    quantity : number;
    comment?: string | null;
}


export const getInventoryMovementsFromApi = async (productId: number)  : Promise<InventoryMovementDto[]> =>{
    const response = await apiClient.get<InventoryMovementDto[]>(`/InventoryMovement/${productId}`);
    return response.data;
};

export const postInventoryMovementFromApi = async (dto: InventoryMovementDto) : Promise<InventoryMovementDto> =>{
    const response = await apiClient.post<InventoryMovementDto>(`/InventoryMovement`, dto);
    return response.data;
};
