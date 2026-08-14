import { useEffect, useMemo, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'

import AddIcon from '@mui/icons-material/Add'
import VisibilityIcon from '@mui/icons-material/Visibility'
import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'
import PaymentIcon from '@mui/icons-material/Payment'
import RefreshIcon from '@mui/icons-material/Refresh'

import PageHeader from '../../components/common/PageHeader'
import { useAuth } from '../../contexts/AuthContext'

import {
  OrderRequest,
  orderRequestApi,
} from '../../api/orderRequestApi'

import OrderRequestStatusChip from '../../components/orderRequests/OrderRequestStatusChip'
import OrderRequestFormDialog from '../../components/orderRequests/OrderRequestFormDialog'
import OrderRequestDetailsDialog from '../../components/orderRequests/OrderRequestDetailsDialog'
import OrderRequestActionDialog, {
  OrderRequestAction,
} from '../../components/orderRequests/OrderRequestActionDialog'

export default function OrderRequestsPage() {
  const { user } = useAuth()

  const [requests, setRequests] = useState<OrderRequest[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')

  const [createOpen, setCreateOpen] = useState(false)

  const [detailsOpen, setDetailsOpen] = useState(false)
  const [selectedRequestId, setSelectedRequestId] =
    useState<number | null>(null)

  const [action, setAction] =
    useState<OrderRequestAction | null>(null)

  const [actionRequestId, setActionRequestId] =
    useState<number | null>(null)

  const [actionLoading, setActionLoading] = useState(false)

  const [notification, setNotification] = useState<{
    open: boolean
    message: string
    severity: 'success' | 'error'
  }>({
    open: false,
    message: '',
    severity: 'success',
  })

  const isBranchManager =
    user?.role === 'BranchManager'

  const isInventoryManager =
    user?.role === 'InventoryManager'

  // --------------------------------------------------
  // LOAD REQUESTS
  // --------------------------------------------------

  const loadRequests = async () => {
    try {
      setLoading(true)
      setError('')

      const data = await orderRequestApi.getAll()

      setRequests(data)
    } catch (error: any) {
      console.error(
        'Load order requests error:',
        error
      )

      setError(
        error.response?.data?.message ||
          error.response?.data?.title ||
          error.message ||
          'Unable to load order requests.'
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadRequests()
  }, [])

  // --------------------------------------------------
  // STATUS LIST
  // --------------------------------------------------

  const statuses = useMemo(
    () =>
      Array.from(
        new Set(
          requests
            .map((request) => request.status)
            .filter(Boolean)
        )
      ),
    [requests]
  )

  // --------------------------------------------------
  // SEARCH + FILTER
  // --------------------------------------------------

  const filteredRequests = useMemo(() => {
    const searchTerm =
      search.trim().toLowerCase()

    return requests.filter((request) => {
      const matchesSearch =
        !searchTerm ||
        String(request.orderReqId).includes(
          searchTerm
        ) ||
        String(request.requestedBy)
          .toLowerCase()
          .includes(searchTerm)

      const matchesStatus =
        statusFilter === 'all' ||
        request.status.toLowerCase() ===
          statusFilter.toLowerCase()

      return matchesSearch && matchesStatus
    })
  }, [
    requests,
    search,
    statusFilter,
  ])

  // --------------------------------------------------
  // NOTIFICATION
  // --------------------------------------------------

  const showNotification = (
    message: string,
    severity: 'success' | 'error'
  ) => {
    setNotification({
      open: true,
      message,
      severity,
    })
  }

  // --------------------------------------------------
  // CREATE SUCCESS
  // --------------------------------------------------

  const handleCreateSuccess = async () => {
    await loadRequests()

    showNotification(
      'Order request created successfully.',
      'success'
    )
  }

  // --------------------------------------------------
  // VIEW REQUEST
  // --------------------------------------------------

  const handleView = (id: number) => {
    setSelectedRequestId(id)
    setDetailsOpen(true)
  }

  // --------------------------------------------------
  // OPEN ACTION DIALOG
  // --------------------------------------------------

  const handleAction = (
    id: number,
    selectedAction: OrderRequestAction
  ) => {
    setActionRequestId(id)
    setAction(selectedAction)
  }

  // --------------------------------------------------
  // CLOSE ACTION DIALOG
  // --------------------------------------------------

  const closeActionDialog = () => {
    if (actionLoading) {
      return
    }

    setAction(null)
    setActionRequestId(null)
  }

  // --------------------------------------------------
  // PERFORM APPROVE / REJECT / PAYMENT
  // --------------------------------------------------

  const performAction = async () => {
    if (
      !action ||
      actionRequestId === null
    ) {
      return
    }

    try {
      setActionLoading(true)

      // ----------------------------------------------
      // APPROVE
      // ----------------------------------------------

      if (action === 'approve') {
        if (!user?.id) {
          throw new Error(
            'Unable to identify the current user.'
          )
        }

        await orderRequestApi.approve(
          actionRequestId,
          {
            approvedBy: user.id,
          }
        )

        showNotification(
          'Order request approved successfully.',
          'success'
        )
      }

      // ----------------------------------------------
      // REJECT
      // ----------------------------------------------

      else if (action === 'reject') {
        await orderRequestApi.reject(
          actionRequestId
        )

        showNotification(
          'Order request rejected successfully.',
          'success'
        )
      }

      // ----------------------------------------------
      // PAYMENT
      // ----------------------------------------------

      else if (action === 'payment') {
        await orderRequestApi.payment(
          actionRequestId
        )

        showNotification(
          'Payment completed successfully.',
          'success'
        )
      }

      // Close dialog
      closeActionDialog()

      // Reload table
      await loadRequests()
    } catch (error: any) {
      console.error(
        'Order request action error:',
        error
      )

      const message =
        error.response?.data?.message ||
        error.response?.data?.title ||
        error.message ||
        'Unable to complete the action.'

      showNotification(
        message,
        'error'
      )
    } finally {
      setActionLoading(false)
    }
  }

  // --------------------------------------------------
  // STATUS HELPERS
  // --------------------------------------------------

  const isSubmittedForReview = (
    status: string
  ) =>
    status.toLowerCase() ===
    'submittedforreview'

  const isTransportAssigned = (
    status: string
  ) =>
    status.toLowerCase() ===
    'transportassigned'

  return (
    <Box>
      <PageHeader
        title="Order Requests"
        subtitle="Manage branch inventory replenishment requests"
      />

      <Card>
        <CardContent>

          {/* ---------------------------------------- */}
          {/* SEARCH / FILTER / BUTTONS */}
          {/* ---------------------------------------- */}

          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: {
                xs: 'stretch',
                md: 'center',
              },
              flexDirection: {
                xs: 'column',
                md: 'row',
              },
              gap: 2,
              mb: 3,
            }}
          >
            <Box
              sx={{
                display: 'flex',
                gap: 2,
                flex: 1,
                flexDirection: {
                  xs: 'column',
                  md: 'row',
                },
              }}
            >
              <TextField
                label="Search"
                placeholder="ID or requester"
                value={search}
                onChange={(event) =>
                  setSearch(event.target.value)
                }
                fullWidth
              />

              <FormControl
                sx={{
                  minWidth: 180,
                }}
              >
                <InputLabel>
                  Status
                </InputLabel>

                <Select
                  value={statusFilter}
                  label="Status"
                  onChange={(event) =>
                    setStatusFilter(
                      event.target.value
                    )
                  }
                >
                  <MenuItem value="all">
                    All statuses
                  </MenuItem>

                  {statuses.map((status) => (
                    <MenuItem
                      key={status}
                      value={status}
                    >
                      {status}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            <Box
              sx={{
                display: 'flex',
                gap: 1,
              }}
            >
              <Button
                variant="outlined"
                startIcon={<RefreshIcon />}
                onClick={loadRequests}
                disabled={loading}
              >
                Refresh
              </Button>

              {isBranchManager && (
                <Button
                  variant="contained"
                  startIcon={<AddIcon />}
                  onClick={() =>
                    setCreateOpen(true)
                  }
                >
                  New Request
                </Button>
              )}
            </Box>
          </Box>

          {/* ---------------------------------------- */}
          {/* ERROR */}
          {/* ---------------------------------------- */}

          {error && (
            <Alert
              severity="error"
              sx={{ mb: 3 }}
            >
              {error}
            </Alert>
          )}

          {/* ---------------------------------------- */}
          {/* LOADING */}
          {/* ---------------------------------------- */}

          {loading ? (
            <Box
              sx={{
                display: 'flex',
                justifyContent: 'center',
                py: 8,
              }}
            >
              <CircularProgress />
            </Box>
          ) : filteredRequests.length === 0 ? (

            /* -------------------------------------- */
            /* EMPTY */
            /* -------------------------------------- */

            <Box
              sx={{
                textAlign: 'center',
                py: 8,
              }}
            >
              <Typography
                variant="h6"
                color="text.secondary"
              >
                No order requests found
              </Typography>

              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ mt: 1 }}
              >
                {search ||
                statusFilter !== 'all'
                  ? 'Try changing your search or filter.'
                  : 'There are no order requests yet.'}
              </Typography>
            </Box>

          ) : (

            /* -------------------------------------- */
            /* TABLE */
            /* -------------------------------------- */

            <TableContainer>
              <Table>

                <TableHead>
                  <TableRow>
                    <TableCell>
                      ID
                    </TableCell>

                    <TableCell>
                      Requester
                    </TableCell>

                    <TableCell>
                      Status
                    </TableCell>

                    <TableCell align="right">
                      Quantity
                    </TableCell>

                    <TableCell align="right">
                      Total Price
                    </TableCell>

                    <TableCell>
                      Requested On
                    </TableCell>

                    <TableCell align="right">
                      Actions
                    </TableCell>
                  </TableRow>
                </TableHead>

                <TableBody>
                  {filteredRequests.map(
                    (request) => {

                      /*
                       * Inventory Manager can approve/reject
                       * only when status is SubmittedForReview.
                       */

                      const canReview =
                        isInventoryManager &&
                        isSubmittedForReview(
                          request.status
                        )

                      /*
                       * Inventory Manager can make payment
                       * only after transport has been assigned.
                       */

                      const canMakePayment =
                        isInventoryManager &&
                        isTransportAssigned(
                          request.status
                        )

                      return (
                        <TableRow
                          key={
                            request.orderReqId
                          }
                          hover
                        >

                          {/* ID */}
                          <TableCell>
                            <Typography
                              fontWeight={600}
                            >
                              #
                              {
                                request.orderReqId
                              }
                            </Typography>
                          </TableCell>

                          {/* REQUESTER */}
                          <TableCell>
                            {
                              request.requestedBy
                            }
                          </TableCell>

                          {/* STATUS */}
                          <TableCell>
                            <OrderRequestStatusChip
                              status={
                                request.status
                              }
                            />
                          </TableCell>

                          {/* QUANTITY */}
                          <TableCell align="right">
                            {
                              request.totalQuantity
                            }
                          </TableCell>

                          {/* PRICE */}
                          <TableCell align="right">
                            Rs.{' '}
                            {Number(
                              request.totalPrice
                            ).toLocaleString(
                              undefined,
                              {
                                minimumFractionDigits: 2,
                              }
                            )}
                          </TableCell>

                          {/* DATE */}
                          <TableCell>
                            {new Date(
                              request.requestedOn
                            ).toLocaleString()}
                          </TableCell>

                          {/* ACTIONS */}
                          <TableCell align="right">
                            <Box
                              sx={{
                                display: 'flex',
                                justifyContent:
                                  'flex-end',
                                gap: 0.5,
                                flexWrap:
                                  'wrap',
                              }}
                            >

                              {/* VIEW */}
                              <Button
                                size="small"
                                startIcon={
                                  <VisibilityIcon />
                                }
                                onClick={() =>
                                  handleView(
                                    request.orderReqId
                                  )
                                }
                              >
                                View
                              </Button>

                              {/* -------------------------------- */}
                              {/* APPROVE + REJECT                  */}
                              {/* -------------------------------- */}

                              {canReview && (
                                <>
                                  <Button
                                    size="small"
                                    color="success"
                                    startIcon={
                                      <CheckIcon />
                                    }
                                    onClick={() =>
                                      handleAction(
                                        request.orderReqId,
                                        'approve'
                                      )
                                    }
                                  >
                                    Approve
                                  </Button>

                                  <Button
                                    size="small"
                                    color="error"
                                    startIcon={
                                      <CloseIcon />
                                    }
                                    onClick={() =>
                                      handleAction(
                                        request.orderReqId,
                                        'reject'
                                      )
                                    }
                                  >
                                    Reject
                                  </Button>
                                </>
                              )}

                              {/* -------------------------------- */}
                              {/* PAYMENT                           */}
                              {/* -------------------------------- */}

                              {canMakePayment && (
                                <Button
                                  size="small"
                                  color="primary"
                                  variant="contained"
                                  startIcon={
                                    <PaymentIcon />
                                  }
                                  onClick={() =>
                                    handleAction(
                                      request.orderReqId,
                                      'payment'
                                    )
                                  }
                                >
                                  Make Payment
                                </Button>
                              )}

                            </Box>
                          </TableCell>
                        </TableRow>
                      )
                    }
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* ============================================== */}
      {/* CREATE REQUEST DIALOG                         */}
      {/* ============================================== */}

      <OrderRequestFormDialog
        open={createOpen}
        onClose={() =>
          setCreateOpen(false)
        }
        onSuccess={
          handleCreateSuccess
        }
      />

      {/* ============================================== */}
      {/* DETAILS DIALOG                                */}
      {/* ============================================== */}

      <OrderRequestDetailsDialog
        open={detailsOpen}
        requestId={selectedRequestId}
        onClose={() => {
          setDetailsOpen(false)
          setSelectedRequestId(null)
        }}
      />

      {/* ============================================== */}
      {/* ACTION CONFIRMATION DIALOG                    */}
      {/* ============================================== */}

      <OrderRequestActionDialog
        open={
          action !== null &&
          actionRequestId !== null
        }
        action={action}
        requestId={actionRequestId}
        loading={actionLoading}
        onClose={closeActionDialog}
        onConfirm={performAction}
      />

      {/* ============================================== */}
      {/* NOTIFICATION                                  */}
      {/* ============================================== */}

      <Snackbar
        open={notification.open}
        autoHideDuration={4000}
        onClose={() =>
          setNotification(
            (current) => ({
              ...current,
              open: false,
            })
          )
        }
      >
        <Alert
          severity={
            notification.severity
          }
          onClose={() =>
            setNotification(
              (current) => ({
                ...current,
                open: false,
              })
            )
          }
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </Box>
  )
}