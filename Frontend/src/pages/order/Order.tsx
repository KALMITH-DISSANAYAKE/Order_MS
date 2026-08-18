import PageHeader from '../../components/common/PageHeader'

import Grid from '@mui/material/Grid2'
import axiosInstance from '../../api/axiosInstance'
import { useState, useEffect } from 'react'

import IconButton from '@mui/material/IconButton'
import EditIcon from '@mui/icons-material/Edit'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';
import { useAuth } from '../../contexts/AuthContext'


import {
  Paper,
  Typography,
  Box,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  Stack,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TablePagination
} from '@mui/material'

const orderService = {
  // Master Items
  getAllOrders: async () => {
    const response = await axiosInstance.get('/orders');
    return response.data;
  },
  createOrder: async (id: number) => {
    const response = await axiosInstance.post(`/orders/from-request/${id}`);
    return response.data;
  },
  getOrderById: async (id: number) => {
    const response = await axiosInstance.get(`/orders/${id}`);
    return response.data;
  },
  updateOrder: async (id: number, updateData: any) => {
    const response = await axiosInstance.put(`/orders/${id}`, updateData);
    return response.data;
  },
  deleteOrder: async (id: number) => {
    const response = await axiosInstance.delete(`/orders/${id}`);
    return response.data;
  },


};
const branchService = {
  // Master Items

  getBranchById: async (id: number) => {
    const response = await axiosInstance.get(`/branches/${id}`);
    return response.data;
  },
  updateOrder: async (id: number, updateData: any) => {
    const response = await axiosInstance.put(`/orders/${id}`, updateData);
    return response.data;
  },

};
export default function Order() {
  const { user } = useAuth()
  const [orders, setOrders] = useState<any[]>([])

  const [selectedOrder, setSelectedOrder] = useState<any | null>(null)

  const [orderId, setOrderId] = useState('')
  const [role, setRole] = useState(user?.role ?? '')
  const [branchCode, setBranchCode] = useState(null);

  const [openOrderViewModal, setOpenOrderViewModal] = useState(false)
  const [openOrderEditModal, setOpenOrderEditModal] = useState(false)

  const [orderSearchTerm, setOrderSearchTerm] = useState('')
  // const [orderRequestSearchTerm, setOrderRequestSearchTerm] = useState('')

  const [orderFilter, setOrderFilter] = useState('All')
  // const [orderRequestFilter, setOrderRequestFilter] = useState('All')

  const [newOrderStatus, setNewOrderStatus] = useState<string>('')
  const [newOrderRemarks, setNewOrderRemarks] = useState<string>('')

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);

  const [monthFilter, setMonthFilter] = useState('All');
  const [yearFilter, setYearFilter] = useState('All');
  const [dateSort, setDateSort] = useState('desc');




  useEffect(() => {
      const loadData = async () => {
        try {
          const orderData = await orderService.getAllOrders()
          setOrders(orderData)
        } catch (error) {
          console.error("Failed to load order data", error)
        }
      }
      loadData()
    }, [])

  useEffect(() => {
      const loadData = async () => {
        try {
          const branchData = await branchService.getBranchById(user?.branchId ?? 0)
          setBranchCode(branchData.branchCode)
        } catch (error) {
          console.error("Failed to load branch data", error)
        }
      }
      loadData()
  }, [])

  useEffect(() => {
    setPage(0);
  }, [
    orderSearchTerm,
    orderFilter,
    monthFilter,
    yearFilter,
    dateSort
  ]);


  const restrictedOrderEditRoles = ['InventoryManager', 'TransportDepartment']

  const filteredOrders = orders.filter((item) => {
    const matchesSearch =
      item.orderId
        ?.toString()
        .includes(orderSearchTerm);

    const matchesFilter =
      orderFilter === 'All'
        ? true
        : orderFilter === 'InTransit'
          ? item.orderStatus?.includes('InTransit')
          : item.orderStatus?.includes('Delivered');
      
   const matchesBranch =
      role === 'BranchManager'
        ? item.orderBranch === branchCode
        : true;

    const orderDate = new Date(item.createdOn);

    const matchesMonth =
      monthFilter === 'All'
        ? true
        : orderDate.getMonth() + 1 === Number(monthFilter);

    const matchesYear =
      yearFilter === 'All'
        ? true
        : orderDate.getFullYear() === Number(yearFilter);

    return (
      matchesSearch &&
      matchesFilter &&
      matchesMonth &&
      matchesYear &&
      matchesBranch
    );
  });

  const sortedOrders = [...filteredOrders].sort((a, b) => {
    const dateA = new Date(a.createdOn).getTime();
    const dateB = new Date(b.createdOn).getTime();

    return dateSort === 'asc'
      ? dateA - dateB
      : dateB - dateA;
  });

  const paginatedOrder = sortedOrders.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );

  const formatDateTime = (dateString: string) => {
    if (!dateString) return '-';

    return new Date(dateString).toLocaleString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true,
    });
  };


  // const handleCreateOrder = async (orderReqId: number) => {
  //   try {
  //     await orderService.createOrder(orderReqId)
  //     alert('Order created successfully!')

  //     // Refresh data
  //     const orderData = await orderService.getAllOrders()
  //     const orderRequestsData = await orderRequestService.getAllOrderRequests()
  //     setOrders(orderData)
  //     setOrderRequests(orderRequestsData)
  //   } catch (error: any) {
  //     console.error('Failed to create order', error)
  //     alert('Failed to create order. ' + (error.response?.data?.message || ''))
  //   }
  // }


  const handleViewOrder = async (id: number) => {
    try {
      setOrderId(id.toString())
      const order = await orderService.getOrderById(id)
      setSelectedOrder(order)
      setOpenOrderViewModal(true)
    } catch (error) {
      console.error('Failed to load order details', error)
    }
  }

  const handleOrderUpdate = async (id: number) => {
    try {
      setOrderId(id.toString())
      const order = await orderService.getOrderById(id)
      setSelectedOrder(order)
      setNewOrderStatus(order?.orderStatus ?? '')
      setNewOrderRemarks(order?.orderRemark ?? '')
      setOpenOrderEditModal(true)
    } catch (error) {
      console.error('Failed to load order details', error)
    }
  }

  const submitOrderUpdate = async () => {
    try {
      if (!orderId) return

      const updateData = {
        orderStatus: newOrderStatus || selectedOrder?.orderStatus || '',
        orderRemark: (newOrderRemarks || selectedOrder?.orderRemark || '').trim(),
      }

      await orderService.updateOrder(Number(orderId), updateData)

      const updatedOrders = await orderService.getAllOrders()
      setOrders(updatedOrders)

      setOpenOrderEditModal(false)
      setSelectedOrder(null)
      setOrderId('')
      setNewOrderStatus('')
      setNewOrderRemarks('')
    } catch (error) {
      console.error('Failed to update order', error)
    }
  }




  return (
    <div>
      <PageHeader title="Orders Management" subtitle="Overview of Orders" />


      {/* order table */}
      <Box>
        <Box className="flex justify-between items-center mb-4">
          <Typography variant="h6" className="!font-bold !text-gray-800">
            Orders
          </Typography>
          <Box className="flex gap-4">
            <FormControl size="small" className="w-48 bg-white rounded-md">
              <InputLabel id="branch-filter-label">Filter Status</InputLabel>
              <Select
                labelId="branch-filter-label"
                value={orderFilter}
                label="Filter Status"
                onChange={(e) => setOrderFilter(e.target.value)}
              >
                <MenuItem value="All">All Statuses</MenuItem>
                <MenuItem value="InTransit">In Transit</MenuItem>
                <MenuItem value="Delivered">Delivered</MenuItem>
              </Select>
            </FormControl>
            <FormControl
              size="small"
              className="w-40 bg-white rounded-md"
            >
              <InputLabel>Month</InputLabel>

              <Select
                value={monthFilter}
                label="Month"
                onChange={(e) => setMonthFilter(e.target.value)}
              >
                <MenuItem value="All">All Months</MenuItem>

                <MenuItem value="1">January</MenuItem>
                <MenuItem value="2">February</MenuItem>
                <MenuItem value="3">March</MenuItem>
                <MenuItem value="4">April</MenuItem>
                <MenuItem value="5">May</MenuItem>
                <MenuItem value="6">June</MenuItem>
                <MenuItem value="7">July</MenuItem>
                <MenuItem value="8">August</MenuItem>
                <MenuItem value="9">September</MenuItem>
                <MenuItem value="10">October</MenuItem>
                <MenuItem value="11">November</MenuItem>
                <MenuItem value="12">December</MenuItem>
              </Select>
            </FormControl>
            <FormControl
              size="small"
              className="w-32 bg-white rounded-md"
            >
              <InputLabel>Year</InputLabel>

              <Select
                value={yearFilter}
                label="Year"
                onChange={(e) => setYearFilter(e.target.value)}
              >
                <MenuItem value="All">All Years</MenuItem>

                <MenuItem value="2024">2024</MenuItem>
                <MenuItem value="2025">2025</MenuItem>
                <MenuItem value="2026">2026</MenuItem>
              </Select>
            </FormControl>
            <FormControl
              size="small"
              className="w-44 bg-white rounded-md"
            >
              <InputLabel>Sort by Date</InputLabel>

              <Select
                value={dateSort}
                label="Sort by Date"
                onChange={(e) => setDateSort(e.target.value)}
              >
                <MenuItem value="desc">
                  Newest First
                </MenuItem>

                <MenuItem value="asc">
                  Oldest First
                </MenuItem>
              </Select>
            </FormControl>
            <TextField
              size="small"
              placeholder="Search by Order ID "
              variant="outlined"
              value={orderSearchTerm}
              onChange={(e) => setOrderSearchTerm(e.target.value)}
              className="w-72 bg-white rounded-md"
            />
          </Box>
        </Box>
        <TableContainer sx={{ mt: 1 }} component={Paper} className="rounded-xl shadow-sm overflow-hidden">
          <Table sx={{ minWidth: 650 }}>
            <TableHead className="bg-gray-50">
              <TableRow>
                <TableCell align="center" className="!font-bold !text-gray-700">Order ID</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Request ID</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Requested By</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Requested Branch</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Date</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Status</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Action</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredOrders.length > 0 ? (
                paginatedOrder.map((row) => (
                  <TableRow key={row.orderId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                    <TableCell align="center">{row.orderId}</TableCell>
                    <TableCell align="center">{row.orderReqId}</TableCell>
                    <TableCell align="center">{row.orderRequestedBy || '-'}</TableCell>
                    <TableCell align="center">{row.orderBranch || '-'}</TableCell>
                    <TableCell align="center">{formatDateTime(row.createdOn)}</TableCell>
                    <TableCell align="center">
                      {row.orderStatus === 'InTransit' ? (
                        <Chip label="InTransit" color="warning" size="small" className="!font-medium" />
                      ) : (
                        <Chip label="Delivered" color="success" size="small" className="!font-medium" />
                      )}
                    </TableCell>
                    <TableCell align="center">
                      <Stack direction="row" spacing={1} justifyContent="center" divider={<Divider orientation="vertical" flexItem />}>
                        <IconButton
                          color="primary"
                          aria-label="edit order"
                          onClick={() => handleOrderUpdate(row.orderId)}
                          disabled={
                            restrictedOrderEditRoles.includes(user?.role ?? '') ||
                            String(row.orderStatus ?? '').trim() === 'Delivered'
                          }
                        >
                          <EditIcon />
                        </IconButton>

                        <IconButton color="primary" aria-label="view order" onClick={() => handleViewOrder(row.orderId)}>
                          <VisibilityRoundedIcon />
                        </IconButton>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={4} align="center" className="!py-8 !text-gray-500">
                    No Orders available
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>

          <TablePagination
            component="div"
            count={filteredOrders.length}
            page={page}
            onPageChange={(_, newPage) => setPage(newPage)}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={(event) => {
              setRowsPerPage(parseInt(event.target.value, 10));
              setPage(0);
            }}
            rowsPerPageOptions={[5, 10, 25]}
          />
        </TableContainer>
        
      </Box>

      <Dialog open={openOrderViewModal} onClose={() => setOpenOrderViewModal(false)} maxWidth="sm" fullWidth>
        <DialogTitle className="!font-bold !text-[#1A1A1A]">View Order {selectedOrder?.orderId ?? orderId}</DialogTitle>
        <DialogContent dividers>
          <Box className="space-y-6 pt-2">
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Order ID" value={selectedOrder?.orderId ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Created Date" value={selectedOrder?.createdOn ? new Date(selectedOrder.createdOn).toLocaleString() : ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Order Status" value={selectedOrder?.orderStatus ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Total Price" value={selectedOrder?.total ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
            </Grid>

            <Box>
              <Typography variant="subtitle1" className="!font-bold !text-gray-800 mb-2">
                Order Lines
              </Typography>
              <TableContainer component={Paper} className="rounded-xl shadow-sm overflow-hidden">
                <Table size="small">
                  <TableHead className="bg-gray-50">
                    <TableRow>
                      <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
                      <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
                      <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
                      <TableCell align="right" className="!font-bold !text-gray-700">Line Price</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {selectedOrder?.orderLines?.length > 0 ? (
                      selectedOrder.orderLines.map((line: any) => (
                        <TableRow key={line.orderlineId}>
                          <TableCell>{line.itemId}</TableCell>
                          <TableCell>{line.itemName}</TableCell>
                          <TableCell align="right">{line.quantity}</TableCell>
                          <TableCell align="right">{line.totalPrice ?? line.unitPrice ?? '-'}</TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={4} align="center" className="!py-6 !text-gray-500">
                          No order lines available
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>

            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Driver Name" value={selectedOrder?.connectionLine?.driversName ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Driver License" value={selectedOrder?.connectionLine?.driverLicenseNumber ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Vehicle Number" value={selectedOrder?.connectionLine?.vehicalNumber ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Vehicle ID" value={selectedOrder?.connectionLine?.vehicalId ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
            </Grid>

            <TextField
              label="Remarks"
              value={selectedOrder?.orderRemark ?? ''}
              variant="outlined"
              fullWidth
              size="small"
              multiline
              minRows={3}
              InputProps={{ readOnly: true }}
            />
          </Box>
        </DialogContent>
        <DialogActions className="p-4">
          <Button onClick={() => { setOpenOrderViewModal(false); setOrderId(''); setSelectedOrder(null); }} color="inherit" className="!normal-case !font-medium">Close</Button>
        </DialogActions>
      </Dialog>



      <Dialog open={openOrderEditModal} onClose={() => setOpenOrderEditModal(false)} maxWidth="sm" fullWidth>
        <DialogTitle className="!font-bold !text-[#1A1A1A]">Edit Order {selectedOrder?.orderId ?? orderId}</DialogTitle>
        <DialogContent dividers>
          <Box className="space-y-6 pt-2">
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Order ID" value={selectedOrder?.orderId ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Created Date" value={selectedOrder?.createdOn ? new Date(selectedOrder.createdOn).toLocaleString() : ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControl size="small" className="w-48 bg-white rounded-md">
                  <InputLabel id="branch-filter-label">Filter Status</InputLabel>
                  <Select
                    labelId="branch-filter-label"
                    value={newOrderStatus ? newOrderStatus : selectedOrder?.orderStatus ?? ''}
                    label="Filter Status"
                    onChange={(e) => setNewOrderStatus(e.target.value)}
                  >
                    <MenuItem value="InTransit">In Transit</MenuItem>
                    <MenuItem value="Delivered">Delivered</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Total Price" value={selectedOrder?.total ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
            </Grid>

            <Box>
              <Typography variant="subtitle1" className="!font-bold !text-gray-800 mb-2">
                Order Lines
              </Typography>
              <TableContainer component={Paper} className="rounded-xl shadow-sm overflow-hidden">
                <Table size="small">
                  <TableHead className="bg-gray-50">
                    <TableRow>
                      <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
                      <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
                      <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
                      <TableCell align="right" className="!font-bold !text-gray-700">Line Price</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {selectedOrder?.orderLines?.length > 0 ? (
                      selectedOrder.orderLines.map((line: any) => (
                        <TableRow key={line.orderlineId}>
                          <TableCell>{line.itemId}</TableCell>
                          <TableCell>{line.itemName}</TableCell>
                          <TableCell align="right">{line.quantity}</TableCell>
                          <TableCell align="right">{line.totalPrice ?? line.unitPrice ?? '-'}</TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={4} align="center" className="!py-6 !text-gray-500">
                          No order lines available
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>

            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Driver Name" value={selectedOrder?.connectionLine?.driversName ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Driver License" value={selectedOrder?.connectionLine?.driverLicenseNumber ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Vehicle Number" value={selectedOrder?.connectionLine?.vehicalNumber ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField label="Vehicle ID" value={selectedOrder?.connectionLine?.vehicalId ?? ''} variant="outlined" fullWidth size="small" InputProps={{ readOnly: true }} />
              </Grid>
            </Grid>

            <TextField
              label="Remarks"
              value={newOrderRemarks ? newOrderRemarks : selectedOrder?.orderRemark ?? ''}
              variant="outlined"
              fullWidth
              size="small"
              multiline
              minRows={3}
              onChange={(e) => setNewOrderRemarks(e.target.value)}
            />
          </Box>
        </DialogContent>
        <DialogActions className="p-4">
          <Button onClick={() => { setOpenOrderEditModal(false); setOrderId(''); setSelectedOrder(null); setNewOrderStatus(''); setNewOrderRemarks(''); }} color="inherit" className="!normal-case !font-medium">Close</Button>
          <Button onClick={submitOrderUpdate} variant="contained" className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium">Update</Button>
        </DialogActions>
      </Dialog>





    </div>
  )
}
