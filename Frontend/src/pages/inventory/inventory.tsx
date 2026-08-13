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
  const currentBranchId = user?.branchId || 1;

  const [inventory, setInventory] = useState<any[]>([])
  const [masterItems, setMasterItems] = useState<any[]>([])
  const [allBranchStock, setAllBranchStock] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  const [tabIndex, setTabIndex] = useState(0)
  
  // Modals state
  const [openBranchModal, setOpenBranchModal] = useState(false)
  const [openMasterModal, setOpenMasterModal] = useState(false)

  // Master Item Edit State
  const [newItemName, setNewItemName] = useState('')
  const [newUnitPrice, setNewUnitPrice] = useState<number | string>('')
  const [newSupplierId, setNewSupplierId] = useState<number | string>('')
  const [editingItemId, setEditingItemId] = useState<number | null>(null)

  // Branch Stock Edit State
  const [editingBranchStockId, setEditingBranchStockId] = useState<number | null>(null)
  const [newBranchStockItemId, setNewBranchStockItemId] = useState<number | string>('')
  const [newBranchStockQuantity, setNewBranchStockQuantity] = useState<number | string>('')
  const [newBranchStockReorderLevel, setNewBranchStockReorderLevel] = useState<number | string>('')

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        const branchData = await inventoryService.getBranchStock(currentBranchId)
        const masterData = await inventoryService.getMasterItems()
        const allBranchData = await inventoryService.getAllBranchStock()
        setInventory(branchData)
        setMasterItems(masterData)
        setAllBranchStock(allBranchData)
      } catch (error) {
        console.error("Failed to load inventory data", error)
      } finally {
        setLoading(false)
      }
    }
    loadData()
  }, [currentBranchId])

  const handleTabChange = (_event: SyntheticEvent, newValue: number) => {
    setTabIndex(newValue)
  }

  const handleSaveBranchStock = async () => {
    if (newBranchStockItemId === '' || newBranchStockQuantity === '' || newBranchStockReorderLevel === '') {
      alert("Please fill all fields")
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
      const allBranchData = await inventoryService.getAllBranchStock()
      setInventory(branchData)
      setAllBranchStock(allBranchData)

      setNewBranchStockItemId('')
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
    setNewBranchStockQuantity(row.quantity);
    setNewBranchStockReorderLevel(row.reorderLevel);
    setOpenBranchModal(true);
  }

  const handleDeleteBranchStock = async (id: number) => {
    if (window.confirm("Are you sure you want to delete this branch stock?")) {
      try {
        await inventoryService.deleteBranchStock(id);
        const branchData = await inventoryService.getBranchStock(currentBranchId);
        const allBranchData = await inventoryService.getAllBranchStock();
        setInventory(branchData);
        setAllBranchStock(allBranchData);
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
    if (!newItemName || newUnitPrice === '' || newSupplierId === '') {
      alert("Please fill all fields")
      return
    }
    try {
      const payload = {
        itemName: newItemName,
        unitPrice: Number(newUnitPrice),
        supplierId: Number(newSupplierId),
      }
      
      if (editingItemId) {
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
        subtitle="Manage master items and monitor branch stock levels"
        action={
          tabIndex === 0 ? (
            <Button
              variant="contained"
              onClick={() => {
                setEditingBranchStockId(null)
                setNewBranchStockItemId('')
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
        <Tabs value={tabIndex} onChange={handleTabChange} aria-label="inventory tabs" TabIndicatorProps={{ className: '!bg-[#E21E26]' }}>
          <Tab label="Branch Stock" className={tabIndex === 0 ? '!text-[#E21E26] !font-bold' : '!font-medium'} />
          <Tab label="Items" className={tabIndex === 1 ? '!text-[#E21E26] !font-bold' : '!font-medium'} />
        </Tabs>
      </Box>

      {tabIndex === 0 && (
        <BranchStockTab 
          inventory={inventory} 
          onOpenEdit={handleOpenBranchEdit} 
          onDelete={handleDeleteBranchStock} 
        />
      )}

      {tabIndex === 1 && (
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
      />
    </div>
  )
}