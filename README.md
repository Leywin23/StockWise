📦 StockWise

Intelligent Product, Order & Warehouse Management System

StockWise is a modern web application designed to manage products, inventory, orders, and inter-company offers.
It supports role-based access, secure authentication, inventory movements, and order lifecycle management.

🚀 Key Features
🏢 Company Products

Create and edit products
(name, category, price, currency, photo)

Inventory level management

Mark products as available for ordering

Inline editing in product tables

📦 Warehouse / Inventory

Record inventory movements:

Inbound

Outbound

Full history of all product movements

Movement details:

Date

Quantity

Comment

Dedicated warehouse history panel

🛒 Market / Offers

Browse products available for order from other companies.

Filtering

Stock availability

Price range

Sorting

Price

Condition

Company name

Category

Searching

Product name

Company name

EAN

Additional

Pagination support

🧾 Orders

Create orders based on quotes

Multiple products per order

Seller and buyer data validation

Order status management:

Pending

Accepted

Rejected

Canceled

Completed

🔐 Security

JWT-based authentication

Role-based authorization:

Worker

Manager

Admin

Company-level data isolation

Fully secured API endpoints

🔎 Project Status
🟢 Current Stage
Backend

Full CRUD API:

Products

Orders

Order status handling

Inventory management logic

Stock synchronization

User account system

Role management:

Worker

Manager

Admin

Authentication & authorization (JWT)

Clean Architecture:

Application

Domain

Infrastructure

API

Extensive unit & integration testing

Frontend

Product management (CRUD)

Order management (CRUD)

Inventory management

User authentication & authorization

Account registration

Email verification (code-based)

Password reset (code-based)

🟡 In Progress / Planned

Manager Panel

Admin Panel

Docker support

GitHub Actions (CI/CD):

Build

Tests

Linting

🧪 Testing

The project includes extensive integration and unit testing, covering:

API controllers

Authorization and role validation

Order status transitions

Database operations

Error handling and validation

💡 Technologies Used
Backend

.NET 8 / ASP.NET Core Web API

Clean Architecture

Entity Framework Core

Microsoft SQL Server

ASP.NET Core Identity

JWT (JSON Web Tokens)

xUnit

FluentAssertions

AutoMapper

IMemoryCache

Email Service (Fake / SMTP – test & prod)

Azure Blob Storage

SignalR

Frontend

React

TypeScript

Tailwind CSS

React Hook Form + Yup

Axios
