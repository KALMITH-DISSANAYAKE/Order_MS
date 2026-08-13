import { useState, useEffect } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Grid,
  Typography,
} from '@mui/material'

export interface Vehicle {
  id: string
  licensePlate: string
  capacity: number
  availability: 'Available' | 'Assigned' | 'Maintenance'
}

interface VehicleFormModalProps {
  open: boolean
  onClose: () => void
  onSave: (vehicle: Vehicle) => void
  initialData?: Vehicle | null
}

const AVAILABILITIES = ['Available', 'Assigned', 'Maintenance']

export default function VehicleFormModal({ open, onClose, onSave, initialData }: VehicleFormModalProps) {
  const [formData, setFormData] = useState<Partial<Vehicle>>({})
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (open) {
      if (initialData) {
        setFormData(initialData)
      } else {
        setFormData({
          licensePlate: '',
          capacity: 0,
          availability: 'Available'
        })
      }
      setErrors({})
    }
  }, [open, initialData])

  const handleChange = (field: keyof Vehicle) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }))
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: '' }))
    }
  }

  const validate = () => {
    const newErrors: Record<string, string> = {}
    if (!formData.licensePlate?.trim()) {
      newErrors.licensePlate = 'License plate is required'
    } else if (!/^[A-Z0-9-\s]{4,15}$/i.test(formData.licensePlate)) {
      newErrors.licensePlate = 'Invalid format (e.g., WP BAC-1234)'
    }
    
    if (!formData.capacity || formData.capacity <= 0) {
      newErrors.capacity = 'Capacity must be greater than 0'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSave = () => {
    if (validate()) {
      onSave({
        id: formData.id || Math.random().toString(36).substr(2, 9),
        licensePlate: formData.licensePlate || '',
        capacity: formData.capacity || 0,
        availability: formData.availability || 'Available'
      })
      onClose()
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth slotProps={{ paper: { className: '!rounded-xl' } }}>
      <DialogTitle className="!font-bold !text-xl !pb-2">
        {initialData ? 'Edit Vehicle' : 'Add New Vehicle'}
      </DialogTitle>
      <DialogContent>
        <Typography className="!text-sm !text-gray-500 !mb-5">
          {initialData ? 'Update vehicle details.' : 'Enter details of the new vehicle.'}
        </Typography>
        <Grid container spacing={3}>
          <Grid xs={12} sm={6}>
            <TextField
              fullWidth
              label="License Plate"
              value={formData.licensePlate || ''}
              onChange={handleChange('licensePlate')}
              error={!!errors.licensePlate}
              helperText={errors.licensePlate}
              placeholder="e.g. WP-ABC-1234"
            />
          </Grid>
          <Grid xs={12} sm={6}>
            <TextField
              fullWidth
              type="number"
              label="Capacity"
              value={formData.capacity || ''}
              onChange={handleChange('capacity')}
              error={!!errors.capacity}
              helperText={errors.capacity}
              placeholder="e.g. 1000"
            />
          </Grid>
          <Grid xs={12} sm={6}>
            <TextField
              fullWidth
              select
              label="Availability"
              value={formData.availability || ''}
              onChange={handleChange('availability')}
            >
              {AVAILABILITIES.map((availability) => (
                <MenuItem key={availability} value={availability}>
                  {availability}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions className="!p-6 !pt-0">
        <Button onClick={onClose} variant="outlined" color="inherit" className="!rounded-lg">
          Cancel
        </Button>
        <Button onClick={handleSave} variant="contained" color="primary" className="!rounded-lg">
          {initialData ? 'Save Changes' : 'Add Vehicle'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
