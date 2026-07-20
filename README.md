# Sales Management System

A comprehensive **Sales Management System** developed with **ASP.NET Core Web API (.NET 10)** as part of my software development internship at **DBHSoft**.

The project provides a secure and scalable RESTful API for managing products, categories, customers, orders, users and inventory while demonstrating modern backend development practices such as JWT authentication, role-based authorization, caching, background jobs, Excel import/export, logging and global exception handling.

---

# Project Overview

The main goal of this project is to build a real-world Sales Management System by applying modern backend development concepts using Microsoft's technology stack.

The project focuses on:

* Clean layered architecture
* RESTful API development
* SQL Server database design
* Entity Framework Core
* Authentication & Authorization
* Background processing
* Logging & Exception Handling
* Excel Import/Export
* API Documentation

---

# Features

## Authentication & Authorization

* JWT Authentication
* Refresh Token Authentication
* BCrypt Password Hashing
* Role-Based Authorization (Admin/User)
* Account Lockout after multiple failed login attempts
* Login Audit Logging
* Email Domain Validation
* Welcome Email Service (Resend Integration)
* Rate Limiting for Login Endpoint

---

## Product Management

* Product CRUD Operations
* Product Search
* Pagination
* Filtering
* Sorting
* Category Relationship
* Memory Cache Support

---

## Category Management

* Category CRUD Operations

---

## Customer Management

* Customer CRUD Operations

---

## Order Management

* Order CRUD Operations
* Order Details Management

---

## Inventory Management

* Stock Tracking
* Low Stock Detection
* Daily Stock Report using Hangfire
* Email Notification for Low Stock Products

---

## Excel Import / Export

### Excel Import

* Import products from Excel (.xlsx)
* Multiple validation checks
* Duplicate detection
* Invalid category detection
* Missing field detection
* Negative stock validation
* Invalid price validation
* Automatic Import Error Excel generation

### Excel Export

* Export all products to Excel
* Ready for reporting and backup

---

## API Documentation

* Swagger UI
* XML Documentation
* Endpoint descriptions
* Request/Response documentation

---

## Logging

* Request Logging Middleware
* Login Audit Logging
* Email Failure Logging

---

## Exception Handling

* Global Exception Middleware
* Custom AppException
* Standardized Error Responses

---

## Performance

* Memory Cache
* Efficient Pagination
* Optimized Product Queries

---

# Technologies

## Backend

* C#
* ASP.NET Core Web API (.NET 10)

## Database

* Microsoft SQL Server
* Entity Framework Core

## Authentication

* JWT Bearer Authentication
* BCrypt.Net

## Background Jobs

* Hangfire

## Email

* Resend API

## Excel

* ClosedXML
* CsvHelper

## Documentation

* Swagger (OpenAPI)
* XML Comments

## Version Control

* Git
* GitHub

---

# Project Architecture

The project follows a layered architecture.

```
SalesManagementSystem
│
├── SalesManagementSystem.API
│
├── SalesManagementSystem.Data
│
├── SalesManagementSystem.Models
│
└── Database
```

### API Layer

Responsible for

* Controllers
* Middleware
* Authentication
* DTOs
* Services
* Swagger Configuration

---

### Data Layer

Responsible for

* Entity Framework Core
* DbContext
* Database Access
* Migrations

---

### Models Layer

Contains

* Entity Models
* Domain Objects

---

# Database

Current database includes:

* Users
* Customers
* Products
* Categories
* Orders
* OrderDetails
* LoginAudits

---

# Security

Implemented security features:

* JWT Authentication
* Refresh Tokens
* Password Hashing
* Login Rate Limiting
* Account Lockout
* Role-Based Authorization
* Login Audit
* Email Domain Validation
* Global Exception Handling

---

# API Highlights

Some of the implemented endpoints include:

## Authentication

* Register
* Login
* Refresh Token

## Products

* Get Products
* Get Product By Id
* Create Product
* Update Product
* Delete Product
* Import Products (Excel)
* Export Products (Excel)

## Categories

* CRUD Operations

## Customers

* CRUD Operations

## Orders

* CRUD Operations

---

# Background Jobs

The project uses **Hangfire** for scheduled background processing.

Implemented job:

* Daily Low Stock Report
* Automatic Email Notification

---

# Installation

## Clone Repository

```bash
git clone https://github.com/EmirhanKaradeniz10/SalesManagementSystem.git
```

---

## Restore Packages

```bash
dotnet restore
```

---

## Update Database

```bash
dotnet ef database update
```

---

## Run Project

```bash
dotnet run
```

Swagger:

```
https://localhost:{PORT}/swagger
```

---

# Future Improvements

Possible future enhancements include:

* Password Reset
* Email Verification
* Coupon System
* Docker Support
* Azure Deployment
* Unit Testing
* Redis Cache
* CI/CD Pipeline

---

# Screenshots

The following screenshots will be added after deployment.

* Swagger UI
* Hangfire Dashboard
* Login
* Product Import
* Product Export

---

# License

This project was developed for educational and internship purposes.
