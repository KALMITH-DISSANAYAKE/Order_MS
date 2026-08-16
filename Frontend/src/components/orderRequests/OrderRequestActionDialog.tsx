import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from '@mui/material'

export type OrderRequestAction =
  | 'approve'
  | 'reject'
  | 'payment'

interface Props {
  open: boolean
  action: OrderRequestAction | null
  requestId: number | null
  loading: boolean
  onClose: () => void
  onConfirm: () => void
}

const labels: Record<
  OrderRequestAction,
  string
> = {
  approve: 'Approve',
  reject: 'Reject',
  payment: 'Mark as Paid',
}

export default function OrderRequestActionDialog({
  open,
  action,
  requestId,
  loading,
  onClose,
  onConfirm,
}: Props) {
  if (!action) return null

  const isReject = action === 'reject'

  return (
    <Dialog
      open={open}
      onClose={loading ? undefined : onClose}
      maxWidth="xs"
      fullWidth
    >
      <DialogTitle>
        {labels[action]} Order Request
      </DialogTitle>

      <DialogContent>
        {isReject && (
          <Alert
            severity="warning"
            sx={{ mb: 2 }}
          >
            This action may not be reversible.
          </Alert>
        )}

        <Typography>
          Are you sure you want to{' '}
          <strong>
            {labels[action].toLowerCase()}
          </strong>{' '}
          order request #{requestId}?
        </Typography>
      </DialogContent>

      <DialogActions>
        <Button
          onClick={onClose}
          disabled={loading}
        >
          Cancel
        </Button>

        <Button
          variant="contained"
          color={
            isReject ? 'error' : 'primary'
          }
          onClick={onConfirm}
          disabled={loading}
        >
          {loading
            ? 'Processing...'
            : labels[action]}
        </Button>
      </DialogActions>
    </Dialog>
  )
}