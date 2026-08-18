import { useEffect, useState } from 'react'
import PageHeader from '../../components/common/PageHeader'
import {
  Paper,
  Typography,
  Grid,
  Box,
  LinearProgress,
  Chip,
} from '@mui/material'
import {
  People,
  AccountTree,
  RequestQuote,
  LocalShipping,
  Inventory2,
  PendingActions,
  CheckCircle,
  Cancel,
  TrendingUp,
} from '@mui/icons-material'
import axiosInstance from '../../api/axiosInstance'
import { useAuth } from '../../contexts/AuthContext'

interface StatCard {
  label: string
  value: number
  total?: number
  icon: React.ReactNode
  color: string
  bg: string
  subtitle?: string
}

interface StatusBreakdown {
  label: string
  count: number
  color: string
}

export default function Dashboard() {
  const { user } = useAuth()
  const [stats, setStats] = useState<StatCard[]>([])
  const [statuses, setStatuses] = useState<StatusBreakdown[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadDashboard()
  }, [user?.role])

  const loadDashboard = async () => {
    if (!user) return
    setLoading(true)

    try {
      // Fetch all existing endpoints in parallel
      const [usersRes, branchesRes, ordersRes, requestsRes, inventoryRes, assignmentsRes] = await Promise.all([
        axiosInstance.get('/users').catch(() => ({ data: [] })),
        axiosInstance.get('/branches').catch(() => ({ data: [] })),
        axiosInstance.get('/orders').catch(() => ({ data: [] })),
        axiosInstance.get('/OrderRequest').catch(() => ({ data: [] })),
        axiosInstance.get('/inventory').catch(() => ({ data: [] })),
        axiosInstance.get('/Transport/assignments').catch(() => ({ data: [] })),
      ])

      const users = usersRes.data
      const branches = branchesRes.data
      const orders = ordersRes.data
      const requests = requestsRes.data
      const inventory = inventoryRes.data
      const assignments = assignmentsRes.data

      const role = user.role
      const cards: StatCard[] = []
      const breakdowns: StatusBreakdown[] = []

      // ─── HELPERS ───
      const getStatus = (item: any) =>
        item.reqStatus || item.status || item.orderStatus || item.ReqStatus || item.Status || 'Unknown'

      const countByStatus = (arr: any[], statusVal: string) =>
        arr.filter((x) => getStatus(x) === statusVal).length

      // ─── ADMIN ───
      if (role === 'Admin') {
        const pendingReqs = countByStatus(requests, 'Pending')
        const approvedReqs = countByStatus(requests, 'Approved')
        const rejectedReqs = countByStatus(requests, 'Rejected')
        const totalReqs = requests.length

        cards.push(
          {
            label: 'Total Users',
            value: users.length,
            icon: <People fontSize="small" />,
            color: '#D42027',
            bg: '#FFF0F0',
            subtitle: 'System staff',
          },
          {
            label: 'Total Branches',
            value: branches.length,
            icon: <AccountTree fontSize="small" />,
            color: '#1565C0',
            bg: '#E3F2FD',
            subtitle: 'Supermarket locations',
          },
          {
            label: 'Total Orders',
            value: orders.length,
            icon: <LocalShipping fontSize="small" />,
            color: '#2E7D32',
            bg: '#E8F5E9',
            subtitle: 'All time orders',
          },
          {
            label: 'Order Requests',
            value: totalReqs,
            total: totalReqs,
            icon: <RequestQuote fontSize="small" />,
            color: '#ED6C02',
            bg: '#FFF3E0',
            subtitle: `${pendingReqs} pending approval`,
          }
        )

        breakdowns.push(
          { label: 'Pending', count: pendingReqs, color: '#ED6C02' },
          { label: 'Approved', count: approvedReqs, color: '#2E7D32' },
          { label: 'Rejected', count: rejectedReqs, color: '#D42027' },
          { label: 'Total', count: totalReqs, color: '#666666' },
        )
      }

      // ─── BRANCH MANAGER ───
      else if (role === 'BranchManager') {
        const myReqs = requests.filter(
          (r: any) => r.requestedBy === user.id || r.RequestedBy === user.id || r.userId === user.id
        )
        const myPending = countByStatus(myReqs, 'Pending')
        const myApproved = countByStatus(myReqs, 'Approved')
        const myRejected = countByStatus(myReqs, 'Rejected')

        // Low stock from inventory (adjust property names to match your DB)
        const lowStock = inventory.filter(
          (i: any) => (i.quantity || i.Quantity || 0) < 10 && (i.branchId || i.BranchId) === user.branchId
        ).length

        cards.push(
          {
            label: 'My Requests',
            value: myReqs.length,
            icon: <RequestQuote fontSize="small" />,
            color: '#D42027',
            bg: '#FFF0F0',
            subtitle: 'Submitted by you',
          },
          {
            label: 'Pending',
            value: myPending,
            icon: <PendingActions fontSize="small" />,
            color: '#ED6C02',
            bg: '#FFF3E0',
            subtitle: 'Awaiting approval',
          },
          {
            label: 'Approved',
            value: myApproved,
            icon: <CheckCircle fontSize="small" />,
            color: '#2E7D32',
            bg: '#E8F5E9',
            subtitle: 'Ready to process',
          },
          {
            label: 'Low Stock',
            value: lowStock,
            icon: <Inventory2 fontSize="small" />,
            color: '#C62828',
            bg: '#FFEBEE',
            subtitle: 'Items below threshold',
          }
        )

        breakdowns.push(
          { label: 'Pending', count: myPending, color: '#ED6C02' },
          { label: 'Approved', count: myApproved, color: '#2E7D32' },
          { label: 'Rejected', count: myRejected, color: '#D42027' },
          { label: 'Total', count: myReqs.length, color: '#666666' },
        )
      }

      // ─── INVENTORY MANAGER ───
      else if (role === 'InventoryManager') {
        const pendingReqs = countByStatus(requests, 'Pending')
        const approvedReqs = countByStatus(requests, 'Approved')
        const rejectedReqs = countByStatus(requests, 'Rejected')

        cards.push(
          {
            label: 'Pending Approvals',
            value: pendingReqs,
            icon: <PendingActions fontSize="small" />,
            color: '#ED6C02',
            bg: '#FFF3E0',
            subtitle: 'Need your review',
          },
          {
            label: 'Approved',
            value: approvedReqs,
            icon: <CheckCircle fontSize="small" />,
            color: '#2E7D32',
            bg: '#E8F5E9',
            subtitle: 'Requests approved',
          },
          {
            label: 'Rejected',
            value: rejectedReqs,
            icon: <Cancel fontSize="small" />,
            color: '#D42027',
            bg: '#FFF0F0',
            subtitle: 'Requests declined',
          },
          {
            label: 'Total Orders',
            value: orders.length,
            icon: <TrendingUp fontSize="small" />,
            color: '#1565C0',
            bg: '#E3F2FD',
            subtitle: 'System wide orders',
          }
        )

        breakdowns.push(
          { label: 'Pending', count: pendingReqs, color: '#ED6C02' },
          { label: 'Approved', count: approvedReqs, color: '#2E7D32' },
          { label: 'Rejected', count: rejectedReqs, color: '#D42027' },
          { label: 'Total', count: requests.length, color: '#666666' },
        )
      }

      // ─── TRANSPORT DEPARTMENT ───
      else if (role === 'TransportDepartment') {
        const assigned = countByStatus(requests, 'TransportAssigned')
        const pendingDel = countByStatus(requests, 'Approved')

        cards.push(
          {
            label: 'Assigned',
            value: assigned,
            icon: <LocalShipping fontSize="small" />,
            color: '#1565C0',
            bg: '#E3F2FD',
            subtitle: 'Ready for delivery',
          },
          {
            label: 'Pending Delivery',
            value: pendingDel,
            icon: <PendingActions fontSize="small" />,
            color: '#ED6C02',
            bg: '#FFF3E0',
            subtitle: 'In progress',
          },
          {
            label: 'Total Branches',
            value: branches.length,
            icon: <AccountTree fontSize="small" />,
            color: '#D42027',
            bg: '#FFF0F0',
            subtitle: 'Delivery locations',
          }
        )

        breakdowns.push(
          { label: 'Pending Delivery', count: pendingDel, color: '#ED6C02' },
          { label: 'Assigned', count: assigned, color: '#1565C0' },
          { label: 'Total', count: assigned + pendingDel, color: '#666666' },
        )
      }

      setStats(cards)
      setStatuses(breakdowns)
    } catch (err) {
      console.error('Dashboard load error', err)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div>
        <PageHeader title="Dashboard" subtitle="Loading your overview..." />
        <Box className="mt-8">
          <LinearProgress className="!bg-cargills-light" />
        </Box>
      </div>
    )
  }

  return (
    <div>
      <PageHeader
        title="Dashboard"
        subtitle={`Welcome back, ${user?.fullName} — here's what's happening`}
      />

      {/* ─── STAT CARDS ─── */}
      <Grid container spacing={3} className="mb-8">
        {stats.map((stat, idx) => (
          <Grid item xs={12} sm={6} lg={3} key={idx}>
            <Paper
              elevation={0}
              className="p-5 rounded-xl border border-gray-100 h-full"
              sx={{
                background: 'linear-gradient(135deg, #FFFFFF 0%, #FAFAFA 100%)',
                transition: 'all 0.2s ease',
                '&:hover': {
                  boxShadow: '0 8px 24px rgba(212, 32, 39, 0.08)',
                  transform: 'translateY(-2px)',
                },
              }}
            >
              <Box className="flex items-start justify-between mb-3">
                <Box
                  className="w-10 h-10 rounded-lg flex items-center justify-center"
                  style={{ backgroundColor: stat.bg, color: stat.color }}
                >
                  {stat.icon}
                </Box>
                {stat.total !== undefined && stat.total > 0 && (
                  <Chip
                    label={`${Math.round((stat.value / stat.total) * 100)}%`}
                    size="small"
                    className="!text-xs !font-bold"
                    style={{
                      backgroundColor: stat.bg,
                      color: stat.color,
                      border: `1px solid ${stat.color}20`,
                    }}
                  />
                )}
              </Box>

              <Typography
                variant="h4"
                className="!font-bold !text-[#1A1A1A] !mb-0.5"
              >
                {stat.value}
              </Typography>

              <Typography className="!text-sm !font-semibold !text-gray-700 !mb-0.5">
                {stat.label}
              </Typography>

              <Typography className="!text-xs !text-gray-400">
                {stat.subtitle}
              </Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>

      {/* ─── STATUS BREAKDOWN ─── */}
      {statuses.length > 0 && (
        <Paper
          elevation={0}
          className="p-6 rounded-xl border border-gray-100"
          sx={{ background: 'linear-gradient(135deg, #FFFFFF 0%, #FAFAFA 100%)' }}
        >
          <Typography
            variant="h6"
            className="!font-bold !text-[#1A1A1A] !mb-5"
          >
            Status Overview
          </Typography>

          <Grid container spacing={3}>
            {statuses.map((s, idx) => {
              const max = Math.max(...statuses.map((x) => x.count), 1)
              const pct = Math.round((s.count / max) * 100)

              return (
                <Grid item xs={12} sm={6} md={3} key={idx}>
                  <Box className="mb-2">
                    <Box className="flex justify-between items-center mb-1.5">
                      <Typography className="!text-sm !font-medium !text-gray-600">
                        {s.label}
                      </Typography>
                      <Typography
                        className="!text-sm !font-bold"
                        style={{ color: s.color }}
                      >
                        {s.count}
                      </Typography>
                    </Box>

                    <Box className="h-2 w-full bg-gray-100 rounded-full overflow-hidden">
                      <Box
                        className="h-full rounded-full transition-all duration-500"
                        style={{
                          width: `${pct}%`,
                          backgroundColor: s.color,
                        }}
                      />
                    </Box>
                  </Box>
                </Grid>
              )
            })}
          </Grid>
        </Paper>
      )}

    </div>
  )
}