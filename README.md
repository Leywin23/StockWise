📦 StockWise
Intelligent product, order and warehouse management

StockWise is a modern web application for managing products, orders, and inventory levels in companies. 
The system supports user roles, order processes, inventory movements, and secure authentication.

🏢 Company products  
  Adding and editing products (name, category, price, currency, photo)
  Inventory management
  Marking products as available for ordering
  Inline editing in a table

📦 Warehouse / Inventory
  Recording warehouse movements (Inbound/Outbound)
  History of all product movements
  Adding movements with date, quantity, and comment
  Dedicated warehouse history panel

🛒 Market / Offers  
  View products available for order from other companies
  
  Filter by:
  -stock
  -price

  Sort by:
  -price
  -condition
  -company name
  -category

  Search by:
  -product name
  -company name
  -EAN
  
  Paginacja

🧾 Orders
  Creating orders based on quotes
  Managing multiple products in a single order
  Validating seller and buyer data
  Order statuses

🔐Security
  JWT Authorization
  User Roles (Manager / Worker)
  Access to company data only
  Secured API Endpoints

🔎 Project Status

  🟢 Current stage:
    Backend:
      Full CRUD API for products
      Full CRUD API for orders
      Order status handling:
        Pending / Accepted / Rejected / Canceled / Completed
      Inventory management logic
      Stock synchronization
      User account system
      User roles:
        -Worker
        -Manager
        -Admin
      Authorization and authentication (JWT)
      Extensive integration and unit testing
      Clean Architecture
    Frontend:
      Product CRUD
      Order CRUD
      Inventory Management
      User Authorization
      Account Registration
      Email Verification (code)
      Password Reset (code)
  
  🟡 In progress/planned
    Manager Panel
    Admin Panel
    Docker
    GitHub Actions (CI/CD)
      -build
      -tests
      -lint
    
🧪 Tests
The project includes extensive integration testing, including:
  API controllers
  authorization and roles
  order statuses
  database operations
  error validation

💡 Technologies used
  Backend
    .NET 8 / ASP.NET Core Web API
    Clean Architecture
    Application / Domain / Infrastructure / API
    Entity Framework Core
    Microsoft SQL Server
    ASP.NET Core Identity
    JWT (JSON Web Tokens)
    xUnit
    FluentAssertions
    AutoMapper
    In-memory cache (IMemoryCache)
    Email Service (Fake / SMTP – test/prod)
    Azure Blob Storage
    SignalR
    
  Frontend
    React
    TypeScript
    Tailwind CSS
    React Hook Form + Yup
    Axios

  
