import { Chip } from '@mui/material'

interface Props {
  status: string
}

export default function OrderRequestStatusChip({
  status,
}: Props) {
  const normalizedStatus = status?.toLowerCase()

  let color:
    | 'default'
    | 'primary'
    | 'secondary'
    | 'success'
    | 'error'
    | 'warning'
    | 'info' = 'default'

  switch (normalizedStatus) {
    case 'pending':
      color = 'warning'
      break

    case 'approved':
      color = 'success'
      break

    case 'rejected':
      color = 'error'
      break

    case 'paid':
      color = 'info'
      break

    case 'completed':
      color = 'success'
      break

    default:
      color = 'default'
  }

  return (
    <Chip
      label={status || 'Unknown'}
      color={color}
      size="small"
    />
  )
}