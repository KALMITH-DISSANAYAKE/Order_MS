import { useState, useEffect } from 'react'
import { Box, Button, Paper, Chip, IconButton, Typography, Grid, Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem, } from '@mui/material'
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid'
import { Add, Edit, Delete, Link as LinkIcon } from '@mui/icons-material'
import PageHeader from '../../components/common/PageHeader'
import VehicleFormModal, { Vehicle } from '../../components/transport/VehicleFormModal'
import DriverFormModal, { Driver } from '../../components/transport/DriverFormModal'
import axiosInstance from '../../api/axiosInstance'

export interface DriverVehicleLink {
  id: string
  driverId: string
  driverName: string
  vehicleId: string
  vehiclePlate: string
  availability: 'Available' | 'Assigned' | 'Unavailable'
}

export default function TransportList() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [links, setLinks] = useState<DriverVehicleLink[]>([])

  // Modals state
  const [isVehicleModalOpen, setVehicleModalOpen] = useState(false)
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | null>(null)

  const [isDriverModalOpen, setDriverModalOpen] = useState(false)
  const [editingDriver, setEditingDriver] = useState<Driver | null>(null)

  const [isLinkModalOpen, setLinkModalOpen] = useState(false)
  const [newLinkData, setNewLinkData] = useState({ driverId: '', vehicleId: '' })

  useEffect(() => {
    fetchVehicles()
    fetchDrivers()
    fetchLinks()
  }, [])

  const fetchVehicles = async () => {
    try {
      const res = await axiosInstance.get('/Transport/vehicles')
      const mappedVehicles = res.data.map((v: any) => ({
        id: v.vehicleId.toString(),
        licensePlate: v.vehicleNumber,
        capacity: v.capacity || 0,
        availability: v.available || 'Available'
      }))
      setVehicles(mappedVehicles)
    } catch (err) {
      console.error('Failed to fetch vehicles', err)
    }
  }

  const fetchDrivers = async () => {
    try {
      const res = await axiosInstance.get('/Transport/drivers')
      const mappedDrivers = res.data.map((d: any) => ({
        id: d.driverId.toString(),
        name: d.driversName,
        licenseNumber: d.licenseNumber,
        availability: d.available || 'Available'
      }))
      setDrivers(mappedDrivers)
    } catch (err) {
      console.error('Failed to fetch drivers', err)
    }
  }

  const fetchLinks = async () => {
    try {
      const res = await axiosInstance.get('/Transport/links')
      const mappedLinks = res.data.map((l: any) => ({
        id: l.connectionId.toString(),
        driverId: l.driverId.toString(),
        driverName: l.driverName,
        vehicleId: l.vehicleId.toString(),
        vehiclePlate: l.vehicleNumber,
        availability: 'Available'
      }))
      setLinks(mappedLinks)
    } catch (err) {
      console.error('Failed to fetch links', err)
    }
  }

  // --- Handlers ---
  const handleSaveVehicle = async (vehicle: Vehicle) => {
    try {
      const payload = {
        vehicleNumber: vehicle.licensePlate,
        capacity: vehicle.capacity,
        available: vehicle.availability
      }
      if (editingVehicle) {
        await axiosInstance.put(`/Transport/vehicles/${editingVehicle.id}`, payload)
      } else {
        await axiosInstance.post('/Transport/vehicles', payload)
      }
      fetchVehicles()
    } catch (err) {
      console.error('Error saving vehicle', err)
      alert('Failed to save vehicle')
    }
  }

  const handleDeleteVehicle = (id: string) => {
    if (window.confirm('Delete this vehicle?')) setVehicles(vehicles.filter((v) => v.id !== id))
  }

  const handleSaveDriver = async (driver: Driver) => {
    try {
      const payload = {
        driversName: driver.name,
        licenseNumber: driver.licenseNumber,
        available: driver.availability
      }
      if (editingDriver) {
        await axiosInstance.put(`/Transport/drivers/${editingDriver.id}`, payload)
      } else {
        await axiosInstance.post('/Transport/drivers', payload)
      }
      fetchDrivers()
    } catch (err) {
      console.error('Error saving driver', err)
      alert('Failed to save driver')
    }
  }

  const handleDeleteDriver = (id: string) => {
    if (window.confirm('Delete this driver?')) setDrivers(drivers.filter((d) => d.id !== id))
  }

  const handleCreateLink = async () => {
    if (!newLinkData.driverId || !newLinkData.vehicleId) return
    try {
      const payload = {
        driverId: parseInt(newLinkData.driverId),
        vehicleId: parseInt(newLinkData.vehicleId)
      }
      await axiosInstance.post('/Transport/links', payload)
      setLinkModalOpen(false)
      setNewLinkData({ driverId: '', vehicleId: '' })
      fetchLinks()
    } catch (err) {
      console.error('Error creating link', err)
      alert('Failed to create assignment')
    }
  }

  const handleDeleteLink = async (id: string) => {
    if (window.confirm('Delete this assignment?')) {
      try {
        await axiosInstance.delete(`/Transport/links/${id}`)
        fetchLinks()
      } catch (err) {
        console.error('Error deleting link', err)
        alert('Failed to delete assignment')
      }
    }
  }

  // --- Columns ---
  const renderAvailability = (params: GridRenderCellParams) => {
    let color: 'success' | 'warning' | 'error' | 'default' = 'default'
    if (params.value === 'Available') color = 'success'
    if (params.value === 'Assigned') color = 'warning'
    if (params.value === 'Unavailable' || params.value === 'Maintenance') color = 'error'
    return <Chip label={params.value} color={color} size="small" variant="outlined" />
  }

  const vehicleColumns: GridColDef[] = [
    { field: 'licensePlate', headerName: 'License Plate', flex: 1 },
    { field: 'capacity', headerName: 'Capacity', width: 100 },
    { field: 'availability', headerName: 'Availability', width: 130, renderCell: renderAvailability },
    {
      field: 'actions', headerName: 'Actions', width: 100, sortable: false,
      renderCell: (params) => (
        <Box>
          <IconButton size="small" color="primary" onClick={() => { setEditingVehicle(params.row); setVehicleModalOpen(true); }}><Edit fontSize="small" /></IconButton>
          <IconButton size="small" color="error" onClick={() => handleDeleteVehicle(params.row.id)}><Delete fontSize="small" /></IconButton>
        </Box>
      ),
    },
  ]

  const driverColumns: GridColDef[] = [
    { field: 'name', headerName: 'Driver Name', flex: 1 },
    { field: 'licenseNumber', headerName: 'License Number', width: 150 },
    { field: 'availability', headerName: 'Availability', width: 130, renderCell: renderAvailability },
    {
      field: 'actions', headerName: 'Actions', width: 100, sortable: false,
      renderCell: (params) => (
        <Box>
          <IconButton size="small" color="primary" onClick={() => { setEditingDriver(params.row); setDriverModalOpen(true); }}><Edit fontSize="small" /></IconButton>
          <IconButton size="small" color="error" onClick={() => handleDeleteDriver(params.row.id)}><Delete fontSize="small" /></IconButton>
        </Box>
      ),
    },
  ]

  const linkColumns: GridColDef[] = [
    { field: 'driverName', headerName: 'Driver Name', flex: 1 },
    { field: 'vehiclePlate', headerName: 'Vehicle Plate', flex: 1 },
    {
      field: 'actions', headerName: 'Actions', width: 70, sortable: false,
      renderCell: (params) => (
        <IconButton size="small" color="error" onClick={() => handleDeleteLink(params.row.id)}><Delete fontSize="small" /></IconButton>
      ),
    },
  ]

  return (
    <Box>
      <Box className="flex flex-col gap-4 justify-between items-start mb-6 sm:flex-row sm:items-center">
        <PageHeader title="Transport Management" subtitle="Manage vehicles, drivers, and their assignments" />
        <Box className="flex gap-2">
          <Button variant="contained" color="secondary" startIcon={<Add />} onClick={() => { setEditingDriver(null); setDriverModalOpen(true); }}>
            Add Driver
          </Button>
          <Button variant="contained" color="primary" startIcon={<Add />} onClick={() => { setEditingVehicle(null); setVehicleModalOpen(true); }}>
            Add Vehicle
          </Button>
        </Box>
      </Box>

      <Box className="flex justify-between items-end mb-6">
        <Typography variant="h6" className="!font-bold">Active Driver-Vehicle Assignments</Typography>
        <Button variant="outlined" color="primary" startIcon={<LinkIcon />} onClick={() => setLinkModalOpen(true)}>
          Assign Driver to Vehicle
        </Button>
      </Box>

      <Paper className="overflow-hidden mb-8 rounded-xl shadow-sm" style={{ height: 400, width: '100%' }}>
        <DataGrid rows={links} columns={linkColumns} pageSizeOptions={[5, 10]} initialState={{ pagination: { paginationModel: { pageSize: 5 } } }} disableRowSelectionOnClick />
      </Paper>

      <Typography variant="h6" className="!font-bold !mb-3">Vehicles</Typography>
      <Paper className="overflow-hidden mb-8 rounded-xl shadow-sm" style={{ height: 400, width: '100%' }}>
        <DataGrid rows={vehicles} columns={vehicleColumns} pageSizeOptions={[5, 10]} initialState={{ pagination: { paginationModel: { pageSize: 5 } } }} disableRowSelectionOnClick />
      </Paper>

      <Typography variant="h6" className="!font-bold !mb-3">Drivers</Typography>
      <Paper className="overflow-hidden mb-8 rounded-xl shadow-sm" style={{ height: 400, width: '100%' }}>
        <DataGrid rows={drivers} columns={driverColumns} pageSizeOptions={[5, 10]} initialState={{ pagination: { paginationModel: { pageSize: 5 } } }} disableRowSelectionOnClick />
      </Paper>

      <VehicleFormModal open={isVehicleModalOpen} onClose={() => setVehicleModalOpen(false)} onSave={handleSaveVehicle} initialData={editingVehicle} />
      <DriverFormModal open={isDriverModalOpen} onClose={() => setDriverModalOpen(false)} onSave={handleSaveDriver} initialData={editingDriver} />

      {/* Link Assignment Modal */}
      <Dialog open={isLinkModalOpen} onClose={() => setLinkModalOpen(false)} maxWidth="sm" fullWidth slotProps={{ paper: { className: '!rounded-xl' } }}>
        <DialogTitle className="!font-bold !text-xl !pb-2">Assign Driver to Vehicle</DialogTitle>
        <DialogContent>
          <Typography className="!text-sm !text-gray-500 !mb-5">Select an available driver and vehicle to link them together for deliveries.</Typography>
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <TextField select fullWidth label="Select Driver" value={newLinkData.driverId} onChange={(e) => setNewLinkData({ ...newLinkData, driverId: e.target.value })}>
                {drivers.filter(d => d.availability === 'Available').map((d) => (
                  <MenuItem key={d.id} value={d.id}>{d.name} ({d.licenseNumber})</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={12}>
              <TextField select fullWidth label="Select Vehicle" value={newLinkData.vehicleId} onChange={(e) => setNewLinkData({ ...newLinkData, vehicleId: e.target.value })}>
                {vehicles.filter(v => v.availability === 'Available').map((v) => (
                  <MenuItem key={v.id} value={v.id}>{v.licensePlate}</MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions className="!p-6 !pt-0">
          <Button onClick={() => setLinkModalOpen(false)} variant="outlined" color="inherit" className="!rounded-lg">Cancel</Button>
          <Button onClick={handleCreateLink} variant="contained" color="primary" className="!rounded-lg" disabled={!newLinkData.driverId || !newLinkData.vehicleId}>Create Assignment</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
