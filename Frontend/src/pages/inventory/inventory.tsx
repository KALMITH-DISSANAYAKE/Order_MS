import { useState, useEffect, SyntheticEvent } from 'react'
import PageHeader from '../../components/common/PageHeader'
import axiosInstance from '../../api/axiosInstance'
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  Tabs,
  Tab,
  Box,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Typography
} from '@mui/material'
import IconButton from '@mui/material/IconButton'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'


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
  const [inventory, setInventory] = useState<any[]>([])
  const [masterItems, setMasterItems] = useState<any[]>([])
  const [allBranchStock, setAllBranchStock] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  const [tabIndex, setTabIndex] = useState(0)
  const [openBranchModal, setOpenBranchModal] = useState(false)
  const [openMasterModal, setOpenMasterModal] = useState(false)
  const [branchSearchTerm, setBranchSearchTerm] = useState('')
  const [masterSearchTerm, setMasterSearchTerm] = useState('')
  const [allBranchSearchTerm, setAllBranchSearchTerm] = useState('')
  const [branchFilter, setBranchFilter] = useState('All')
  const [masterFilter, setMasterFilter] = useState('All')
  const [allBranchFilter, setAllBranchFilter] = useState('All')

  const [newItemName, setNewItemName] = useState('')
  const [newUnitPrice, setNewUnitPrice] = useState<number | string>('')
  const [newSupplierId, setNewSupplierId] = useState<number | string>('')
  const [editingItemId, setEditingItemId] = useState<number | null>(null)

  const [editingBranchStockId, setEditingBranchStockId] = useState<number | null>(null)
  const [newBranchStockItemId, setNewBranchStockItemId] = useState<number | string>('')
  const [newBranchStockQuantity, setNewBranchStockQuantity] = useState<number | string>('')
  const [newBranchStockReorderLevel, setNewBranchStockReorderLevel] = useState<number | string>('')

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        // Fetching specific branch stock until auth is implemented
        const branchData = await inventoryService.getBranchStock(1)
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
  }, [])

  const filteredBranchStock = inventory.filter(item => {
    const matchesSearch = item.itemName?.toLowerCase().includes(branchSearchTerm.toLowerCase()) ||
      item.branchCode?.toLowerCase().includes(branchSearchTerm.toLowerCase()) ||
      item.itemId?.toString().includes(branchSearchTerm)
    const matchesFilter = branchFilter === 'All' ? true :
      branchFilter === 'Low Stock' ? item.isBelowReorderLevel :
        !item.isBelowReorderLevel
    return matchesSearch && matchesFilter
  })

  const filteredMasterItems = masterItems.filter(item => {
    const matchesSearch = item.itemName?.toLowerCase().includes(masterSearchTerm.toLowerCase()) ||
      item.supplierName?.toLowerCase().includes(masterSearchTerm.toLowerCase()) ||
      item.itemId?.toString().includes(masterSearchTerm)
    const matchesFilter = masterFilter === 'All' ? true :
      masterFilter === 'Active' ? item.isActive :
        !item.isActive
    return matchesSearch && matchesFilter
  })

  const filteredAllBranchStock = allBranchStock.filter(item => {
    const matchesSearch = item.itemName?.toLowerCase().includes(allBranchSearchTerm.toLowerCase()) ||
      item.branchCode?.toLowerCase().includes(allBranchSearchTerm.toLowerCase()) ||
      item.branchLocation?.toLowerCase().includes(allBranchSearchTerm.toLowerCase()) ||
      item.itemId?.toString().includes(allBranchSearchTerm)
    const matchesFilter = allBranchFilter === 'All' ? true :
      allBranchFilter === 'Low Stock' ? item.isBelowReorderLevel :
        !item.isBelowReorderLevel
    return matchesSearch && matchesFilter
  })

  const groupedBranchStock = filteredAllBranchStock.reduce((acc, item) => {
    const branchKey = `${item.branchCode || 'Unknown'} - ${item.branchLocation || 'Unknown'}`
    if (!acc[branchKey]) {
      acc[branchKey] = []
    }
    acc[branchKey].push(item)
    return acc
  }, {} as Record<string, any[]>)

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
          branchId: 1, // Hardcoded until auth is implemented
          itemId: Number(newBranchStockItemId),
          quantity: Number(newBranchStockQuantity),
          reorderLevel: Number(newBranchStockReorderLevel)
        }
        await inventoryService.addBranchInventory(payload)
      }
      setOpenBranchModal(false)
      setEditingBranchStockId(null)
      
      const branchData = await inventoryService.getBranchStock(1)
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
        const branchData = await inventoryService.getBranchStock(1);
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
        <Box>
          <Box className="flex justify-between items-center mb-4">
            <Typography variant="h6" className="!font-bold !text-gray-800">
              {inventory.length > 0 ? `${inventory[0].branchLocation} (${inventory[0].branchCode})` : 'Branch Stock'}
            </Typography>
            <Box className="flex gap-4">
              <FormControl size="small" className="w-48 bg-white rounded-md">
                <InputLabel id="branch-filter-label">Filter Status</InputLabel>
                <Select
                  labelId="branch-filter-label"
                  value={branchFilter}
                  label="Filter Status"
                  onChange={(e) => setBranchFilter(e.target.value)}
                >
                  <MenuItem value="All">All Statuses</MenuItem>
                  <MenuItem value="In Stock">In Stock</MenuItem>
                  <MenuItem value="Low Stock">Low Stock</MenuItem>
                </Select>
              </FormControl>
              <TextField
                size="small"
                placeholder="Search by Item Name, ID or Branch..."
                variant="outlined"
                value={branchSearchTerm}
                onChange={(e) => setBranchSearchTerm(e.target.value)}
                className="w-72 bg-white rounded-md"
              />
            </Box>
          </Box>
          <TableContainer component={Paper} className="rounded-xl shadow-sm overflow-hidden">
            <Table sx={{ minWidth: 650 }}>
              <TableHead className="bg-gray-50">
                <TableRow>
                  <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
                  <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
                  <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
                  <TableCell align="right" className="!font-bold !text-gray-700">Reorder Level</TableCell>
                  <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
                  <TableCell align="center" className="!font-bold !text-gray-700">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredBranchStock.map((row) => (
                  <TableRow key={row.inventoryId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                    <TableCell>{row.itemId}</TableCell>
                    <TableCell>{row.itemName}</TableCell>
                    <TableCell align="right">{row.quantity}</TableCell>
                    <TableCell align="right">{row.reorderLevel}</TableCell>
                    <TableCell align="center">
                      {row.isBelowReorderLevel ? (
                        <Chip label="Low Stock" color="error" size="small" className="!font-medium" />
                      ) : (
                        <Chip label="In Stock" color="success" size="small" className="!font-medium" />
                      )}
                    </TableCell>
                    <TableCell align="center">
                      <IconButton size="small" color="primary" onClick={() => handleOpenBranchEdit(row)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" color="error" onClick={() => handleDeleteBranchStock(row.inventoryId)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}


      {tabIndex === 1 && (
        <Box>
          <Box className="flex justify-end gap-4 mb-4">
            <FormControl size="small" className="w-48 bg-white rounded-md">
              <InputLabel id="master-filter-label">Filter Status</InputLabel>
              <Select
                labelId="master-filter-label"
                value={masterFilter}
                label="Filter Status"
                onChange={(e) => setMasterFilter(e.target.value)}
              >
                <MenuItem value="All">All Statuses</MenuItem>
                <MenuItem value="Active">Active</MenuItem>
                <MenuItem value="Inactive">Inactive</MenuItem>
              </Select>
            </FormControl>
            <TextField
              size="small"
              placeholder="Search by Item Name, ID or Supplier..."
              variant="outlined"
              value={masterSearchTerm}
              onChange={(e) => setMasterSearchTerm(e.target.value)}
              className="w-72 bg-white rounded-md"
            />
          </Box>
          <TableContainer component={Paper} className="rounded-xl shadow-sm overflow-hidden">
            <Table sx={{ minWidth: 650 }}>
              <TableHead className="bg-gray-50">
                <TableRow>
                  <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
                  <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
                  <TableCell className="!font-bold !text-gray-700">Supplier</TableCell>
                  <TableCell align="right" className="!font-bold !text-gray-700">Unit Price (LKR)</TableCell>
                  <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
                  <TableCell align="center" className="!font-bold !text-gray-700">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredMasterItems.map((row) => (
                  <TableRow key={row.itemId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                    <TableCell>{row.itemId}</TableCell>
                    <TableCell className="!font-medium">{row.itemName}</TableCell>
                    <TableCell>{row.supplierName}</TableCell>
                    <TableCell align="right">{row.unitPrice.toFixed(2)}</TableCell>
                    <TableCell align="center">
                      {row.isActive ? (
                        <Chip label="Active" color="success" size="small" className="!font-medium" />
                      ) : (
                        <Chip label="Inactive" color="default" size="small" className="!font-medium" />
                      )}
                    </TableCell>
                    <TableCell align="center">
                      <IconButton size="small" color="primary" onClick={() => handleOpenEdit(row)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" color="error" onClick={() => handleDeleteMasterItem(row.itemId)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          <Box className="mt-10">
            <Box className="flex justify-between items-center mb-4">
              <Typography variant="h6" className="!font-bold !text-gray-800">
                All Branch Stock
              </Typography>
              <Box className="flex gap-4">
                <FormControl size="small" className="w-48 bg-white rounded-md">
                  <InputLabel id="all-branch-filter-label">Filter Status</InputLabel>
                  <Select
                    labelId="all-branch-filter-label"
                    value={allBranchFilter}
                    label="Filter Status"
                    onChange={(e) => setAllBranchFilter(e.target.value)}
                  >
                    <MenuItem value="All">All Statuses</MenuItem>
                    <MenuItem value="In Stock">In Stock</MenuItem>
                    <MenuItem value="Low Stock">Low Stock</MenuItem>
                  </Select>
                </FormControl>
                <TextField
                  size="small"
                  placeholder="Search by Item Name, ID, or Branch..."
                  variant="outlined"
                  value={allBranchSearchTerm}
                  onChange={(e) => setAllBranchSearchTerm(e.target.value)}
                  className="w-72 bg-white rounded-md"
                />
              </Box>
            </Box>
            {Object.keys(groupedBranchStock).length === 0 ? (
              <Typography className="text-gray-500 italic py-4 text-center border rounded-lg bg-gray-50">
                No stock found for selected filters.
              </Typography>
            ) : (
              Object.entries(groupedBranchStock).map(([branchName, items]) => (
                <Box key={branchName} className="mb-6">
                  <Typography variant="subtitle1" className="!font-bold !text-gray-700 mb-3 bg-gray-100 p-2 rounded-t-lg border border-b-0 border-gray-200">
                    {branchName}
                  </Typography>
                  <TableContainer component={Paper} className="shadow-sm overflow-hidden border border-gray-200 rounded-b-lg rounded-t-none">
                    <Table sx={{ minWidth: 650 }}>
                      <TableHead className="bg-gray-50">
                        <TableRow>
                          <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
                          <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
                          <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
                          <TableCell align="right" className="!font-bold !text-gray-700">Reorder Level</TableCell>
                          <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {(items as any[]).map((row: any) => (
                          <TableRow key={row.inventoryId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                            <TableCell>{row.itemId}</TableCell>
                            <TableCell>{row.itemName}</TableCell>
                            <TableCell align="right">{row.quantity}</TableCell>
                            <TableCell align="right">{row.reorderLevel}</TableCell>
                            <TableCell align="center">
                              {row.isBelowReorderLevel ? (
                                <Chip label="Low Stock" color="error" size="small" className="!font-medium" />
                              ) : (
                                <Chip label="In Stock" color="success" size="small" className="!font-medium" />
                              )}
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </Box>
              ))
            )}
          </Box>
        </Box>
      )}


      <Dialog open={openBranchModal} onClose={() => setOpenBranchModal(false)} maxWidth="sm" fullWidth>
        <DialogTitle className="!font-bold !text-[#1A1A1A]">
          {editingBranchStockId ? "Edit Branch Stock" : "Add Branch Stock"}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={3} className="pt-2">
            <Grid item xs={12}>
              <TextField label="Item ID" type="number" variant="outlined" fullWidth size="small" disabled={!!editingBranchStockId} value={newBranchStockItemId} onChange={(e) => setNewBranchStockItemId(e.target.value)} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Quantity" type="number" variant="outlined" fullWidth size="small" value={newBranchStockQuantity} onChange={(e) => setNewBranchStockQuantity(e.target.value)} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Reorder Level" type="number" variant="outlined" fullWidth size="small" value={newBranchStockReorderLevel} onChange={(e) => setNewBranchStockReorderLevel(e.target.value)} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions className="p-4">
          <Button onClick={() => setOpenBranchModal(false)} color="inherit" className="!normal-case !font-medium">Cancel</Button>
          <Button onClick={handleSaveBranchStock} variant="contained" className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium">Save Stock</Button>
        </DialogActions>
      </Dialog>


      <Dialog open={openMasterModal} onClose={() => setOpenMasterModal(false)} maxWidth="sm" fullWidth>
        <DialogTitle className="!font-bold !text-[#1A1A1A]">
          {editingItemId ? "Edit Master Item" : "Create Master Item"}
        </DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={3} className="pt-2">
            <Grid item xs={12}>
              <TextField 
                label="Item Name" 
                variant="outlined" 
                fullWidth 
                size="small" 
                value={newItemName}
                onChange={(e) => setNewItemName(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Unit Price" type="number" variant="outlined" fullWidth size="small" value={newUnitPrice}
                onChange={(e) => setNewUnitPrice(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField label="Supplier ID" type="number" variant="outlined" fullWidth size="small" value={newSupplierId}
                onChange={(e) => setNewSupplierId(e.target.value)}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions className="p-4">
          <Button onClick={() => { setOpenMasterModal(false); setEditingItemId(null); }} color="inherit" className="!normal-case !font-medium">Cancel</Button>
          <Button onClick={handleSaveMasterItem} variant="contained" className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium">
            {editingItemId ? "Update Item" : "Create Item"}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  )
}