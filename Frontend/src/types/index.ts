export interface User {
  id: number
  username: string
  fullName: string
  role: 'BranchManager' | 'InventoryManager' | 'TransportDepartment' | 'Admin'
  branchId?: number
  token: string
}

export interface LoginCredentials {
  username: string
  password: string
}

export interface RegisterData {
  firstName: string
  lastName: string
  username: string
  password: string
  roleId: number
  branchId?: number
}
