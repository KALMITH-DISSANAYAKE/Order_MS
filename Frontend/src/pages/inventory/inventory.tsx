import { useState, useEffect, SyntheticEvent } from 'react'
import { useAuth } from '../../contexts/AuthContext'
import PageHeader from '../../components/common/PageHeader'
import axiosInstance from '../../api/axiosInstance'
import { Box, Tabs, Tab, Button, CircularProgress } from '@mui/material'

import BranchStockTab from '../../components/inventory/BranchStockTab'
import MasterItemsTab from '../../components/inventory/MasterItemsTab'
import BranchStockModal from '../../components/inventory/BranchStockModal'
import MasterItemModal from '../../components/inventory/MasterItemModal'

const inventoryService = {
  // Master Items
  getMasterItems: async () => {
    const response = await axiosInstance.get('/inventory/items');
    return response.data;
  },
  getSuppliers: async () => {
    const response = await axiosInstance.get('/inventory/suppliers');
    return response.data;
  },
  createMasterItem: async (itemData: any) => {
    const response = await axiosInstance.post('/inventory/items', itemData);
    return response.data;
  },
  getMasterItemById: async (id: number) => {
    const response = await axiosInstance.get(`/inventory/items/${id}`);
    return response.data;
  },
  updateMasterItem: async (id: number, itemData: any) => {
    const response = await axiosInstance.put(`/inventory/items/${id}`, itemData);
    return response.data;
  },
  deleteMasterItem: async (id: number) => {
    const response = await axiosInstance.delete(`/inventory/items/${id}`);
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
  addBranchInventory: async (data: any) => {
    const response = await axiosInstance.post('/inventory/branch', data);
    return response.data;
  },
  updateBranchStock: async (data: any) => {
    const response = await axiosInstance.put('/inventory/update', data);
    return response.data;
  },
  deleteBranchStock: async (id: number) => {
    const response = await axiosInstance.delete(`/inventory/branch/${id}`);
    return response.data;
  }
};

export default function Inventory() {
  const { user } = useAuth();
  const isBranchManager = user?.role === 'BranchManager';
  const currentBranchId = user?.branchId || 1;

  const [inventory, setInventory] = useState<any[]>([])
  const [masterItems, setMasterItems] = useState<any[]>([])
  const [allBranchStock, setAllBranchStock] = useState<any[]>([])
  const [suppliers, setSuppliers] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  const [activeTab, setActiveTab] = useState<'branch-stock' | 'items'>(
    isBranchManager ? 'branch-stock' : 'items'
  );
  
  // Modals state
  const [openBranchModal, setOpenBranchModal] = useState(false)
  const [openMasterModal, setOpenMasterModal] = useState(false)

  // Master Item Edit State
  const [newItemName, setNewItemName] = useState('')
  const [newUnitPrice, setNewUnitPrice] = useState<number | string>('')
  const [newSupplierId, setNewSupplierId] = useState<number | string>('')
  const [newItemIsActive, setNewItemIsActive] = useState<boolean>(true)
  const [editingItemId, setEditingItemId] = useState<number | null>(null)

  // Branch Stock Edit State
  const [editingBranchStockId, setEditingBranchStockId] = useState<number | null>(null)
  const [newBranchStockItemId, setNewBranchStockItemId] = useState<number | string>('')
  const [newBranchStockItemName, setNewBranchStockItemName] = useState<string>('')
  const [newBranchStockQuantity, setNewBranchStockQuantity] = useState<number | string>('')
  const [newBranchStockReorderLevel, setNewBranchStockReorderLevel] = useState<number | string>('')

  useEffect(() => {
    setActiveTab(isBranchManager ? 'branch-stock' : 'items');
  }, [isBranchManager]);

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        if (isBranchManager) {
          const [branchData, masterData] = await Promise.all([
            inventoryService.getBranchStock(currentBranchId),
            inventoryService.getMasterItems()
          ]);
          setInventory(branchData || []);
          setMasterItems(masterData || []);
        } else {
          const [masterData, allBranchData, suppliersData] = await Promise.all([
            inventoryService.getMasterItems(),
            inventoryService.getAllBranchStock(),
            inventoryService.getSuppliers()
          ]);
          setMasterItems(masterData || []);
          setAllBranchStock(allBranchData || []);
          setSuppliers(suppliersData || []);
        }
      } catch (error) {
        console.error("Failed to load inventory data", error)
      } finally {
        setLoading(false)
      }
    }
    loadData()
  }, [currentBranchId, isBranchManager])

  const handleTabChange = (_event: SyntheticEvent, newValue: 'branch-stock' | 'items') => {
    setActiveTab(newValue)
  }

  const handleSaveBranchStock = async () => {
    if (newBranchStockItemId === '' || newBranchStockQuantity === '' || newBranchStockReorderLevel === '') {
      alert("Please fill all fields")
      return
    }
    if (Number(newBranchStockQuantity) < 0 || Number(newBranchStockReorderLevel) < 0) {
      alert("Quantity and reorder level cannot be negative.")
      return
    }
    try {
      if (editingBranchStockId) {
        const payload = {
          inventoryId: editingBranchStockId,
          newQuantity: Number(newBranchStockQuantity),
          reorderLevel: Number(newBranchStockReorderLevel)
        }
        await inventoryService.updateBranchStock(payload)
      } else {
        const payload = {
          branchId: currentBranchId,
          itemId: Number(newBranchStockItemId),
          quantity: Number(newBranchStockQuantity),
          reorderLevel: Number(newBranchStockReorderLevel)
        }
        await inventoryService.addBranchInventory(payload)
      }
      setOpenBranchModal(false)
      setEditingBranchStockId(null)
      
      const branchData = await inventoryService.getBranchStock(currentBranchId)
      setInventory(branchData)

      setNewBranchStockItemId('')
      setNewBranchStockItemName('')
      setNewBranchStockQuantity('')
      setNewBranchStockReorderLevel('')
    } catch (error: any) {
      console.error("Failed to save branch stock", error)
      alert("Failed to save branch stock. " + (error.response?.data?.message || error.response?.data?.Message || "Make sure Branch ID and Item ID are valid."))
    }
  }

  const handleOpenBranchEdit = (row: any) => {
    setEditingBranchStockId(row.inventoryId);
    setNewBranchStockItemId(row.itemId);
    setNewBranchStockItemName(row.itemName || '');
    setNewBranchStockQuantity(row.quantity);
    setNewBranchStockReorderLevel(row.reorderLevel);
    setOpenBranchModal(true);
  }

  const handleDeleteBranchStock = async (id: number) => {
    if (window.confirm("Are you sure you want to delete this branch stock?")) {
      try {
        await inventoryService.deleteBranchStock(id);
        const branchData = await inventoryService.getBranchStock(currentBranchId);
        setInventory(branchData);
      } catch (error) {
        console.error("Failed to delete branch stock", error);
        alert("Failed to delete branch stock.");
      }
    }
  }

  const handleOpenEdit = async (item: any) => {
    try {
      const fullItem = await inventoryService.getMasterItemById(item.itemId);
      setEditingItemId(fullItem.itemId);
      setNewItemName(fullItem.itemName);
      setNewUnitPrice(fullItem.unitPrice);
      setNewSupplierId(fullItem.supplier?.supplierId || '');
      setNewItemIsActive(fullItem.isActive ?? item.isActive ?? true);
      setOpenMasterModal(true);
    } catch (e) {
      console.error(e);
      alert("Failed to load item details.");
    }
  }

  const handleDeleteMasterItem = async (id: number) => {
    if (window.confirm("Are you sure you want to delete this item?")) {
      try {
        await inventoryService.deleteMasterItem(id);
        const masterData = await inventoryService.getMasterItems();
        setMasterItems(masterData);
      } catch (error) {
        console.error("Failed to delete", error);
        alert("Failed to delete item.");
      }
    }
  }

  const handleSaveMasterItem = async () => {
    if (!newItemName.trim() || newUnitPrice === '' || newSupplierId === '') {
      alert("Please fill all fields")
      return
    }
    if (Number(newUnitPrice) <= 0 || isNaN(Number(newUnitPrice))) {
      alert("Unit price must be greater than 0.")
      return
    }
    try {
      const payload: any = {
        itemName: newItemName,
        unitPrice: Number(newUnitPrice),
        supplierId: Number(newSupplierId),
      }
      
      if (editingItemId) {
        payload.isActive = newItemIsActive;
        await inventoryService.updateMasterItem(editingItemId, payload)
      } else {
        await inventoryService.createMasterItem(payload)
      }
      
      setOpenMasterModal(false)
      setEditingItemId(null)

      const masterData = await inventoryService.getMasterItems()
      setMasterItems(masterData)
      
      setNewItemName('')
      setNewUnitPrice('')
      setNewSupplierId('')
      setNewItemIsActive(true)
    } catch (error: any) {
      console.error("Failed to save item", error)
      alert("Failed to save item. " + (error.response?.data?.message || "Make sure Supplier ID is valid."))
    }
  }

  if (loading) {
    return (
      <Box className="flex justify-center items-center h-64">
        <CircularProgress sx={{ color: '#E21E26' }} />
      </Box>
    )
  }

  return (
    <div>
      <PageHeader
        title="Inventory Management"
        subtitle={isBranchManager ? "Monitor and manage your branch stock levels" : "Manage master items and monitor branch stock levels"}
        action={
          isBranchManager ? (
            <Button
              variant="contained"
              onClick={() => {
                setEditingBranchStockId(null)
                setNewBranchStockItemId('')
                setNewBranchStockItemName('')
                setNewBranchStockQuantity('')
                setNewBranchStockReorderLevel('')
                setOpenBranchModal(true)
              }}
              className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium !rounded-lg"
            >
              Add Branch Stock
            </Button>
          ) : (
            <Button
              variant="contained"
              onClick={() => {
                setEditingItemId(null);
                setNewItemName('');
                setNewUnitPrice('');
                setNewSupplierId('');
                setNewItemIsActive(true);
                setOpenMasterModal(true);
              }}
              className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium !rounded-lg"
            >
              Add Item
            </Button>
          )
        }
      />

      <Box sx={{ borderBottom: 1, borderColor: 'divider' }} className="mb-6">
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          aria-label="inventory tabs"
          TabIndicatorProps={{ className: '!bg-[#E21E26]' }}
        >
          {isBranchManager && (
            <Tab
              value="branch-stock"
              label="Branch Stock"
              className={activeTab === 'branch-stock' ? '!text-[#E21E26] !font-bold' : '!font-medium'}
            />
          )}
          {!isBranchManager && (
            <Tab
              value="items"
              label="Items"
              className={activeTab === 'items' ? '!text-[#E21E26] !font-bold' : '!font-medium'}
            />
          )}
        </Tabs>
      </Box>

      {isBranchManager && activeTab === 'branch-stock' && (
        <BranchStockTab 
          inventory={inventory} 
          onOpenEdit={handleOpenBranchEdit} 
          onDelete={handleDeleteBranchStock} 
        />
      )}

      {!isBranchManager && activeTab === 'items' && (
        <MasterItemsTab 
          masterItems={masterItems} 
          allBranchStock={allBranchStock} 
          onOpenEdit={handleOpenEdit} 
          onDelete={handleDeleteMasterItem} 
        />
      )}

      <BranchStockModal 
        open={openBranchModal}
        onClose={() => setOpenBranchModal(false)}
        onSave={handleSaveBranchStock}
        editingId={editingBranchStockId}
        itemId={newBranchStockItemId}
        setItemId={setNewBranchStockItemId}
        itemName={newBranchStockItemName}
        masterItems={masterItems}
        quantity={newBranchStockQuantity}
        setQuantity={setNewBranchStockQuantity}
        reorderLevel={newBranchStockReorderLevel}
        setReorderLevel={setNewBranchStockReorderLevel}
      />

      <MasterItemModal 
        open={openMasterModal}
        onClose={() => { setOpenMasterModal(false); setEditingItemId(null); }}
        onSave={handleSaveMasterItem}
        editingId={editingItemId}
        itemName={newItemName}
        setItemName={setNewItemName}
        unitPrice={newUnitPrice}
        setUnitPrice={setNewUnitPrice}
        supplierId={newSupplierId}
        setSupplierId={setNewSupplierId}
        suppliers={suppliers}
        isActive={newItemIsActive}
        setIsActive={setNewItemIsActive}
      />
    </div>
  )
}