import axios from 'axios';

const axiosInstance = axios.create({

  baseURL: 'http://localhost:5076/api',

  headers: { 'Content-Type': 'application/json' },
});

axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  console.log('Sending token:', token);
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const url = error.config?.url || '';

    // ← KEY FIX: Don't redirect when the 401 comes from the LOGIN request.
    // That 401 means "wrong credentials", not "expired session".
    const isLoginRequest = url.includes('/auth/login');

    if (status === 401 && !isLoginRequest) {
      // Real session expiry — clear storage and redirect
      localStorage.removeItem('cargills_auth');
      localStorage.removeItem('token');
      sessionStorage.removeItem('cargills_auth_session');
      sessionStorage.removeItem('token');
      window.location.href = '/login';
    }

    // Always reject so the component's catch block can show the error message
    return Promise.reject(error);
  }
);

export default axiosInstance;