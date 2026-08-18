import { useState, useEffect } from 'react'
import { Box, Button, Paper, Chip, IconButton, Typography, Grid, Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem, Snackbar, Alert } from '@mui/material'
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

  // Search state
  const [linkSearch, setLinkSearch] = useState({ driverName: '', vehiclePlate: '' })
  const [vehicleSearch, setVehicleSearch] = useState({ licensePlate: '', capacity: '', availability: '' })
  const [driverSearch, setDriverSearch] = useState({ name: '', licenseNumber: '', availability: '' })

  // Modals state
  const [isVehicleModalOpen, setVehicleModalOpen] = useState(false)
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | null>(null)

  const [isDriverModalOpen, setDriverModalOpen] = useState(false)
  const [editingDriver, setEditingDriver] = useState<Driver | null>(null)

  const [isLinkModalOpen, setLinkModalOpen] = useState(false)
  const [newLinkData, setNewLinkData] = useState({ driverId: '', vehicleId: '' })

  const [snackbar, setSnackbar] = useState<{open: boolean, message: string, severity: 'success' | 'error'}>({
    open: false,
    message: '',
    severity: 'success'
  })

  const handleCloseSnackbar = () => setSnackbar({ ...snackbar, open: false })

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
        availability: l.status || 'Available'
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
        setSnackbar({ open: true, message: 'Vehicle updated successfully!', severity: 'success' })
      } else {
        await axiosInstance.post('/Transport/vehicles', payload)
        setSnackbar({ open: true, message: 'Vehicle added successfully!', severity: 'success' })
      }
      fetchVehicles()
      setVehicleModalOpen(false)
    } catch (err: any) {
      console.error('Error saving vehicle', err)
      setSnackbar({ open: true, message: err.response?.data?.message || 'Failed to save vehicle', severity: 'error' })
    }
  }

  const handleDeleteVehicle = async (id: string) => {
    if (window.confirm('Delete this vehicle?')) {
      try {
        await axiosInstance.delete(`/Transport/vehicles/${id}`)
        setSnackbar({ open: true, message: 'Vehicle deleted successfully!', severity: 'success' })
        fetchVehicles()
      } catch (err: any) {
        console.error('Error deleting vehicle', err)
        setSnackbar({ open: true, message: err.response?.data?.message || 'Failed to delete vehicle', severity: 'error' })
      }
    }
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
        setSnackbar({ open: true, message: 'Driver updated successfully!', severity: 'success' })
      } else {
        await axiosInstance.post('/Transport/drivers', payload)
        setSnackbar({ open: true, message: 'Driver added successfully!', severity: 'success' })
      }
      fetchDrivers()
      setDriverModalOpen(false)
    } catch (err: any) {
      console.error('Error saving driver', err)
      setSnackbar({ open: true, message: err.response?.data?.message || 'Failed to save driver', severity: 'error' })
    }
  }

  const handleDeleteDriver = async (id: string) => {
    if (window.confirm('Delete this driver?')) {
      try {
        await axiosInstance.delete(`/Transport/drivers/${id}`)
        setSnackbar({ open: true, message: 'Driver deleted successfully!', severity: 'success' })
        fetchDrivers()
      } catch (err: any) {
        console.error('Error deleting driver', err)
        setSnackbar({ open: true, message: err.response?.data?.message || 'Failed to delete driver', severity: 'error' })
      }
    }
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
      fetchVehicles()
      fetchDrivers()
      setSnackbar({ open: true, message: 'Assignment created successfully!', severity: 'success' })
    } catch (err: any) {
      console.error('Error creating link', err)
      setSnackbar({ open: true, message: err.response?.data?.message || 'Failed to create assignment', severity: 'error' })
    }
  }

  const handleDeleteLink = async (id: string) => {
    if (window.confirm('Delete this assignment?')) {
      try {
        await axiosInstance.delete(`/Transport/links/${id}`)
        fetchLinks()
        fetchVehicles()
        fetchDrivers()
        setSnackbar({ open: true, message: 'Assignment deleted successfully!', severity: 'success' })
      } catch (err: any) {
        console.error('Error deleting link', err)
        setSnackbar({ open: true, message: err.response?.data?.message || 'Failed to delete assignment', severity: 'error' })
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
    { field: 'availability', headerName: 'Status', width: 130, renderCell: renderAvailability },
    {
      field: 'actions', headerName: 'Actions', width: 70, sortable: false,
      renderCell: (params) => (
        <IconButton size="small" color="error" onClick={() => handleDeleteLink(params.row.id)}><Delete fontSize="small" /></IconButton>
      ),
    },
  ]

  const filteredLinks = links.filter(l => 
    l.driverName.toLowerCase().includes(linkSearch.driverName.toLowerCase()) &&
    l.vehiclePlate.toLowerCase().includes(linkSearch.vehiclePlate.toLowerCase())
  )

  const filteredVehicles = vehicles.filter(v => 
    v.licensePlate.toLowerCase().includes(vehicleSearch.licensePlate.toLowerCase()) &&
    (vehicleSearch.capacity === '' || v.capacity >= Number(vehicleSearch.capacity)) &&
    (vehicleSearch.availability === '' || v.availability === vehicleSearch.availability)
  )

  const filteredDrivers = drivers.filter(d => 
    d.name.toLowerCase().includes(driverSearch.name.toLowerCase()) &&
    d.licenseNumber.toLowerCase().includes(driverSearch.licenseNumber.toLowerCase()) &&
    (driverSearch.availability === '' || d.availability === driverSearch.availability)
  )

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

      <Box className="flex justify-between items-end mb-3">
        <Typography variant="h6" className="!font-bold">Active Driver-Vehicle Assignments</Typography>
        <Button variant="outlined" color="primary" startIcon={<LinkIcon />} onClick={() => setLinkModalOpen(true)}>
          Assign Driver to Vehicle
        </Button>
      </Box>

      <Box className="flex gap-4 mb-4">
        <input type="text" placeholder="Driver Name" className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={linkSearch.driverName} onChange={(e) => setLinkSearch({ ...linkSearch, driverName: e.target.value })} />
        <input type="text" placeholder="Vehicle Plate" className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={linkSearch.vehiclePlate} onChange={(e) => setLinkSearch({ ...linkSearch, vehiclePlate: e.target.value })} />
      </Box>

      <Paper className="overflow-hidden mb-8 rounded-xl shadow-sm" style={{ height: 400, width: '100%' }}>
        <DataGrid rows={filteredLinks} columns={linkColumns} pageSizeOptions={[5, 10]} initialState={{ pagination: { paginationModel: { pageSize: 5 } } }} disableRowSelectionOnClick />
      </Paper>

      <Typography variant="h6" className="!font-bold !mb-3">Vehicles</Typography>
      
      <Box className="flex gap-4 mb-4">
        <input type="text" placeholder="License Plate" className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={vehicleSearch.licensePlate} onChange={(e) => setVehicleSearch({ ...vehicleSearch, licensePlate: e.target.value })} />
        <input type="number" placeholder="Min Capacity" className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={vehicleSearch.capacity} onChange={(e) => setVehicleSearch({ ...vehicleSearch, capacity: e.target.value })} />
        <select className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={vehicleSearch.availability} onChange={(e) => setVehicleSearch({ ...vehicleSearch, availability: e.target.value })}>
          <option value="">All Statuses</option>
          <option value="Available">Available</option>
          <option value="Assigned">Assigned</option>
          <option value="Unavailable">Unavailable</option>
          <option value="Maintenance">Maintenance</option>
        </select>
      </Box>

      <Paper className="overflow-hidden mb-8 rounded-xl shadow-sm" style={{ height: 400, width: '100%' }}>
        <DataGrid rows={filteredVehicles} columns={vehicleColumns} pageSizeOptions={[5, 10]} initialState={{ pagination: { paginationModel: { pageSize: 5 } } }} disableRowSelectionOnClick />
      </Paper>

      <Typography variant="h6" className="!font-bold !mb-3">Drivers</Typography>

      <Box className="flex gap-4 mb-4">
        <input type="text" placeholder="Driver Name" className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={driverSearch.name} onChange={(e) => setDriverSearch({ ...driverSearch, name: e.target.value })} />
        <input type="text" placeholder="License Number" className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={driverSearch.licenseNumber} onChange={(e) => setDriverSearch({ ...driverSearch, licenseNumber: e.target.value })} />
        <select className="border p-2 rounded text-sm flex-1 min-w-[120px]" value={driverSearch.availability} onChange={(e) => setDriverSearch({ ...driverSearch, availability: e.target.value })}>
          <option value="">All Statuses</option>
          <option value="Available">Available</option>
          <option value="Assigned">Assigned</option>
          <option value="Unavailable">Unavailable</option>
        </select>
      </Box>

      <Paper className="overflow-hidden mb-8 rounded-xl shadow-sm" style={{ height: 400, width: '100%' }}>
        <DataGrid rows={filteredDrivers} columns={driverColumns} pageSizeOptions={[5, 10]} initialState={{ pagination: { paginationModel: { pageSize: 5 } } }} disableRowSelectionOnClick />
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

      <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={handleCloseSnackbar} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} variant="filled" sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  )
}
