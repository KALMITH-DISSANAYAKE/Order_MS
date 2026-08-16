import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import {
  AppBar,
  Toolbar,
  Typography,
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  IconButton,
  Divider,
} from '@mui/material'
import { Menu as MenuIcon, Logout, Dashboard, Inventory, RequestQuote, LocalShipping, DeliveryDining, People, AccountTree } from '@mui/icons-material'
import { useAuth } from '../../contexts/AuthContext'
import { useState } from 'react'

const drawerWidth = 260

const navItems = [
  { label: 'Dashboard', path: '/dashboard', icon: <Dashboard fontSize="small" />, roles: ['Admin', 'BranchManager', 'InventoryManager', 'TransportDepartment'] },
  { label: 'Inventory', path: '/inventory', icon: <Inventory fontSize="small" />, roles: ['Admin', 'BranchManager', 'InventoryManager'] },
  { label: 'Order Requests', path: '/order-requests', icon: <RequestQuote fontSize="small" />, roles: ['Admin', 'BranchManager', 'InventoryManager'] },
  { label: 'Orders', path: '/orders', icon: <RequestQuote fontSize="small" />, roles: ['Admin', 'InventoryManager', 'BranchManager'] },
  { label: 'Transport', path: '/transport', icon: <LocalShipping fontSize="small" />, roles: ['Admin', 'TransportDepartment'] },
  { label: 'Delivery', path: '/delivery', icon: <DeliveryDining fontSize="small" />, roles: ['Admin', 'TransportDepartment'] },
  { label: 'Users', path: '/users', icon: <People fontSize="small" />, roles: ['Admin'] },
  { label: 'Branches', path: '/branches', icon: <AccountTree fontSize="small" />, roles: ['Admin'] },
];

export default function DashboardLayout() {
  const { user, logout } = useAuth()

  const visibleNavItems = navItems.filter
  (item => 
  item.roles.includes(user?.role || '')
  );

  const navigate = useNavigate()
  const location = useLocation()
  const [mobileOpen, setMobileOpen] = useState(false)

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const drawer = (
    <Box className="h-full bg-white">
      <Box className="p-6 flex items-center gap-3">
        <div className="w-10 h-10 bg-cargills-red rounded-full flex items-center justify-center text-white font-extrabold text-lg">
          C
        </div>
        <div>
          <Typography className="!font-extrabold !text-cargills-red !text-sm !leading-tight">
            CARGILLS
          </Typography>
          <Typography className="!text-xs !text-gray-500">Food City</Typography>
        </div>
      </Box>
      <Divider />
      <List className="px-3 pt-2">
        {visibleNavItems.map((item) => {
          const isActive = location.pathname === item.path
          return (
            <ListItem key={item.label} disablePadding className="mb-1">
              <ListItemButton
                onClick={() => { navigate(item.path); setMobileOpen(false) }}
                className={`!rounded-lg ${isActive ? '!bg-cargills-light' : 'hover:!bg-gray-50'}`}
              >
                <span className={`mr-3 ${isActive ? 'text-cargills-red' : 'text-gray-500'}`}>
                  {item.icon}
                </span>
                <ListItemText
                  primary={item.label}
                  slotProps={{
                    primary: {
                      className: `!text-sm !font-medium ${isActive ? '!text-cargills-red' : '!text-gray-700'}`,
                    }
                  }}
                />
              </ListItemButton>
            </ListItem>
          )
        })}
      </List>
    </Box>
  )

  return (
    <Box className="flex h-screen">
      <AppBar
        position="fixed"
        className="!bg-white !shadow-sm !text-[#1A1A1A]"
        sx={{ width: { lg: `calc(100% - ${drawerWidth}px)` }, ml: { lg: `${drawerWidth}px` } }}
      >
        <Toolbar className="flex justify-between">
          <IconButton
            color="inherit"
            edge="start"
            onClick={() => setMobileOpen(!mobileOpen)}
            className="lg:!hidden"
          >
            <MenuIcon />
          </IconButton>
          <Typography className="!font-semibold !text-lg hidden lg:block">
            Order Management System
          </Typography>
          <Box className="flex items-center gap-4">
            <Typography className="!text-sm !text-gray-600 hidden sm:block">
              {user?.fullName} ({user?.role})
            </Typography>
            <IconButton onClick={handleLogout} className="!text-cargills-red" title="Logout">
              <Logout />
            </IconButton>
          </Box>
        </Toolbar>
      </AppBar>

      <Box component="nav" className="w-[260px] flex-shrink-0 hidden lg:block">
        <Drawer
          variant="permanent"
          open
          className="!w-[260px]"
          classes={{ paper: '!w-[260px] !border-r !border-gray-200' }}
        >
          {drawer}
        </Drawer>
      </Box>

      <Box className="flex-1 flex flex-col min-w-0 overflow-hidden pt-16">
        <main className="flex-1 overflow-y-auto p-6 bg-[#F0F2F5]">
          <Outlet />
        </main>
      </Box>
    </Box>
  )
}
