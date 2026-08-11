import { createTheme } from '@mui/material/styles'

export const cargillsTheme = createTheme({
  palette: {
    primary: {
      main: '#D42027',
      dark: '#B01C22',
      light: '#FFF0F0',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#1565C0',
    },
    success: {
      main: '#2E7D32',
    },
    warning: {
      main: '#ED6C02',
    },
    error: {
      main: '#B01C22',
    },
    background: {
      default: '#F0F2F5',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1A1A1A',
      secondary: '#666666',
    },
  },
  typography: {
    fontFamily: "'Inter', 'Segoe UI', sans-serif",
    h4: { fontWeight: 700 },
    h5: { fontWeight: 700 },
    h6: { fontWeight: 600 },
    button: { textTransform: 'none', fontWeight: 600 },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: '10px',
          height: '48px',
          fontSize: '0.95rem',
        },
        containedPrimary: {
          background: 'linear-gradient(135deg, #D42027 0%, #B01C22 100%)',
          '&:hover': {
            background: 'linear-gradient(135deg, #C41D24 0%, #9A181E 100%)',
            boxShadow: '0 6px 20px rgba(212, 32, 39, 0.3)',
          },
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: '10px',
            backgroundColor: '#FAFAFA',
            '& fieldset': { borderColor: '#E0E0E0' },
            '&:hover fieldset': { borderColor: '#D42027' },
            '&.Mui-focused fieldset': { borderColor: '#D42027' },
          },
        },
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          borderRadius: '10px',
        },
      },
    },
  },
})
