import axiosInstance from './axiosInstance'

export interface OrderRequestItem {
  itemId: number
  itemName: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export interface OrderRequest {
  orderReqId: number
  status: string
  totalQuantity: number
  totalPrice: number
  requestedOn: string
  requestedBy: string  // Change to string (username)
  items?: OrderRequestItem[]
}

export interface CreateOrderRequestItem {
  itemId: number
  quantity: number
}

export interface CreateOrderRequestPayload {
  requestedBy: number
  items: CreateOrderRequestItem[]
}

export interface ApproveOrderRequestPayload {
  approvedBy: number
}

export interface Item {
  itemId: number
  itemName: string
  unitPrice?: number
}

export const orderRequestApi = {
  async getAll(): Promise<OrderRequest[]> {
    const response = await axiosInstance.get<OrderRequest[]>(
      '/OrderRequest'
    )

    return response.data
  },

  async getById(id: number): Promise<OrderRequest> {
    const response = await axiosInstance.get<OrderRequest>(
      `/OrderRequest/${id}`
    )

    return response.data
  },

  async create(
    payload: CreateOrderRequestPayload
  ): Promise<OrderRequest> {
    const response = await axiosInstance.post<OrderRequest>(
      '/OrderRequest',
      payload
    )

    return response.data
  },

  async approve(
    id: number,
    payload: ApproveOrderRequestPayload
  ): Promise<OrderRequest> {
    const response = await axiosInstance.put<OrderRequest>(
      `/OrderRequest/${id}/approve`,
      payload
    )

    return response.data
  },

  async reject(id: number): Promise<OrderRequest> {
    const response = await axiosInstance.put<OrderRequest>(
      `/OrderRequest/${id}/reject`
    )

    return response.data
  },

  async payment(id: number): Promise<OrderRequest> {
    const response = await axiosInstance.put<OrderRequest>(
      `/OrderRequest/${id}/payment`
    )

    return response.data
  },

  /*
   * Assumption:
   * GET /Item returns the available items.
   *
   * Change only this endpoint if your backend
   * uses a different Item URL.
   */
  async getItems(): Promise<Item[]> {
    const response = await axiosInstance.get<Item[]>('/Inventory/Items')

    return response.data
  },
}