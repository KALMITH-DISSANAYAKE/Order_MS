# Cargills Food City - Order Management Frontend

React + TypeScript + Vite + Tailwind CSS + Material UI

## Setup

```bash
cd cargills-frontend
npm install
npm run dev
```

Open http://localhost:5173

## Demo Login
- Username: `admin`
- Password: `admin`

Or click any demo account chip on the login page.

## Tech Stack
- **React 18** + TypeScript
- **Vite** (build tool)
- **Tailwind CSS** (layout + spacing)
- **Material UI (MUI)** (components + theme)
- **React Router DOM** (navigation)
- **Axios** (API calls — backend connection ready)

## Folder Structure
```
src/
├── api/              # Axios instance (backend ready)
├── components/
│   ├── common/       # Reusable pieces (PageHeader, etc.)
│   └── layout/       # DashboardLayout (sidebar + topbar)
├── contexts/         # AuthContext (global auth state)
├── pages/
│   ├── auth/         # Login + Register
│   ├── dashboard/    # Dashboard home
│   ├── inventory/    # Member 2
│   ├── order-requests/ # Member 3
│   ├── orders/       # Member 4
│   ├── transport/    # Member 4
│   ├── delivery/     # Member 5
│   ├── users/        # YOU (Member 1)
│   └── branches/     # YOU (Member 1)
├── theme/            # MUI Cargills theme
└── types/            # Shared TypeScript interfaces
```

## Your Next Steps (Member 1)
1. Build `src/pages/users/UsersPage.tsx`
2. Build `src/pages/branches/BranchesPage.tsx`
3. Connect to backend: update `src/api/axiosInstance.ts` baseURL
