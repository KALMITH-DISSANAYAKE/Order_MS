import PageHeader from '../../components/common/PageHeader'
import { Paper, Typography, Grid } from '@mui/material'

export default function Dashboard() {
  return (
    <div>
      <PageHeader title="Dashboard" subtitle="Overview of your branch operations" />
      <Grid container spacing={3}>
        {[ 
          { label: 'Total Orders', value: '24' },
          { label: 'Pending Requests', value: '7' },
          { label: 'Low Stock Items', value: '3' },
          { label: 'Deliveries Today', value: '5' },
        ].map((stat) => (
          <Grid item xs={12} sm={6} md={3} key={stat.label}>
            <Paper className="p-5 rounded-xl shadow-sm">
              <Typography className="!text-gray-500 !text-sm !mb-1">{stat.label}</Typography>
              <Typography className="!text-3xl !font-bold !text-[#1A1A1A]">{stat.value}</Typography>
            </Paper>
          </Grid>
        ))}
      </Grid>
    </div>
  )
}
