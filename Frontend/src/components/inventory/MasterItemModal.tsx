import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  TextField,
  Button
} from '@mui/material';

interface MasterItemModalProps {
  open: boolean;
  onClose: () => void;
  onSave: () => void;
  editingId: number | null;
  itemName: string;
  setItemName: (val: string) => void;
  unitPrice: number | string;
  setUnitPrice: (val: string) => void;
  supplierId: number | string;
  setSupplierId: (val: string) => void;
}

export default function MasterItemModal({
  open,
  onClose,
  onSave,
  editingId,
  itemName,
  setItemName,
  unitPrice,
  setUnitPrice,
  supplierId,
  setSupplierId
}: MasterItemModalProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle className="!font-bold !text-[#1A1A1A]">
        {editingId ? "Edit Master Item" : "Create Master Item"}
      </DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={3} className="pt-2">
          <Grid item xs={12}>
            <TextField 
              label="Item Name" 
              variant="outlined" 
              fullWidth 
              size="small" 
              value={itemName}
              onChange={(e) => setItemName(e.target.value)}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField 
              label="Unit Price" 
              type="number" 
              variant="outlined" 
              fullWidth 
              size="small" 
              value={unitPrice}
              onChange={(e) => setUnitPrice(e.target.value)}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField 
              label="Supplier ID" 
              type="number" 
              variant="outlined" 
              fullWidth 
              size="small" 
              value={supplierId}
              onChange={(e) => setSupplierId(e.target.value)}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions className="p-4">
        <Button onClick={onClose} color="inherit" className="!normal-case !font-medium">Cancel</Button>
        <Button onClick={onSave} variant="contained" className="!bg-[#E21E26] hover:!bg-[#C61A22] !shadow-none !normal-case !font-medium">
          {editingId ? "Update Item" : "Create Item"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
