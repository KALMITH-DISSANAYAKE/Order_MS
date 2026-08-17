import { useEffect, useState } from 'react'
import {
  Alert,
  Box,
  CircularProgress,
  Dialog,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'

import {
  OrderRequest,
  orderRequestApi,
} from '../../api/orderRequestApi'
import OrderRequestStatusChip from './OrderRequestStatusChip'

interface Props {
  open: boolean
  requestId: number | null
  onClose: () => void
}

export default function OrderRequestDetailsDialog({
  open,
  requestId,
  onClose,
}: Props) {
  const [request, setRequest] =
    useState<OrderRequest | null>(null)

  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!open || requestId === null) {
      setRequest(null)
      return
    }

    const loadRequest = async () => {
      try {
        setLoading(true)
        setError('')

        const data =
          await orderRequestApi.getById(
            requestId
          )

        setRequest(data)
      } catch (error: any) {
        console.error(error)

        setError(
          error.response?.data?.message ||
          'Unable to load request details.'
        )
      } finally {
        setLoading(false)
      }
    }

    loadRequest()
  }, [open, requestId])

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="md"
    >
      <DialogTitle
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        Order Request Details

        <IconButton onClick={onClose}>
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers>
        {loading && (
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              py: 6,
            }}
          >
            <CircularProgress />
          </Box>
        )}

        {!loading && error && (
          <Alert severity="error">
            {error}
          </Alert>
        )}

        {!loading && !error && request && (
          <>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: {
                  xs: '1fr',
                  sm: 'repeat(2, 1fr)',
                },
                gap: 3,
                mb: 3,
              }}
            >
              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Request ID
                </Typography>

                <Typography fontWeight={600}>
                  #{request.orderReqId}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Status
                </Typography>

                <Box sx={{ mt: 0.5 }}>
                  <OrderRequestStatusChip
                    status={request.status}
                  />
                </Box>
              </Box>

              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Requested By
                </Typography>

                <Typography>
                  {request.firstName} {request.lastName}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Branch Code
                </Typography>

                <Typography fontWeight={600}>
                  {request.branchCode}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Requested On
                </Typography>

                <Typography>
                  {new Date(
                    request.requestedOn
                  ).toLocaleString()}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Total Quantity
                </Typography>

                <Typography fontWeight={600}>
                  {request.totalQuantity}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="caption"
                  color="text.secondary"
                >
                  Total Price
                </Typography>

                <Typography fontWeight={600}>
                  Rs.{' '}
                  {Number(
                    request.totalPrice || 0
                  ).toLocaleString(
                    undefined,
                    {
                      minimumFractionDigits: 2,
                    }
                  )}
                </Typography>
              </Box>
            </Box>

            <Divider sx={{ mb: 3 }} />

            <Typography
              variant="h6"
              sx={{ mb: 2 }}
            >
              Requested Items
            </Typography>

            {!request.items ||
              request.items.length === 0 ? (
              <Alert severity="info">
                No items found.
              </Alert>
            ) : (
              <Paper variant="outlined">
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>
                        Item
                      </TableCell>

                      <TableCell align="right">
                        Quantity
                      </TableCell>

                      <TableCell align="right">
                        Unit Price
                      </TableCell>

                      <TableCell align="right">
                        Line Total
                      </TableCell>
                    </TableRow>
                  </TableHead>

                  <TableBody>
                    {request.items.map(
                      (item) => (
                        <TableRow
                          key={item.itemId}
                        >
                          <TableCell>
                            {item.itemName}
                          </TableCell>

                          <TableCell align="right">
                            {item.quantity}
                          </TableCell>

                          <TableCell align="right">
                            Rs.{' '}
                            {Number(
                              item.unitPrice
                            ).toLocaleString(
                              undefined,
                              {
                                minimumFractionDigits: 2,
                              }
                            )}
                          </TableCell>

                          <TableCell align="right">
                            Rs.{' '}
                            {Number(
                              item.lineTotal
                            ).toLocaleString(
                              undefined,
                              {
                                minimumFractionDigits: 2,
                              }
                            )}
                          </TableCell>
                        </TableRow>
                      )
                    )}
                  </TableBody>
                </Table>
              </Paper>
            )}
          </>
        )}
      </DialogContent>
    </Dialog>
  )
}