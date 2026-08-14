import PageHeader from '../../components/common/PageHeader'

import Grid from '@mui/material/Grid2'
import axiosInstance from '../../api/axiosInstance'
import { useState, useEffect } from 'react'

import IconButton from '@mui/material/IconButton'
import EditIcon from '@mui/icons-material/Edit'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded';

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
const orderRequestService = {

  getAllOrderRequests: async () => {
    const response = await axiosInstance.get('/OrderRequest');
    return response.data;
  },
  getOrderRequestById: async (id: number) => {
    const response = await axiosInstance.get(`/OrderRequest/${id}`);
    return response.data;
  },
};

export default function Order() {
  const [orders, setOrders] = useState<any[]>([])
  const [orderRequests, setOrderRequests] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  const [selectedOrder, setSelectedOrder] = useState<any | null>(null)

  const [orderId, setOrderId] = useState('')

  const [openOrderViewModal, setOpenOrderViewModal] = useState(false)
  const [openOrderEditModal, setOpenOrderEditModal] = useState(false)

  const [orderSearchTerm, setOrderSearchTerm] = useState('')
  const [orderRequestSearchTerm, setOrderRequestSearchTerm] = useState('')

  const [orderFilter, setOrderFilter] = useState('All')
  const [orderRequestFilter, setOrderRequestFilter] = useState('All')

  const [newOrderStatus, setNewOrderStatus] = useState<string>('')
  const [newOrderRemarks, setNewOrderRemarks] = useState<string>('')

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true)
        // Fetching specific branch stock until auth is implemented
        const orderData = await orderService.getAllOrders()
        const orderRequestsData = await orderRequestService.getAllOrderRequests()
        setOrders(orderData)
        setOrderRequests(orderRequestsData)
      } catch (error) {
        console.error("Failed to load order data", error)
      } finally {
        setLoading(false)
      }
    }
    loadData()
  }, [])

  const filteredOrders = orders.filter(item => {
    const matchesSearch = item.orderId?.toString().includes(orderSearchTerm)
    const matchesFilter = orderFilter === 'All' ? true :
      orderFilter === 'InTransit' ? item.orderStatus?.includes('InTransit') :
        item.orderStatus?.includes('Delivered')
    return matchesSearch && matchesFilter
  })
  const filteredOrderRequests = orderRequests.filter(item => {
    const matchesSearch = item.orderReqId?.toString().includes(orderRequestSearchTerm)
    const matchesFilter = orderRequestFilter === 'All' ? true :
      orderRequestFilter === 'PaymentSuccessful' ? item.status?.includes('PaymentSuccessful') :
        item.status?.includes('Ordered')
    return matchesSearch && matchesFilter
  })

  const handleCreateOrder = async (orderReqId: number) => {
    try {
      await orderService.createOrder(orderReqId)
      alert('Order created successfully!')

      // Refresh data
      const orderData = await orderService.getAllOrders()
      const orderRequestsData = await orderRequestService.getAllOrderRequests()
      setOrders(orderData)
      setOrderRequests(orderRequestsData)
    } catch (error: any) {
      console.error('Failed to create order', error)
      alert('Failed to create order. ' + (error.response?.data?.message || ''))
    }
  }


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

      if (!updateData.orderRemark) {
        console.error('Order remark is required')
        return
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
                <TableCell align="center" className="!font-bold !text-gray-700">Order Date</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Status</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Action</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredOrders.length > 0 ? (
                filteredOrders.map((row) => (
                  <TableRow key={row.orderId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                    <TableCell align="center">{row.orderId}</TableCell>
                    <TableCell align="center">{row.createdOn}</TableCell>
                    <TableCell align="center">
                      {row.orderStatus === 'InTransit' ? (
                        <Chip label="InTransit" color="warning" size="small" className="!font-medium" />
                      ) : (
                        <Chip label="Delivered" color="success" size="small" className="!font-medium" />
                      )}
                    </TableCell>
                    <TableCell align="center">
                      <Stack direction="row" spacing={1} justifyContent="center" divider={<Divider orientation="vertical" flexItem />}>
                        <IconButton color="primary" aria-label="edit order" onClick={() => handleOrderUpdate(row.orderId)}>
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
        </TableContainer>
      </Box>

      {/* order request table */}
      <Box sx={{ mt: 6 }}>
        <Box className="flex justify-between items-center mb-4">
          <Typography variant="h6" className="!font-bold !text-gray-800">
            Order Requests
          </Typography>
          <Box className="flex gap-4">
            <FormControl size="small" className="w-48 bg-white rounded-md">
              <InputLabel id="branch-filter-label">Filter Status</InputLabel>
              <Select
                labelId="branch-filter-label"
                value={orderRequestFilter}
                label="Filter Status"
                onChange={(e) => setOrderRequestFilter(e.target.value)}
              >
                <MenuItem value="All">All Statuses</MenuItem>
                <MenuItem value="TransportAssigned">TransportAssigned</MenuItem>
                <MenuItem value="PaymentSuccessful">PaymentSuccessful</MenuItem>
              </Select>
            </FormControl>
            <TextField
              size="small"
              placeholder="Search by Order Request ID "
              variant="outlined"
              value={orderRequestSearchTerm}
              onChange={(e) => setOrderRequestSearchTerm(e.target.value)}
              className="w-72 bg-white rounded-md"
            />
          </Box>
        </Box>

        <TableContainer sx={{ mt: 1 }} component={Paper} className="rounded-xl shadow-sm overflow-hidden">
          <Table sx={{ minWidth: 650 }}>
            <TableHead className="bg-gray-50">
              <TableRow>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Request ID</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Request Date</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Order Request Status</TableCell>
                <TableCell align="center" className="!font-bold !text-gray-700">Action</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredOrderRequests.length > 0 ? (
                filteredOrderRequests.map((row) => (
                  <TableRow key={row.orderReqId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                    <TableCell align="center">{row.orderReqId}</TableCell>
                    <TableCell align="center">{row.requestedOn}</TableCell>
                    <TableCell align="center">
                      {row.status === 'TransportAssigned' ? (
                        <Chip label="TransportAssigned" color="warning" size="small" className="!font-medium" />
                      ) : (
                        <Chip label="PaymentSuccessful" color="success" size="small" className="!font-medium" />
                      )}
                    </TableCell>
                    <TableCell align="center">
                      <Stack direction="row" spacing={1} justifyContent="center" divider={<Divider orientation="vertical" flexItem />}>
                        <Button variant='contained' color='primary' onClick={() => handleCreateOrder(row.orderReqId)}>
                          Place Order
                        </Button>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={4} align="center" className="!py-8 !text-gray-500">
                    No Order Requests available
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
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
          <Button onClick={() => setOpenOrderViewModal(false)} variant="contained" className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium">Close</Button>
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
