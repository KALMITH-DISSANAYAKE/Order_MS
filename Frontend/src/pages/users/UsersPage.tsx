import { useEffect, useState } from 'react'
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  IconButton,
  Chip,
  InputAdornment,
  CircularProgress,
} from '@mui/material'
import {
  DataGrid,
  GridColDef,
  GridRenderCellParams,
} from '@mui/x-data-grid'
import {
  Add,
  Edit,
  Delete,
  Search,
  Visibility,
  VisibilityOff,
} from '@mui/icons-material'
import PageHeader from '../../components/common/PageHeader'
import axiosInstance from '../../api/axiosInstance'

interface UserRow {
  id: number
  firstName: string
  lastName: string
  username: string
  role: string
  branch: string
  isActive: boolean
}

const roleOptions = ['BranchManager', 'InventoryManager', 'TransportDepartment']
const branchOptions = ['Colombo - Bambalapitiya', 'Colombo - Nugegoda', 'Kandy - City Center', 'Galle - Fort']

// Map role name to role_id (must match your DB)
const roleToId: Record<string, number> = {
  'BranchManager': 1,
  'InventoryManager': 2,
  'TransportDepartment': 3,
}

// Map branch name to branch_id (must match your DB)
const branchToId: Record<string, number> = {
  'Colombo - Bambalapitiya': 1,
  'Colombo - Nugegoda': 2,
  'Kandy - City Center': 3,
  'Galle - Fort': 4,
}

