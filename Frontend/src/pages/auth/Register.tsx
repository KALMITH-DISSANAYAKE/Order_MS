import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import {
  TextField,
  Button,
  Paper,
  Box,
  Typography,
  Divider,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  InputAdornment,
  IconButton,
  Alert,
} from '@mui/material'
import { Visibility, VisibilityOff } from '@mui/icons-material'

export default function Register() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    username: '',
    password: '',
    role: '',
    branch: '',
  })
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const handleChange = (field: string, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setLoading(true)

    // TODO: Replace with real API call
    setTimeout(() => {
      setSuccess('Account created successfully! Redirecting to login...')
      setLoading(false)
      setTimeout(() => navigate('/login'), 1500)
    }, 1200)
  }

  const isBranchManager = form.role === 'BranchManager'

  return (
    <div className="flex min-h-screen w-full">
      {/* LEFT BRANDING */}
      <div className="hidden lg:flex lg:w-1/2 relative items-center justify-center bg-gradient-to-br from-[#9a181e] via-[#d42027] to-[#e8454c] overflow-hidden p-12">
        <div className="absolute -top-20 -right-20 w-[500px] h-[500px] rounded-full bg-white/5" />
        <div className="absolute bottom-[15%] -left-16 w-[280px] h-[280px] rounded-full bg-white/5" />
        <div className="absolute -bottom-8 right-1/4 w-[160px] h-[160px] rounded-full bg-white/10" />

        <div className="relative z-10 text-white max-w-md">
          <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mb-6 shadow-xl">
            <span className="text-[#d42027] text-2xl font-extrabold">C</span>
          </div>
          <Typography variant="h3" className="!font-extrabold !tracking-wider !text-white !text-5xl">
            CARGILLS
          </Typography>
          <Typography variant="h5" className="!font-light !text-white/90 !mb-6 !text-2xl">
            Food City
          </Typography>
          <div className="w-12 h-1 bg-white/50 rounded mb-6" />
          <Typography className="!text-white/90 !mb-10 !text-lg">
            Order Management System
          </Typography>
          <ul className="space-y-3 text-white/90">
            {[
              'Branch Inventory Tracking',
              'Order Request Workflow',
              'Transport Assignment',
              'Delivery Verification',
            ].map((item) => (
              <li key={item} className="flex items-center gap-3">
                <span className="w-6 h-6 rounded-full bg-white/20 flex items-center justify-center text-xs backdrop-blur-sm">
                  ✓
                </span>
                {item}
              </li>
            ))}
          </ul>
        </div>
      </div>

      {/* RIGHT FORM */}
      <div className="w-full lg:w-1/2 flex items-center justify-center bg-[#F0F2F5] p-6 overflow-y-auto">
        <Paper elevation={0} className="w-full max-w-md p-8 lg:p-10 rounded-2xl shadow-md my-auto">
          <Box className="mb-6">
            <Typography variant="h4" className="!font-bold !text-[#1A1A1A] !mb-1">
              Create Account
            </Typography>
            <Typography variant="body1" className="!text-[#666666]">
              Register a new staff account
            </Typography>
          </Box>

          {error && <Alert severity="error" className="!mb-4 !rounded-xl">{error}</Alert>}
          {success && <Alert severity="success" className="!mb-4 !rounded-xl">{success}</Alert>}

          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex gap-4">
              <TextField
                label="First Name"
                placeholder="Kamal"
                value={form.firstName}
                onChange={(e) => handleChange('firstName', e.target.value)}
                fullWidth
                required
              />
              <TextField
                label="Last Name"
                placeholder="Perera"
                value={form.lastName}
                onChange={(e) => handleChange('lastName', e.target.value)}
                fullWidth
                required
              />
            </div>

            <TextField
              label="Username"
              placeholder="bm_colombo"
              value={form.username}
              onChange={(e) => handleChange('username', e.target.value)}
              fullWidth
              required
            />

            <TextField
              label="Password"
              type={showPassword ? 'text' : 'password'}
              placeholder="••••••••"
              value={form.password}
              onChange={(e) => handleChange('password', e.target.value)}
              fullWidth
              required
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowPassword(!showPassword)}
                      edge="end"
                      className="!text-gray-400"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <FormControl fullWidth required>
              <InputLabel>Role</InputLabel>
              <Select
                value={form.role}
                label="Role"
                onChange={(e) => handleChange('role', e.target.value)}
              >
                <MenuItem value=""><em>Select a role</em></MenuItem>
                <MenuItem value="BranchManager">Branch Manager</MenuItem>
                <MenuItem value="InventoryManager">Inventory Manager</MenuItem>
                <MenuItem value="TransportDepartment">Transport Department</MenuItem>
              </Select>
            </FormControl>

            {isBranchManager && (
              <FormControl fullWidth required={isBranchManager}>
                <InputLabel>Branch</InputLabel>
                <Select
                  value={form.branch}
                  label="Branch"
                  onChange={(e) => handleChange('branch', e.target.value)}
                >
                  <MenuItem value=""><em>Select a branch</em></MenuItem>
                  <MenuItem value="1">Colombo - Bambalapitiya (CMB-001)</MenuItem>
                  <MenuItem value="2">Colombo - Nugegoda (CMB-002)</MenuItem>
                  <MenuItem value="3">Kandy - City Center (KDY-001)</MenuItem>
                </Select>
              </FormControl>
            )}

            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              size="large"
              disabled={loading}
              className="!mt-2 !normal-case !font-bold !tracking-wide"
            >
              {loading ? 'Creating Account...' : 'CREATE ACCOUNT'}
            </Button>
          </form>

          <Divider className="!my-6">
            <Typography variant="caption" className="!text-gray-400 !px-2">or</Typography>
          </Divider>

          <Typography className="!text-center !text-gray-600">
            Already have an account?{' '}
            <Link to="/login" className="font-bold text-cargills-red hover:text-cargills-dark hover:underline">
              Sign In
            </Link>
          </Typography>
        </Paper>
      </div>
    </div>
  )
}
