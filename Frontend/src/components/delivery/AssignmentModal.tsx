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

import axiosInstance from '../../api/axiosInstance'

export interface AssignmentData {
  driver: string
  vehicle: string
  connectionId?: number
}

interface AssignmentModalProps {
  open: boolean
  onClose: () => void
  onAssign: (data: AssignmentData) => void
  deliveryId: string | null
}

export default function AssignmentModal({ open, onClose, onAssign, deliveryId }: AssignmentModalProps) {
  const [selectedLinkId, setSelectedLinkId] = useState<string>('')
  const [error, setError] = useState<string>('')
  const [availableLinks, setAvailableLinks] = useState<any[]>([])

  useEffect(() => {
    if (open) {
      setSelectedLinkId('')
      setError('')
      fetchLinks()
    }
  }, [open])

  const fetchLinks = async () => {
    try {
      const response = await axiosInstance.get('/Transport/available-links')
      setAvailableLinks(response.data || [])
    } catch (err) {
      console.error('Failed to fetch available links:', err)
    }
  }

  const validate = () => {
    if (!selectedLinkId) {
      setError('Assignment selection is required')
      return false
    }
    setError('')
    return true
  }

  const handleAssign = () => {
    if (validate()) {
      const link = availableLinks.find(l => l.connectionId.toString() === selectedLinkId)
      if (link) {
        onAssign({ 
          driver: link.driverName, 
          vehicle: link.vehicleNumber, 
          connectionId: link.connectionId 
        })
        onClose()
      }
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth slotProps={{ paper: { className: '!rounded-xl' } }}>
      <DialogTitle className="!font-bold !text-xl !pb-2">
        Assign Delivery (ID: {deliveryId})
      </DialogTitle>
      <DialogContent>
        <Typography className="!text-sm !text-gray-500 !mb-5">
          Select an available Driver-Vehicle assignment to dispatch this delivery.
          <br/>
          <i>Note: Only available links created in the Transport module are shown here.</i>
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              select
              label="Select Available Assignment"
              value={selectedLinkId}
              onChange={(e) => {
                setSelectedLinkId(e.target.value)
                if (error) setError('')
              }}
              error={!!error}
              helperText={error}
            >
              {availableLinks.length === 0 ? (
                <MenuItem disabled value="">No available assignments</MenuItem>
              ) : (
                availableLinks.map((link) => (
                  <MenuItem key={link.connectionId} value={link.connectionId.toString()}>
                    {link.driverName} • {link.vehicleNumber}
                  </MenuItem>
                ))
              )}
            </TextField>
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions className="!p-6 !pt-0">
        <Button onClick={onClose} variant="outlined" color="inherit" className="!rounded-lg">
          Cancel
        </Button>
        <Button onClick={handleAssign} variant="contained" color="primary" className="!rounded-lg">
          Confirm Assignment
        </Button>
      </DialogActions>
    </Dialog>
  )
}
