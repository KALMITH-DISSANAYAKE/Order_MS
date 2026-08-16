import { Typography, Box } from '@mui/material'
import { ReactNode } from 'react'

interface PageHeaderProps {
  title: string
  subtitle?: string
  action?: ReactNode
}

export default function PageHeader({ title, subtitle, action }: PageHeaderProps) {
  return (
    <Box className="mb-6 flex justify-between items-start">
      <Box>
        <Typography variant="h5" className="!font-bold !text-[#1A1A1A]">
          {title}
        </Typography>
        {subtitle && (
          <Typography variant="body2" className="!text-gray-500 mt-1">
            {subtitle}
          </Typography>
        )}
      </Box>
      {action && (
        <Box>
          {action}
        </Box>
      )}
    </Box>
  )
}
