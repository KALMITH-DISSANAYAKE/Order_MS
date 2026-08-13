export interface User {
  id: number;
  username: string;
  fullName: string;
  role: 'BranchManager' | 'InventoryManager' | 'TransportDepartment' | 'Admin';
  branchId?: number;
  token: string;
}

