import { useState } from 'react'
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Card,
  CardContent,
  IconButton,
  Chip,
  Grid,
  Typography,
} from '@mui/material'
import {
  Add,
  Edit,
  Delete,
  LocationOn,
  People,
  Inventory,
} from '@mui/icons-material'
import PageHeader from '../../components/common/PageHeader'

interface Branch {
  id: number
  branchCode: string
  location: string
  userCount: number
  inventoryCount: number
  createdOn: string
}

const mockBranches: Branch[] = [
  { id: 1, branchCode: 'CMB-001', location: 'Colombo - Bambalapitiya', userCount: 3, inventoryCount: 45, createdOn: '2024-01-15' },
  { id: 2, branchCode: 'CMB-002', location: 'Colombo - Nugegoda', userCount: 2, inventoryCount: 38, createdOn: '2024-02-20' },
  { id: 3, branchCode: 'KDY-001', location: 'Kandy - City Center', userCount: 2, inventoryCount: 52, createdOn: '2024-03-10' },
  { id: 4, branchCode: 'GAL-001', location: 'Galle - Fort', userCount: 1, inventoryCount: 29, createdOn: '2024-04-05' },
]

export default function BranchesPage() {
  const [branches, setBranches] = useState<Branch[]>(mockBranches)
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<Branch | null>(null)
  const [form, setForm] = useState({ branchCode: '', location: '' })

  const handleOpen = (branch?: Branch) => {
    if (branch) {
      setEditing(branch)
      setForm({ branchCode: branch.branchCode, location: branch.location })
    } else {
      setEditing(null)
      setForm({ branchCode: '', location: '' })
    }
    setOpen(true)
  }

  const handleClose = () => {
    setOpen(false)
    setEditing(null)
  }

  const handleSave = () => {
    if (editing) {
      setBranches((prev) =>
        prev.map((b) =>
          b.id === editing.id ? { ...b, branchCode: form.branchCode, location: form.location } : b
        )
      )
    } else {
      const newBranch: Branch = {
        id: Math.max(...branches.map((b) => b.id), 0) + 1,
        branchCode: form.branchCode,
        location: form.location,
        userCount: 0,
        inventoryCount: 0,
        createdOn: new Date().toISOString().split('T')[0],
      }
      setBranches((prev) => [...prev, newBranch])
    }
    handleClose()
  }

  const handleDelete = (id: number) => {
    if (confirm('Are you sure you want to delete this branch?')) {
      setBranches((prev) => prev.filter((b) => b.id !== id))
    }
  }

  return (
    <div>
      <PageHeader title="Branch Management" subtitle="Manage supermarket branches across Sri Lanka" />

      <Box className="flex justify-end mb-6">
        <Button
          variant="contained"
          color="primary"
          startIcon={<Add />}
          onClick={() => handleOpen()}
          className="!normal-case !font-semibold"
        >
          Add Branch
        </Button>
      </Box>

      <Grid container spacing={3}>
        {branches.map((branch) => (
          <Grid item xs={12} sm={6} lg={4} key={branch.id}>
            <Card className="rounded-xl shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
              <CardContent className="!p-5">
                <Box className="flex justify-between items-start mb-3">
                  <Chip
                    label={branch.branchCode}
                    size="small"
                    className="!bg-cargills-light !text-cargills-red !font-bold !border !border-red-100"
                  />
                  <Box className="flex gap-1">
                    <IconButton size="small" onClick={() => handleOpen(branch)} className="!text-blue-600">
                      <Edit fontSize="small" />
                    </IconButton>
                    <IconButton size="small" onClick={() => handleDelete(branch.id)} className="!text-red-600">
                      <Delete fontSize="small" />
                    </IconButton>
                  </Box>
                </Box>

                <Box className="flex items-start gap-2 mb-4">
                  <LocationOn className="!text-cargills-red mt-0.5" fontSize="small" />
                  <Typography variant="subtitle1" className="!font-semibold !text-[#1A1A1A]">
                    {branch.location}
                  </Typography>
                </Box>

                <Box className="flex gap-4">
                  <Box className="flex items-center gap-1.5 text-gray-600">
                    <People fontSize="small" className="!text-gray-400" />
                    <span className="text-sm">{branch.userCount} Users</span>
                  </Box>
                  <Box className="flex items-center gap-1.5 text-gray-600">
                    <Inventory fontSize="small" className="!text-gray-400" />
                    <span className="text-sm">{branch.inventoryCount} Items</span>
                  </Box>
                </Box>

                <Typography variant="caption" className="!text-gray-400 !mt-3 !block">
                  Created on {branch.createdOn}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Add/Edit Dialog */}
      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle className="!font-bold !text-lg">
          {editing ? 'Edit Branch' : 'Add New Branch'}
        </DialogTitle>
        <DialogContent className="!pt-2">
          <Box className="flex flex-col gap-4 mt-2">
            <TextField
              label="Branch Code"
              placeholder="CMB-001"
              value={form.branchCode}
              onChange={(e) => setForm({ ...form, branchCode: e.target.value })}
              fullWidth
              required
            />
            <TextField
              label="Location"
              placeholder="Colombo - Bambalapitiya"
              value={form.location}
              onChange={(e) => setForm({ ...form, location: e.target.value })}
              fullWidth
              required
            />
          </Box>
        </DialogContent>
        <DialogActions className="!px-6 !pb-4">
          <Button onClick={handleClose} variant="outlined" color="inherit" className="!normal-case">
            Cancel
          </Button>
          <Button onClick={handleSave} variant="contained" color="primary" className="!normal-case !font-semibold">
            {editing ? 'Save Changes' : 'Create Branch'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  )
}
