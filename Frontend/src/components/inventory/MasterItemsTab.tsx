import { useState, useMemo, useEffect } from 'react';
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

interface MasterItemsTabProps {
  masterItems: any[];
  allBranchStock: any[];
  onOpenEdit: (row: any) => void;
  onDelete: (id: number) => void;
}

interface BranchStockTableProps {
  branchName: string;
  items: any[];
}

function BranchStockTable({ branchName, items }: BranchStockTableProps) {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);

  useEffect(() => {
    setPage(0);
  }, [items]);

  const paginatedItems = items.slice(
    page * rowsPerPage,
    page * rowsPerPage + rowsPerPage
  );

  return (
    <Box className="mb-6">
      <Typography variant="subtitle1" className="!font-bold !text-gray-700 mb-3 bg-gray-100 p-2 rounded-t-lg border border-b-0 border-gray-200">
        {branchName}
      </Typography>
      <TableContainer component={Paper} className="shadow-sm overflow-hidden border border-gray-200 rounded-b-lg rounded-t-none">
        <Table sx={{ minWidth: 650 }}>
          <TableHead className="bg-gray-50">
            <TableRow>
              <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
              <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Quantity</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Reorder Level</TableCell>
              <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center" className="py-6 text-gray-500 italic">
                  No stock items for this branch.
                </TableCell>
              </TableRow>
            ) : (
              paginatedItems.map((row: any) => (
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
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        <TablePagination
          rowsPerPageOptions={[5, 10, 25]}
          component="div"
          count={items.length}
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

export default function MasterItemsTab({ masterItems, allBranchStock, onOpenEdit, onDelete }: MasterItemsTabProps) {
  const [masterFilter, setMasterFilter] = useState('All');
  const [masterSearchTerm, setMasterSearchTerm] = useState('');
  const [masterPage, setMasterPage] = useState(0);
  const [masterRowsPerPage, setMasterRowsPerPage] = useState(10);

  const [allBranchFilter, setAllBranchFilter] = useState('All');
  const [allBranchSearchTerm, setAllBranchSearchTerm] = useState('');

  const filteredMasterItems = masterItems.filter(item => {
    const matchesSearch = item.itemName?.toLowerCase().includes(masterSearchTerm.toLowerCase()) ||
      item.supplierName?.toLowerCase().includes(masterSearchTerm.toLowerCase()) ||
      item.itemId?.toString().includes(masterSearchTerm);
    const matchesFilter = masterFilter === 'All' ? true :
      masterFilter === 'Active' ? item.isActive :
        !item.isActive;
    return matchesSearch && matchesFilter;
  });

  useEffect(() => {
    setMasterPage(0);
  }, [masterFilter, masterSearchTerm]);

  const paginatedMasterItems = filteredMasterItems.slice(
    masterPage * masterRowsPerPage,
    masterPage * masterRowsPerPage + masterRowsPerPage
  );

  const filteredAllBranchStock = allBranchStock.filter(item => {
    const matchesSearch = item.itemName?.toLowerCase().includes(allBranchSearchTerm.toLowerCase()) ||
      item.branchCode?.toLowerCase().includes(allBranchSearchTerm.toLowerCase()) ||
      item.branchLocation?.toLowerCase().includes(allBranchSearchTerm.toLowerCase()) ||
      item.itemId?.toString().includes(allBranchSearchTerm);
    const matchesFilter = allBranchFilter === 'All' ? true :
      allBranchFilter === 'Low Stock' ? item.isBelowReorderLevel :
        !item.isBelowReorderLevel;
    return matchesSearch && matchesFilter;
  });

  const groupedBranchStock = useMemo(() => {
    return filteredAllBranchStock.reduce((acc, item) => {
      const branchKey = `${item.branchCode || 'Unknown'} - ${item.branchLocation || 'Unknown'}`;
      if (!acc[branchKey]) {
        acc[branchKey] = [];
      }
      acc[branchKey].push(item);
      return acc;
    }, {} as Record<string, any[]>);
  }, [filteredAllBranchStock]);

  return (
    <Box>
      <Box className="flex justify-end gap-4 mb-4">
        <FormControl size="small" className="w-48 bg-white rounded-md">
          <InputLabel id="master-filter-label">Filter Status</InputLabel>
          <Select
            labelId="master-filter-label"
            value={masterFilter}
            label="Filter Status"
            onChange={(e) => setMasterFilter(e.target.value)}
          >
            <MenuItem value="All">All Statuses</MenuItem>
            <MenuItem value="Active">Active</MenuItem>
            <MenuItem value="Inactive">Inactive</MenuItem>
          </Select>
        </FormControl>
        <TextField
          size="small"
          placeholder="Search by Item Name, ID or Supplier..."
          variant="outlined"
          value={masterSearchTerm}
          onChange={(e) => setMasterSearchTerm(e.target.value)}
          className="w-72 bg-white rounded-md"
        />
      </Box>
      <TableContainer component={Paper} className="rounded-xl shadow-sm overflow-hidden">
        <Table sx={{ minWidth: 650 }}>
          <TableHead className="bg-gray-50">
            <TableRow>
              <TableCell className="!font-bold !text-gray-700">Item ID</TableCell>
              <TableCell className="!font-bold !text-gray-700">Item Name</TableCell>
              <TableCell className="!font-bold !text-gray-700">Supplier</TableCell>
              <TableCell align="right" className="!font-bold !text-gray-700">Unit Price (LKR)</TableCell>
              <TableCell align="center" className="!font-bold !text-gray-700">Status</TableCell>
              <TableCell align="center" className="!font-bold !text-gray-700">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredMasterItems.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" className="py-6 text-gray-500 italic">
                  No master items found.
                </TableCell>
              </TableRow>
            ) : (
              paginatedMasterItems.map((row) => (
                <TableRow key={row.itemId} sx={{ '&:last-child td, &:last-child th': { border: 0 } }} className="hover:bg-gray-50 transition-colors">
                  <TableCell>{row.itemId}</TableCell>
                  <TableCell className="!font-medium">{row.itemName}</TableCell>
                  <TableCell>{row.supplierName}</TableCell>
                  <TableCell align="right">{row.unitPrice.toFixed(2)}</TableCell>
                  <TableCell align="center">
                    {row.isActive ? (
                      <Chip label="Active" color="success" size="small" className="!font-medium" />
                    ) : (
                      <Chip label="Inactive" color="default" size="small" className="!font-medium" />
                    )}
                  </TableCell>
                  <TableCell align="center">
                    <IconButton size="small" color="primary" onClick={() => onOpenEdit(row)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="error" onClick={() => onDelete(row.itemId)}>
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
          count={filteredMasterItems.length}
          rowsPerPage={masterRowsPerPage}
          page={masterPage}
          onPageChange={(_, newPage) => setMasterPage(newPage)}
          onRowsPerPageChange={(e) => {
            setMasterRowsPerPage(parseInt(e.target.value, 10));
            setMasterPage(0);
          }}
        />
      </TableContainer>

      <Box className="mt-10">
        <Box className="flex justify-between items-center mb-4">
          <Typography variant="h6" className="!font-bold !text-gray-800">
            All Branch Stock
          </Typography>
          <Box className="flex gap-4">
            <FormControl size="small" className="w-48 bg-white rounded-md">
              <InputLabel id="all-branch-filter-label">Filter Status</InputLabel>
              <Select
                labelId="all-branch-filter-label"
                value={allBranchFilter}
                label="Filter Status"
                onChange={(e) => setAllBranchFilter(e.target.value)}
              >
                <MenuItem value="All">All Statuses</MenuItem>
                <MenuItem value="In Stock">In Stock</MenuItem>
                <MenuItem value="Low Stock">Low Stock</MenuItem>
              </Select>
            </FormControl>
            <TextField
              size="small"
              placeholder="Search by Item Name, ID, or Branch..."
              variant="outlined"
              value={allBranchSearchTerm}
              onChange={(e) => setAllBranchSearchTerm(e.target.value)}
              className="w-72 bg-white rounded-md"
            />
          </Box>
        </Box>
        {Object.keys(groupedBranchStock).length === 0 ? (
          <Typography className="text-gray-500 italic py-4 text-center border rounded-lg bg-gray-50">
            No stock found for selected filters.
          </Typography>
        ) : (
          Object.entries(groupedBranchStock).map(([branchName, items]) => (
            <BranchStockTable key={branchName} branchName={branchName} items={items as any} />
          ))
        )}
      </Box>
    </Box>
  );
}
