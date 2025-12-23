# 📦 StockWise

**Intelligent Product, Order & Warehouse Management System**

StockWise is a modern web application designed to manage **products, inventory, orders, and inter-company offers**.  
It supports **role-based access**, **secure authentication**, **inventory movements**, and **order lifecycle management**.

---

## 🚀 Key Features

### 🏢 Company Products
- Create and edit products  
  *(name, category, price, currency, photo)*
- Inventory level management
- Mark products as available for ordering
- Inline editing in product tables

---

### 📦 Warehouse / Inventory
- Record inventory movements:
  - Inbound
  - Outbound
- Full history of all product movements
- Movement details:
  - Date
  - Quantity
  - Comment
- Dedicated warehouse history panel

---

### 🛒 Market / Offers
Browse products available for order from other companies.

**Filtering**
- Stock availability
- Price range

**Sorting**
- Price
- Condition
- Company name
- Category

**Searching**
- Product name
- Company name
- EAN

**Additional**
- Pagination support

---

### 🧾 Orders
- Create orders based on quotes
- Multiple products per order
- Seller and buyer data validation
- Order status management:
  - Pending
  - Accepted
  - Rejected
  - Canceled
  - Completed

---

### 🔐 Security
- JWT-based authentication
- Role-based authorization:
  - Worker
  - Manager
  - Admin
- Company-level data isolation
- Fully secured API endpoints

---

## 🔎 Project Status

### 🟢 Current Stage

#### Backend
- Full CRUD API:
  - Products
  - Orders
- Order status handling
- Inventory management logic
- Stock synchronization
- User account system
- Role management:
  - Worker
  - Manager
  - Admin
- Authentication & authorization (JWT)
- Clean Architecture:
  - Application
  - Domain
  - Infrastructure
  - API
- Extensive unit & integration testing

#### Frontend
- Product management (CRUD)
- Order management (CRUD)
- Inventory management
- User authentication & authorization
- Account registration
- Email verification (code-based)
- Password reset (code-based)

---

### 🟡 In Progress / Planned
- Manager Panel
- Admin Panel
- Docker support
- GitHub Actions (CI/CD):
  - Build
  - Tests
  - Linting

---

## 🧪 Testing
The project includes **extensive integration and unit testing**, covering:
- API controllers
- Authorization and role validation
- Order status transitions
- Database operations
- Error handling and validation

---

## 💡 Technologies Used

### Backend
- .NET 8 / ASP.NET Core Web API
- Clean Architecture
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- JWT (JSON Web Tokens)
- xUnit
- FluentAssertions
- AutoMapper
- IMemoryCache
- Email Service (Fake / SMTP – test & prod)
- Azure Blob Storage
- SignalR

---

### Frontend
- React
- TypeScript
- Tailwind CSS
- React Hook Form + Yup
- Axios

## Frontend - screenshots 📷
<img width="1842" height="676" alt="CompanyProductList" src="https://github.com/user-attachments/assets/d57acfe7-c5e4-4430-83d6-0c4cba8b63c0" />
<img width="1522" height="133" alt="convert" src="https://github.com/user-attachments/assets/0786e90b-75d9-4c6d-8b0a-625800aa5962" />
<img width="1806" height="838" alt="AvailableList" src="https://github.com/user-attachments/assets/144cb673-8876-475f-bfc8-b345c901bd81" />
<img width="1850" height="520" alt="MovementsList" src="https://github.com/user-attachments/assets/ff2235d6-ea32-422c-9cb6-f40c60e0b6a3" />
<img width="778" height="895" alt="CreateOrderFrom" src="https://github.com/user-attachments/assets/7f9a7290-1bbb-4b5c-9eb4-631929a872da" />
<img width="626" height="581" alt="Add new movement" src="https://github.com/user-attachments/assets/9052672b-853c-4f6d-9d10-cfe518eb1762" />
<img width="1846" height="716" alt="Order Placed" src="https://github.com/user-attachments/assets/c3ee5006-9c63-49db-aeb4-2a013a8639bd" />
<img width="1845" height="791" alt="Order Recived" src="https://github.com/user-attachments/assets/63e6274d-da2d-4e7e-869a-da015141a11d" />
<img width="1848" height="909" alt="Login Page" src="https://github.com/user-attachments/assets/43996487-49db-4693-b486-8dbc9840dad3" />
