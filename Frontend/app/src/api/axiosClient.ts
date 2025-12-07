// apiClient.ts
import axios from "axios";

const apiClient = axios.create({
  baseURL: "https://localhost:7178/api",
  withCredentials: true,
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token"); // lub sessionStorage – patrz punkt 2
  if (token) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ⬇⬇⬇ NOWY INTERCEPTOR ODPOWIEDZI
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // token nieaktualny / brak autoryzacji -> robimy twardy logout
      localStorage.removeItem("token");
      localStorage.removeItem("user");

      // jeżeli już jesteśmy na /login, nie przekierowuj ponownie
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
