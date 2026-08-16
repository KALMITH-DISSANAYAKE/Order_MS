import { useState, useEffect } from 'react'
import {
  Box,
  Button,
  Paper,
  Chip,
  Typography,
} from '@mui/material'
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid'
import { AssignmentInd } from '@mui/icons-material'
import PageHeader from '../../components/common/PageHeader'
import AssignmentModal, { AssignmentData } from '../../components/delivery/AssignmentModal'
import axiosInstance from '../../api/axiosInstance'

interface Delivery {
  id: string
  orderId: string
  destination: string
  driver: string
  vehicle: string
  status: 'Pending' | 'Assigned' | 'In Transit' | 'Delivered'
}

export default function DeliveryList() {
  const [deliveries, setDeliveries] = useState<Delivery[]>([])

  // Modal state
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [selectedDeliveryId, setSelectedDeliveryId] = useState<string | null>(null)

  const fetchDeliveries = async () => {
    try {
      const response = await axiosInstance.get('/Delivery')
      const mapped = response.data.map((d: any) => ({
        id: d.orderReqId.toString(),
        orderId: d.orderReqId.toString(),
        destination: d.branchLocation || '',
        driver: d.driverName || '',
        vehicle: d.vehicleNumber || '',
        status: d.orderStatus === 'Approved' ? 'Pending'
          : d.orderStatus === 'TransportAssigned' ? 'Assigned'
            : d.orderStatus
      }))
      setDeliveries(mapped)
    } catch (error) {
      console.error('Failed to fetch deliveries:', error)
    }
  }

  useEffect(() => {
    fetchDeliveries()
  }, [])

  const handleOpenAssignment = (id: string) => {
    setSelectedDeliveryId(id)
    setIsModalOpen(true)
  }

  const handleCloseAssignment = () => {
    setIsModalOpen(false)
    setSelectedDeliveryId(null)
  }

  const handleAssign = async (data: AssignmentData) => {
    if (selectedDeliveryId && data.connectionId) {
      try {
        await axiosInstance.put(`/Delivery/${selectedDeliveryId}/assign`, {
          connectionId: data.connectionId
        })
        fetchDeliveries()
      } catch (error) {
        console.error('Failed to assign delivery:', error)
      }
    }
  }

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'Delivery ID', width: 130 },
    { field: 'destination', headerName: 'Destination', flex: 1, minWidth: 150 },
    {
      field: 'driver',
      headerName: 'Driver',
      flex: 1,
      minWidth: 150,
      renderCell: (params: GridRenderCellParams) => (
        <Typography className="!text-sm pt-4">
          {params.value || <span className="italic text-gray-400">Unassigned</span>}
        </Typography>
      )
    },
    {
      field: 'vehicle',
      headerName: 'Vehicle',
      flex: 1,
      minWidth: 180,
      renderCell: (params: GridRenderCellParams) => (
        <Typography className="!text-sm pt-4">
          {params.value || <span className="italic text-gray-400">Unassigned</span>}
        </Typography>
      )
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (params: GridRenderCellParams) => {
        const statusColors: Record<string, 'default' | 'primary' | 'warning' | 'success'> = {
          'Pending': 'default',
          'Assigned': 'primary',
          'In Transit': 'warning',
          'Delivered': 'success'
        }
        const color = statusColors[params.value as string] || 'default'
        return (
          <Chip
            label={params.value}
            color={color}
            size="small"
            className="!font-medium"
            variant={color === 'default' ? 'outlined' : 'filled'}
          />
        )
      },
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 150,
      sortable: false,
      renderCell: (params: GridRenderCellParams) => (
        <Box>
          {params.row.status === 'Pending' ? (
            <Button
              size="small"
              variant="outlined"
              startIcon={<AssignmentInd />}
              onClick={() => handleOpenAssignment(params.row.id)}
              className="!text-xs !py-0.5"
            >
              Assign
            </Button>
          ) : (
            <Typography variant="body2" className="italic text-gray-400">
              Assigned
            </Typography>
          )}
        </Box>
      ),
    },
  ]

  return (
    <Box>
      <PageHeader title="Delivery Tracking" subtitle="Monitor and assign deliveries for your branches" />

      <Paper className="overflow-hidden mt-4 rounded-xl shadow-sm" style={{ height: 500, width: '100%' }}>
        <DataGrid
          rows={deliveries}
          columns={columns}
          initialState={{
            pagination: { paginationModel: { pageSize: 10 } },
          }}
          pageSizeOptions={[5, 10, 25]}
          disableRowSelectionOnClick
          sx={{
            border: 0,
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: '#FAFAFA',
              borderBottom: '1px solid #F0F0F0',
            },
            '& .MuiDataGrid-cell:focus': {
              outline: 'none',
            },
          }}
        />
      </Paper>

      <AssignmentModal
        open={isModalOpen}
        onClose={handleCloseAssignment}
        onAssign={handleAssign}
        deliveryId={selectedDeliveryId}
      />
    </Box>
  )
}
