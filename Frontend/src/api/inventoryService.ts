import axiosInstance from './axiosInstance';

export const inventoryService = {
  // Master Items
  getMasterItems: async () => {
    const response = await axiosInstance.get('/inventory/items');
    return response.data;
  },
  createMasterItem: async (itemData: any) => {
    const response = await axiosInstance.post('/inventory/items', itemData);
    return response.data;
  },

  // Branch Stock
  getAllBranchStock: async () => {
    const response = await axiosInstance.get('/inventory/branch');
    return response.data;
  },
  getBranchStock: async (branchId: number) => {
    const response = await axiosInstance.get(`/inventory/branch/${branchId}`);
    return response.data;
  },
  updateBranchStock: async (stockData: any) => {
    const response = await axiosInstance.put('/inventory/update', stockData);
    return response.data;
  }
};
