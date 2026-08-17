import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { ThemeProvider } from '@mui/material/styles'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { cargillsTheme } from './theme/theme'
import Login from './pages/auth/Login'
import DashboardLayout from './components/layout/DashboardLayout'
import Dashboard from './pages/dashboard/Dashboard'
import UsersPage from './pages/users/UsersPage'
import BranchesPage from './pages/branches/BranchesPage'
import Inventory from './pages/inventory/inventory'
import TransportList from './pages/transport/TransportList'
import DeliveryList from './pages/delivery/DeliveryList'
import Order from './pages/order/Order'
import OrderRequest from './pages/orderRequests/OrderRequestsPage'
import Box from '@mui/material/Box/Box';
import CircularProgress from '@mui/material/CircularProgress/CircularProgress';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
    // ← WAIT for auth check to finish
  if (isLoading) {
    return (
      <Box className="flex items-center justify-center h-screen">
        <CircularProgress />
      </Box>
    );
  }

  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

function RoleRoute({ allowedRoles, children }: { allowedRoles: string[], children: React.ReactNode }) {
  const { user } = useAuth();
  if (!allowedRoles.includes(user?.role || '')) {
    return <Navigate to="/dashboard" replace />;
  }
  return <>{children}</>;
}

function App() {
  return (
    <ThemeProvider theme={cargillsTheme}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route
              path="/*"
              element={
                <ProtectedRoute>
                  <DashboardLayout />
                </ProtectedRoute>
              }
            >
              <Route path="dashboard" element={<Dashboard />} />
              <Route path="users" element={<RoleRoute allowedRoles={['Admin']}><UsersPage /></RoleRoute>} />
              <Route path="branches" element={<RoleRoute allowedRoles={['Admin']}><BranchesPage /></RoleRoute>} />
              <Route path="inventory" element={<RoleRoute allowedRoles={['Admin', 'BranchManager', 'InventoryManager']}><Inventory /></RoleRoute>} />
              <Route path="order-requests" element={<RoleRoute allowedRoles={['Admin', 'BranchManager', 'InventoryManager']}><OrderRequest /></RoleRoute>} />
              <Route path="order" element={<RoleRoute allowedRoles={['Admin', 'InventoryManager', 'BranchManager']}><Order /></RoleRoute>} />
              <Route path="transport" element={<RoleRoute allowedRoles={['Admin', 'TransportDepartment']}><TransportList /></RoleRoute>} />
              <Route path="delivery" element={<RoleRoute allowedRoles={['Admin', 'TransportDepartment']}><DeliveryList /></RoleRoute>} />
              <Route path="" element={<Navigate to="/dashboard" replace />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;