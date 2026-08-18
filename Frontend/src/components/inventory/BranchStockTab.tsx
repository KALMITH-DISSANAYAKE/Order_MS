import { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  TableContainer,
  Paper,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  IconButton,
  TablePagination
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

interface BranchStockTabProps {
  inventory: any[];
  onOpenEdit: (row: any) => void;
  onDelete: (id: number) => void;
}

export default function BranchStockTab({ inventory, onOpenEdit, onDelete }: BranchStockTabProps) {
  const [branchFilter, setBranchFilter] = useState('All');
  const [branchSearchTerm, setBranchSearchTerm] = useState('');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const filteredBranchStock = inventory.filter(item => {
    const matchesSearch = item.itemName?.toLowerCase().includes(branchSearchTerm.toLowerCase()) ||
      item.branchCode?.toLowerCase().includes(branchSearchTerm.toLowerCase()) ||
      item.itemId?.toString().includes(branchSearchTerm);
    const matchesFilter = branchFilter === 'All' ? true :
      branchFilter === 'Low Stock' ? item.isBelowReorderLevel :
        !item.isBelowReorderLevel;
    return matchesSearch && matchesFilter;
  });

  useEffect(() => {
    setPage(0);
  }, [branchFilter, branchSearchTerm]);

  const paginatedBranchStock = filteredBranchStock.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );

  return (
    <Box>
      <Box className="flex justify-between items-center mb-4">
        <Typography variant="h6" className="!font-bold !text-gray-800">
          {inventory.length > 0 ? `${inventory[0].branchLocation} (${inventory[0].branchCode})` : 'Branch Stock'}
        </Typography>
        <Box className="flex gap-4">
          <FormControl size="small" className="w-48 bg-white rounded-md">
            <InputLabel id="branch-filter-label">Filter Status</InputLabel>
            <Select
              labelId="branch-filter-label"
              value={branchFilter}
              label="Filter Status"
              onChange={(e) => setBranchFilter(e.target.value)}
            >
              <MenuItem value="All">All Statuses</MenuItem>
              <MenuItem value="In Stock">In Stock</MenuItem>
              <MenuItem value="Low Stock">Low Stock</MenuItem>
            </Select>
          </FormControl>
          <TextField
            size="small"
            placeholder="Search by Item Name, ID or Branch..."
            variant="outlined"
            value={branchSearchTerm}
            onChange={(e) => setBranchSearchTerm(e.target.value)}
            className="w-72 bg-white rounded-md"
          />
        </Box>
      </Box>
      <TableContainer component={Paper} className="rounded-xl shadow-sm overflow-hidden">
        <Table sx={{ minWidth: 650 }}>
          <TableHead className="bg-gray-50">
            <TableRow>
              <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
              <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Reorder Level</TableCell>
              <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
              <TableCell align="center" className="!font-bold !text-gray-700">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredBranchStock.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" className="py-6 text-gray-500 italic">
                  No branch stock items found.
                </TableCell>
              </TableRow>
            ) : (
              paginatedBranchStock.map((row) => (
                <TableRow key={row.inventoryId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                  <TableCell>{row.itemId}</TableCell>
                  <TableCell>{row.itemName}</TableCell>
                  <TableCell align="right">{row.quantity}</TableCell>
                  <TableCell align="right">{row.reorderLevel}</TableCell>
                  <TableCell align="center">
                    {row.isBelowReorderLevel ? (
                      <Chip label="Low Stock" color="error" size="small" className="!font-medium" />
                    ) : (
                      <Chip label="In Stock" color="success" size="small" className="!font-medium" />
                    )}
                  </TableCell>
                  <TableCell align="center">
                    <IconButton size="small" color="primary" onClick={() => onOpenEdit(row)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="error" onClick={() => onDelete(row.inventoryId)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        <TablePagination
          rowsPerPageOptions={[5, 10, 25, 50]}
          component="div"
          count={filteredBranchStock.length}
          rowsPerPage={rowsPerPage}
          page={page}
          onPageChange={(_, newPage) => setPage(newPage)}
          onRowsPerPageChange={(e) => {
            setRowsPerPage(parseInt(e.target.value, 10));
            setPage(0);
          }}
        />
      </TableContainer>
    </Box>
  );
}
