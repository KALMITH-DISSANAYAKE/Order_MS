import { useState, useEffect } from 'react'
import {
  Dialog, DialogTitle, DialogContent, DialogActions, Button, TextField, MenuItem, Grid, Typography,
} from '@mui/material'

export interface Driver {
  id: string
  name: string
  licenseNumber: string
  availability: 'Available' | 'Assigned' | 'Unavailable'
}

interface DriverFormModalProps {
  open: boolean
  onClose: () => void
  onSave: (driver: Driver) => void
  initialData?: Driver | null
}

const AVAILABILITIES = ['Available', 'Assigned', 'Unavailable']

export default function DriverFormModal({ open, onClose, onSave, initialData }: DriverFormModalProps) {
  const [formData, setFormData] = useState<Partial<Driver>>({})
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (open) {
      if (initialData) {
        setFormData(initialData)
      } else {
        setFormData({
          name: '',
          licenseNumber: '',
          availability: 'Available',
        })
      }
      setErrors({})
    }
  }, [open, initialData])

  const handleChange = (field: keyof Driver) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }))
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: '' }))
    }
  }

  const validate = () => {
    const newErrors: Record<string, string> = {}
    if (!formData.name?.trim()) newErrors.name = 'Name is required'
    if (!formData.licenseNumber?.trim()) {
      newErrors.licenseNumber = 'License Number is required'
    } else if (!/^L-\d{9}$/.test(formData.licenseNumber)) {
      newErrors.licenseNumber = 'Invalid format (e.g., L-123456789)'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSave = () => {
    if (validate()) {
      onSave({
        id: formData.id || Math.random().toString(36).substr(2, 9),
        name: formData.name || '',
        licenseNumber: formData.licenseNumber || '',
        availability: formData.availability || 'Available',
      })
      onClose()
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth slotProps={{ paper: { className: '!rounded-xl' } }}>
      <DialogTitle className="!font-bold !text-xl !pb-2">
        {initialData ? 'Edit Driver' : 'Add New Driver'}
      </DialogTitle>
      <DialogContent>
        <Typography className="!text-sm !text-gray-500 !mb-5">
          {initialData ? 'Update driver details.' : 'Enter details for the new driver.'}
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="Name"
              value={formData.name || ''}
              onChange={handleChange('name')}
              error={!!errors.name}
              helperText={errors.name}
              placeholder="Driver Name"
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="License Number"
              value={formData.licenseNumber || ''}
              onChange={handleChange('licenseNumber')}
              error={!!errors.licenseNumber}
              helperText={errors.licenseNumber}
              placeholder="e.g. L-123456789"
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              select
              label="Availability"
              value={formData.availability || ''}
              onChange={handleChange('availability')}
            >
              {AVAILABILITIES.filter(a => initialData || a !== 'Assigned').map((availability) => (
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
          {initialData ? 'Save Changes' : 'Add Driver'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
