import PageHeader from '../../components/common/PageHeader'
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material'


const mockInventory = [
  {
    inventoryId: 1,
    branchId: 101,
    branchCode: 'B-101',
    branchLocation: 'Colombo 01',
    itemId: 5001,
    itemName: 'Fresh Milk 1L',
    quantity: 45,
    reorderLevel: 50,
    isBelowReorderLevel: true,
  }
]

export default function Inventory() {
  return (
    <div>
      <PageHeader title="Inventory Management" subtitle="View and manage branch stock levels" />
      
      <TableContainer component={Paper} className="rounded-xl shadow-sm mt-6 overflow-hidden">
        <Table sx={{ minWidth: 650 }} aria-label="inventory table">
          <TableHead className="bg-gray-50">
            <TableRow>
              <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
              <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
              <TableCell className="!font-bold !text-gray-700">Branch</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Reorder Level</TableCell>
              <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {mockInventory.map((row) => (
              <TableRow
                key={row.inventoryId}
                sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                className="hover:bg-gray-50 transition-colors"
              >
                <TableCell>{row.itemId}</TableCell>
                <TableCell>{row.itemName}</TableCell>
                <TableCell>{row.branchCode} - {row.branchLocation}</TableCell>
                <TableCell align="right">{row.quantity}</TableCell>
                <TableCell align="right">{row.reorderLevel}</TableCell>
                <TableCell align="center">
                  {row.isBelowReorderLevel ? (
                    <Chip label="Low Stock" color="error" size="small" className="!font-medium" />
                  ) : (
                    <Chip label="In Stock" color="success" size="small" className="!font-medium" />
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </div>
  )
}