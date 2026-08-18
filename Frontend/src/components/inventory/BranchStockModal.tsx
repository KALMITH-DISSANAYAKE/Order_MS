import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';

interface BranchStockModalProps {
  open: boolean;
  onClose: () => void;
  onSave: () => void;
  editingId: number | null;
  itemId: number | string;
  setItemId: (val: string) => void;
  itemName?: string;
  masterItems?: any[];
  quantity: number | string;
  setQuantity: (val: string) => void;
  reorderLevel: number | string;
  setReorderLevel: (val: string) => void;
}

export default function BranchStockModal({
  open,
  onClose,
  onSave,
  editingId,
  itemId,
  setItemId,
  itemName = '',
  masterItems = [],
  quantity,
  setQuantity,
  reorderLevel,
  setReorderLevel
}: BranchStockModalProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle className="!font-bold !text-[#1A1A1A]">
        {editingId ? "Edit Branch Stock" : "Add Branch Stock"}
      </DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={3} className="pt-2">
          <Grid item xs={12}>
            {editingId ? (
              <TextField
                label="Item Name"
                variant="outlined"
                fullWidth
                size="small"
                disabled
                value={itemName || (itemId ? `Item #${itemId}` : '')}
              />
            ) : (
              <FormControl fullWidth size="small">
                <InputLabel id="select-item-label">Select Master Item</InputLabel>
                <Select
                  labelId="select-item-label"
                  value={itemId ? String(itemId) : ''}
                  label="Select Master Item"
                  onChange={(e) => setItemId(e.target.value)}
                >
                  <MenuItem value="" disabled>
                    <em>Select an item</em>
                  </MenuItem>
                  {masterItems && masterItems.length > 0 ? (
                    masterItems.map((item) => (
                      <MenuItem key={item.itemId} value={String(item.itemId)}>
                        {item.itemName} (ID: {item.itemId})
                      </MenuItem>
                    ))
                  ) : (
                    <MenuItem value="" disabled>
                      No items available
                    </MenuItem>
                  )}
                </Select>
              </FormControl>
            )}
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              label="Quantity"
              type="number"
              variant="outlined"
              fullWidth
              size="small"
              value={quantity}
              onChange={(e) => setQuantity(e.target.value)}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              label="Reorder Level"
              type="number"
              variant="outlined"
              fullWidth
              size="small"
              value={reorderLevel}
              onChange={(e) => setReorderLevel(e.target.value)}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions className="p-4">
        <Button onClick={onClose} color="inherit" className="!normal-case !font-medium">Cancel</Button>
        <Button onClick={onSave} variant="contained" className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium">Save Stock</Button>
      </DialogActions>
    </Dialog>
  );
}