export default function UsersPage() {
  const [users, setUsers] = useState<UserRow[]>([])
  const [search, setSearch] = useState('')
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<UserRow | null>(null)
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [tableLoading, setTableLoading] = useState(false)

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    username: '',
    password: '',
    role: '',
    branch: '',
  })

  // ─── FETCH FROM BACKEND ON LOAD ───
  useEffect(() => {
    fetchUsers()
  }, [])

  const fetchUsers = async () => {
    try {
      setTableLoading(true)
      const res = await axiosInstance.get('/users')
      // Map backend DTO to frontend format
      const mapped = res.data.map((u: any) => ({
        id: u.id,
        firstName: u.firstName,
        lastName: u.lastName,
        username: u.userName,
        role: u.role,
        branch: u.branchName || '-',
        isActive: true, // Add isActive to your backend UserDto if you want this
      }))
      setUsers(mapped)
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to load users')
    } finally {
      setTableLoading(false)
    }
  }

  const filtered = users.filter((u) =>
    `${u.firstName} ${u.lastName} ${u.username} ${u.role}`.toLowerCase().includes(search.toLowerCase())
  )

  const handleOpen = (user?: UserRow) => {
    if (user) {
      setEditing(user)
      setForm({
        firstName: user.firstName,
        lastName: user.lastName,
        username: user.username,
        password: '',
        role: user.role,
        branch: user.branch === '-' ? '' : user.branch,
      })
    } else {
      setEditing(null)
      setForm({ firstName: '', lastName: '', username: '', password: '', role: '', branch: '' })
    }
    setShowPassword(false)
    setOpen(true)
  }

  const handleClose = () => {
    setOpen(false)
    setEditing(null)
  }

  // ─── CREATE / UPDATE ───
  const handleSave = async () => {
    const payload = {
      firstName: form.firstName,
      lastName: form.lastName,
      userName: form.username,
      password: form.password,
      roleId: roleToId[form.role],
      branchId: isBranchManager && form.branch ? branchToId[form.branch] : null,
    }

    try {
      setLoading(true)
      if (editing) {
        await axiosInstance.put(`/users/${editing.id}`, payload)
      } else {
        await axiosInstance.post('/users', payload)
      }
      await fetchUsers() // Refresh from DB
      handleClose()
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to save user')
    } finally {
      setLoading(false)
    }
  }

  // ─── DELETE ───
  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this user?')) return
    try {
      await axiosInstance.delete(`/users/${id}`)
      await fetchUsers() // Refresh from DB
    } catch (err: any) {
      alert(err.response?.data?.message || 'Failed to delete user')
    }
  }

  const isBranchManager = form.role === 'BranchManager'

  const columns: GridColDef<UserRow>[] = [
    { field: 'id', headerName: 'ID', width: 70 },
    {
      field: 'fullName',
      headerName: 'Full Name',
      width: 180,
      valueGetter: (_, row) => `${row.firstName} ${row.lastName}`,
    },
    { field: 'username', headerName: 'Username', width: 150 },
    {
      field: 'role',
      headerName: 'Role',
      width: 180,
      renderCell: (params: GridRenderCellParams<UserRow>) => (
        <Chip
          label={params.value}
          size="small"
          className={
            params.value === 'BranchManager'
              ? '!bg-blue-50 !text-blue-700 !border !border-blue-200'
              : params.value === 'InventoryManager'
              ? '!bg-purple-50 !text-purple-700 !border !border-purple-200'
              : '!bg-orange-50 !text-orange-700 !border !border-orange-200'
          }
        />
      ),
    },
    { field: 'branch', headerName: 'Branch', width: 200 },
    {
      field: 'isActive',
      headerName: 'Status',
      width: 120,
      renderCell: (params: GridRenderCellParams<UserRow>) => (
        <Chip
          label={params.value ? 'Active' : 'Inactive'}
          size="small"
          className={
            params.value
              ? '!bg-green-50 !text-green-700 !border !border-green-200'
              : '!bg-gray-100 !text-gray-500 !border !border-gray-200'
          }
        />
      ),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 120,
      sortable: false,
      filterable: false,
      renderCell: (params: GridRenderCellParams<UserRow>) => (
        <Box className="flex gap-1">
          <IconButton size="small" onClick={() => handleOpen(params.row)} className="!text-blue-600">
            <Edit fontSize="small" />
          </IconButton>
          <IconButton size="small" onClick={() => handleDelete(params.row.id)} className="!text-red-600">
            <Delete fontSize="small" />
          </IconButton>
        </Box>
      ),
    },
  ]

  return (
    <div>
      <PageHeader title="User Management" subtitle="Manage staff accounts, roles, and branch assignments" />

      <Box className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
        <TextField
          placeholder="Search users..."
          size="small"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search className="text-gray-400" fontSize="small" />
              </InputAdornment>
            ),
          }}
          className="w-full sm:w-72"
        />
        <Button
          variant="contained"
          color="primary"
          startIcon={<Add />}
          onClick={() => handleOpen()}
          className="!normal-case !font-semibold"
        >
          Add User
        </Button>
      </Box>

      <Box className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <DataGrid
          rows={filtered}
          columns={columns}
          initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
          pageSizeOptions={[5, 10, 25]}
          disableRowSelectionOnClick
          loading={tableLoading}
          sx={{
            border: 'none',
            '& .MuiDataGrid-columnHeaders': {
              backgroundColor: '#FAFAFA',
              fontWeight: 700,
              color: '#1A1A1A',
            },
            '& .MuiDataGrid-cell': {
              fontSize: '0.9rem',
            },
          }}
        />
      </Box>

      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle className="!font-bold !text-lg">
          {editing ? 'Edit User' : 'Add New User'}
        </DialogTitle>
        <DialogContent className="!pt-2">
          <Box className="flex flex-col gap-4 mt-2">
            <div className="flex gap-4">
              <TextField
                label="First Name"
                value={form.firstName}
                onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                fullWidth
                required
              />
              <TextField
                label="Last Name"
                value={form.lastName}
                onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                fullWidth
                required
              />
            </div>
            <TextField
              label="Username"
              value={form.username}
              onChange={(e) => setForm({ ...form, username: e.target.value })}
              fullWidth
              required
            />
            {!editing && (
              <TextField
                label="Password"
                type={showPassword ? 'text' : 'password'}
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
                fullWidth
                required
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={() => setShowPassword(!showPassword)} edge="end" size="small">
                        {showPassword ? <VisibilityOff fontSize="small" /> : <Visibility fontSize="small" />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            )}
            <FormControl fullWidth required>
              <InputLabel>Role</InputLabel>
              <Select
                value={form.role}
                label="Role"
                onChange={(e) => setForm({ ...form, role: e.target.value })}
              >
                {roleOptions.map((r) => (
                  <MenuItem key={r} value={r}>{r}</MenuItem>
                ))}
              </Select>
            </FormControl>
            {isBranchManager && (
              <FormControl fullWidth required={isBranchManager}>
                <InputLabel>Branch</InputLabel>
                <Select
                  value={form.branch}
                  label="Branch"
                  onChange={(e) => setForm({ ...form, branch: e.target.value })}
                >
                  {branchOptions.map((b) => (
                    <MenuItem key={b} value={b}>{b}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
          </Box>
        </DialogContent>
        <DialogActions className="!px-6 !pb-4">
          <Button onClick={handleClose} variant="outlined" color="inherit" className="!normal-case">
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            variant="contained"
            color="primary"
            className="!normal-case !font-semibold"
            disabled={loading}
            startIcon={loading ? <CircularProgress size={16} color="inherit" /> : null}
          >
            {loading ? 'Saving...' : editing ? 'Save Changes' : 'Create User'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  )
}