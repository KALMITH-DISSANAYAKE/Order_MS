import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import {
  TextField,
  Button,
  FormControlLabel,
  Checkbox,
  InputAdornment,
  IconButton,
  Paper,
  Box,
  Typography,
  Divider,
  Chip,
  Alert,
} from '@mui/material'
import { Visibility, VisibilityOff } from '@mui/icons-material'
import { useAuth } from '../../contexts/AuthContext'

export default function Login() {
  const navigate = useNavigate()
  const { login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    // TODO: Replace with real API call to your .NET backend
    setTimeout(() => {
      if (username === 'admin' && password === 'admin') {
        login({
          id: 1,
          username,
          fullName: 'Admin User',
          role: 'Admin',
          token: 'mock-jwt-token',
        })
        setLoading(false)
        navigate('/dashboard')
      } else {
        setError('Invalid username or password')
        setLoading(false)
      }
    }, 1000)
  }

  const fillDemo = (u: string) => {
    setUsername(u)
    setPassword('Password123!')
  }

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
      <div className="w-full lg:w-1/2 flex items-center justify-center bg-[#F0F2F5] p-6">
        <Paper elevation={0} className="w-full max-w-md p-8 lg:p-10 rounded-2xl shadow-md">
          <Box className="mb-8">
            <Typography variant="h4" className="!font-bold !text-[#1A1A1A] !mb-1">
              Welcome Back
            </Typography>
            <Typography variant="body1" className="!text-[#666666]">
              Sign in to your account
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" className="!mb-4 !rounded-xl">
              {error}
            </Alert>
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-5">
            <TextField
              label="Username"
              type="text"
              placeholder="Enter your username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              fullWidth
              required
              autoFocus
            />

            <TextField
              label="Password"
              type={showPassword ? 'text' : 'password'}
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
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

            <div className="flex items-center justify-between">
              <FormControlLabel
                control={<Checkbox size="small" className="!text-cargills-red" />}
                label={<span className="text-sm text-gray-600">Remember me</span>}
              />
              <Link
                to="#"
                className="text-sm font-semibold text-cargills-red hover:text-cargills-dark hover:underline"
              >
                Forgot Password?
              </Link>
            </div>

            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              size="large"
              disabled={loading}
              className="!mt-2 !normal-case !font-bold !tracking-wide"
            >
              {loading ? 'Signing in...' : 'SIGN IN'}
            </Button>
          </form>

          <Divider className="!my-6">
            <Typography variant="caption" className="!text-gray-400 !px-2">
              or
            </Typography>
          </Divider>

          <Typography className="!text-center !text-gray-600 !mb-6">
            Don't have an account?{' '}
            <Link
              to="/register"
              className="font-bold text-cargills-red hover:text-cargills-dark hover:underline"
            >
              Register here
            </Link>
          </Typography>

          {/* Demo accounts */}
          <Box className="bg-cargills-light border border-[#FFD6D6] rounded-xl p-4 text-center">
            <Typography className="!text-[0.7rem] !font-bold !text-cargills-red !uppercase !tracking-wider !mb-3">
              Demo Accounts (click to fill)
            </Typography>
            <Box className="flex flex-wrap justify-center gap-2 mb-2">
              {['bm_colombo', 'inv_manager', 'transport_user'].map((acc) => (
                <Chip
                  key={acc}
                  label={acc}
                  size="small"
                  onClick={() => fillDemo(acc)}
                  className="!bg-white !text-cargills-red !border !border-[#FFCDD2] !font-semibold !cursor-pointer hover:!bg-gray-50"
                />
              ))}
            </Box>
            <Typography className="!text-xs !text-gray-500">
              Password: Password123!
            </Typography>
          </Box>
        </Paper>
      </div>
    </div>
  )
}
