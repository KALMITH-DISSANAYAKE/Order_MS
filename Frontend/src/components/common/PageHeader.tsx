import { Typography, Box } from '@mui/material'

interface PageHeaderProps {
  title: string
  subtitle?: string
}

export default function PageHeader({ title, subtitle }: PageHeaderProps) {
  return (
    <Box className="mb-6">
      <Typography variant="h5" className="!font-bold !text-[#1A1A1A]">
        {title}
      </Typography>
      {subtitle && (
        <Typography variant="body2" className="!text-gray-500 mt-1">
          {subtitle}
        </Typography>
      )}
    </Box>
  )
}
