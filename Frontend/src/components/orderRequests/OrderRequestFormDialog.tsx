import axios from 'axios'

import {
  Alert,
  Autocomplete,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  TextField,
  Typography,
  CircularProgress,
} from '@mui/material'

import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'

import { useAuth } from '../../contexts/AuthContext'
import { Item, orderRequestApi } from '../../api/orderRequestApi'
import { useEffect, useState } from 'react'

interface FormItem {
  itemId: number | ''
  quantity: number | ''
}

interface Props {
  open: boolean
  onClose: () => void
  onSuccess: () => void
}

export default function OrderRequestFormDialog({
  open,
  onClose,
  onSuccess,
}: Props) {
  const { user } = useAuth()

  const [items, setItems] = useState<FormItem[]>([
    {
      itemId: '',
      quantity: '',
    },
  ])

  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [availableItems, setAvailableItems] = useState<Item[]>([])
  const [itemsLoading, setItemsLoading] = useState(false)

  useEffect(() => {
    if (!open) return

    const loadItems = async () => {
      try {
        setItemsLoading(true)
        setError('')

        const data = await orderRequestApi.getItems()
        console.log('Available items:', data)
        setAvailableItems(data)
      } catch (error: any) {
        console.error('Failed to load items:', error)

        setError(
          error.response?.data?.message ||
          'Unable to load available products.'
        )
      } finally {
        setItemsLoading(false)
      }
    }

    loadItems()
  }, [open])

  const updateItem = (
    index: number,
    field: keyof FormItem,
    value: number | ''
  ) => {
    setItems((current) =>
      current.map((item, itemIndex) =>
        itemIndex === index
          ? {
            ...item,
            [field]: value,
          }
          : item
      )
    )

    // Remove previous error when user changes a value
    setError('')
  }

  const addItem = () => {
    setItems((current) => [
      ...current,
      {
        itemId: '',
        quantity: '',
      },
    ])

    setError('')
  }

  const removeItem = (index: number) => {
    if (items.length === 1) {
      return
    }

    setItems((current) =>
      current.filter(
        (_, itemIndex) => itemIndex !== index
      )
    )

    setError('')
  }

  const validate = (): string | null => {
    // Check logged-in user
    if (!user?.id || user.id <= 0) {
      return 'Unable to identify the current user.'
    }

    // At least one item
    if (items.length === 0) {
      return 'At least one item is required.'
    }

    const selectedIds: number[] = []

    for (const item of items) {
      // Validate Item ID
      if (
        item.itemId === '' ||
        item.itemId <= 0
      ) {
        return 'Please enter a valid Item ID for every row.'
      }

      // Item ID must be integer
      if (!Number.isInteger(item.itemId)) {
        return 'Item ID must be a whole number.'
      }

      // Validate quantity
      if (
        item.quantity === '' ||
        item.quantity <= 0
      ) {
        return 'Quantity must be greater than 0.'
      }

      // Quantity must be integer
      if (!Number.isInteger(item.quantity)) {
        return 'Quantity must be a whole number.'
      }

      // Prevent duplicate Item IDs
      if (selectedIds.includes(item.itemId)) {
        return `Item ID ${item.itemId} has already been added.`
      }

      selectedIds.push(item.itemId)
    }

    return null
  }

  const handleSubmit = async () => {
    const validationError = validate()

    if (validationError) {
      setError(validationError)
      return
    }

    if (!user) {
      setError('Unable to identify the current user.')
      return
    }

    try {
      setSubmitting(true)
      setError('')

      const requestData = {
        requestedBy: user.id,

        items: items.map((item) => ({
          itemId: Number(item.itemId),
          quantity: Number(item.quantity),
        })),
      }

      console.log(
        'Creating order request:',
        requestData
      )

      await orderRequestApi.create(requestData)

      // Success
      setItems([
        {
          itemId: '',
          quantity: '',
        },
      ])

      onSuccess()
      onClose()

    } catch (error: unknown) {
      console.error(
        'Create order request error:',
        error
      )

      let message =
        'Unable to create order request.'

      /*
       * Axios error
       */
      if (axios.isAxiosError(error)) {

        const status = error.response?.status
        const data = error.response?.data

        console.log(
          'Backend status:',
          status
        )

        console.log(
          'Backend response:',
          data
        )

        /*
         * Our BusinessException middleware
         * should return:
         *
         * {
         *   "message": "The following item IDs were not found: 9999."
         * }
         */
        if (data?.message) {
          message = data.message
        }

        /*
         * ASP.NET ProblemDetails may return
         * a title instead.
         */
        else if (data?.title) {
          message = data.title
        }

        /*
         * Handle 404 specifically
         */
        else if (status === 404) {
          message =
            error.response?.data?.message ||
            'The requested item was not found.'
        }

        /*
         * Handle 400
         */
        else if (status === 400) {
          message =
            'The order request contains invalid data.'
        }

        /*
         * Handle 401
         */
        else if (status === 401) {
          message =
            'You are not authorized. Please log in again.'
        }

        /*
         * Handle 403
         */
        else if (status === 403) {
          message =
            'You do not have permission to create an order request.'
        }

        /*
         * Handle 500
         */
        else if (status === 500) {
          message =
            'A server error occurred. Please try again later.'
        }

        /*
         * Network error
         */
        else if (error.request && !error.response) {
          message =
            'Unable to connect to the server. Please make sure the backend is running.'
        }
      }

      /*
       * Normal JavaScript Error
       */
      else if (error instanceof Error) {
        message = error.message
      }

      setError(message)

    } finally {
      setSubmitting(false)
    }
  }

  const handleClose = () => {
    if (submitting) {
      return
    }

    setError('')

    setItems([
      {
        itemId: '',
        quantity: '',
      },
    ])

    onClose()
  }

  return (
    <Dialog
      open={open}
      onClose={
        submitting
          ? undefined
          : handleClose
      }
      fullWidth
      maxWidth="md"
    >

      <DialogTitle>
        Create New Order Request
      </DialogTitle>

      <DialogContent dividers>

        {/* Error */}

        {error && (
          <Alert
            severity="error"
            sx={{ mb: 3 }}
            onClose={() => setError('')}
          >
            {error}
          </Alert>
        )}

        {/* Requester */}

        <Box sx={{ mb: 3 }}>
          <Typography
            variant="body2"
            color="text.secondary"
          >
            Requester
          </Typography>

          <Typography fontWeight={600}>
            {user?.fullName || 'Unknown User'}{' '}

            {user?.username &&
              `(${user.username})`}
          </Typography>
        </Box>

        {/* Items title */}

        <Typography
          variant="h6"
          sx={{ mb: 2 }}
        >
          Requested Items
        </Typography>

        {/* Items */}

        {items.map((item, index) => (
          <Box
            key={index}
            sx={{
              display: 'flex',
              gap: 2,
              alignItems: 'center',
              mb: 2,
            }}
          >

            {/* Item ID */}

            <Autocomplete
              fullWidth
              options={availableItems}
              loading={itemsLoading}
              value={
                availableItems.find(
                  (availableItem) =>
                    availableItem.itemId === item.itemId
                ) || null
              }
              getOptionLabel={(option) =>
                `${option.itemName} (ID: ${option.itemId})`
              }
              isOptionEqualToValue={(option, value) =>
                option.itemId === value.itemId
              }
              onChange={(_, selectedItem) => {
                updateItem(
                  index,
                  'itemId',
                  selectedItem
                    ? selectedItem.itemId
                    : ''
                )
              }}
              disabled={submitting}
              filterOptions={(options, state) => {
                const searchValue =
                  state.inputValue.toLowerCase().trim()

                return options.filter((option) =>
                  `${option.itemName} ${option.itemId}`
                    .toLowerCase()
                    .includes(searchValue)
                )
              }}
              renderOption={(props, option) => (
                <li {...props} key={option.itemId}>
                  <Box>
                    <Typography fontWeight={600}>
                      {option.itemName}
                    </Typography>

                    <Typography
                      variant="body2"
                      color="text.secondary"
                    >
                      Item ID: {option.itemId}
                    </Typography>
                  </Box>
                </li>
              )}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Product"
                  placeholder="Search product..."
                  InputProps={{
                    ...params.InputProps,
                    endAdornment: (
                      <>
                        {itemsLoading && (
                          <CircularProgress
                            size={20}
                          />
                        )}

                        {params.InputProps.endAdornment}
                      </>
                    ),
                  }}
                />
              )}
            />

            {/* Quantity */}

            <TextField
              label="Quantity"
              type="number"
              value={item.quantity}
              onChange={(event) =>
                updateItem(
                  index,
                  'quantity',
                  event.target.value === ''
                    ? ''
                    : Number(
                      event.target.value
                    )
                )
              }
              inputProps={{
                min: 1,
                step: 1,
              }}
              disabled={submitting}
              sx={{
                width: 180,
              }}
            />

            {/* Delete */}

            <IconButton
              color="error"
              onClick={() =>
                removeItem(index)
              }
              disabled={
                submitting ||
                items.length === 1
              }
            >
              <DeleteIcon />
            </IconButton>

          </Box>
        ))}

        {/* Add item */}

        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={addItem}
          disabled={submitting}
        >
          Add Item
        </Button>

        {/* Help */}

        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ mt: 2 }}
        >
          Select a product from the inventory and enter
          the required quantity.
        </Typography>

      </DialogContent>

      <DialogActions>

        <Button
          onClick={handleClose}
          disabled={submitting}
        >
          Cancel
        </Button>

        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={submitting}
        >
          {submitting
            ? 'Creating...'
            : 'Create Request'}
        </Button>

      </DialogActions>

    </Dialog>
  )
}