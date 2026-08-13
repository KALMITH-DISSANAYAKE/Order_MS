import { useState } from 'react'
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

interface UserRow {
  id: number
  firstName: string
  lastName: string
  username: string
  role: string
  branch: string
  isActive: boolean
}

const mockUsers: UserRow[] = [
  { id: 1, firstName: 'Kamal', lastName: 'Perera', username: 'bm_colombo', role: 'BranchManager', branch: 'Colombo - Bambalapitiya', isActive: true },
  { id: 2, firstName: 'Nimal', lastName: 'Fernando', username: 'inv_manager', role: 'InventoryManager', branch: '-', isActive: true },
  { id: 3, firstName: 'Sunil', lastName: 'Silva', username: 'transport_user', role: 'TransportDepartment', branch: '-', isActive: true },
  { id: 4, firstName: 'Amara', lastName: 'Bandara', username: 'bm_kandy', role: 'BranchManager', branch: 'Kandy - City Center', isActive: false },
  { id: 5, firstName: 'Sajith', lastName: 'Rajapaksa', username: 'bm_galle', role: 'BranchManager', branch: 'Galle - Fort', isActive: true },
]

const roleOptions = ['BranchManager', 'InventoryManager', 'TransportDepartment']
const branchOptions = ['Colombo - Bambalapitiya', 'Colombo - Nugegoda', 'Kandy - City Center', 'Galle - Fort']

export default function UsersPage() {
  const [users, setUsers] = useState<UserRow[]>(mockUsers)
  const [search, setSearch] = useState('')
  const [open, setOpen] = useState(false)
  const [editing, setEditing] = useState<UserRow | null>(null)
  const [showPassword, setShowPassword] = useState(false)

  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    username: '',
    password: '',
    role: '',
    branch: '',
  })

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

  const handleSave = () => {
    if (editing) {
      setUsers((prev) =>
        prev.map((u) =>
          u.id === editing.id
            ? { ...u, firstName: form.firstName, lastName: form.lastName, username: form.username, role: form.role, branch: form.branch || '-' }
            : u
        )
      )
    } else {
      const newUser: UserRow = {
        id: Math.max(...users.map((u) => u.id), 0) + 1,
        firstName: form.firstName,
        lastName: form.lastName,
        username: form.username,
        role: form.role,
        branch: form.branch || '-',
        isActive: true,
      }
      setUsers((prev) => [...prev, newUser])
    }
    handleClose()
  }

  const handleDelete = (id: number) => {
    if (confirm('Are you sure you want to delete this user?')) {
      setUsers((prev) => prev.filter((u) => u.id !== id))
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

      {/* Toolbar */}
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

      {/* Table */}
      <Box className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <DataGrid
          rows={filtered}
          columns={columns}
          initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
          pageSizeOptions={[5, 10, 25]}
          disableRowSelectionOnClick
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

      {/* Add/Edit Dialog */}
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
          <Button onClick={handleSave} variant="contained" color="primary" className="!normal-case !font-semibold">
            {editing ? 'Save Changes' : 'Create User'}
          </Button>
        </DialogActions>
      </Dialog>
    </div>
  )
}
