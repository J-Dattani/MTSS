# Multi-Tenant Society Suite

Multi-Tenant Society Suite is a cloud-based, end-to-end housing society management platform built with ASP.NET Core MVC and SQL Server. It helps societies manage maintenance billing, online payments, complaints, visitor logs, parking, facility bookings, accounting, audits, and resident communication in one secure shared system.

## Features

- Society-wise tenant separation
- Maintenance billing and invoice tracking
- Complaint and grievance tracking
- Visitor management
- Parking allocation and monitoring
- Facility booking management
- Resident and committee dashboards
- Role-based access control
- Reports, notices, and announcements
- Multi-society support in one application

## Tech Stack

- ASP.NET Core MVC
- C#
- SSMS
- Entity Framework Core
- Visual Studio 2022
- Bootstrap
- HTML, CSS, JavaScript

## Project Structure

- `Controllers/` - application controllers
- `Models/` - business entities and view models
- `Views/` - Razor views
- `wwwroot/` - static files such as CSS, JS, and images
- `Data/` - DbContext and database configuration
- `appsettings.json` - application settings and connection strings
- `Program.cs` - application startup configuration

## Prerequisites

- Visual Studio 2022
- .NET 8 SDK or later
- SQL Server
- SQL Server Management Studio (SSMS)

## Getting Started

1. Clone the repository.
2. Open the solution in Visual Studio 2022.
3. Update the SQL Server connection string in `appsettings.json`.
4. Run Entity Framework migrations to create the database.
5. Build and run the application.

## Basic Workflow

- Create a society profile.
- Add flats and residents.
- Generate maintenance bills.
- Track complaints and visitor entries.
- Manage parking and bookings.
- View reports and dashboards.

## Future Enhancements

- SMS and WhatsApp notifications
- Mobile app integration
- Online payment gateway integration
- Document storage for society records
- Smart analytics and reporting
- API-based resident portal
- PDF export for invoices and reports

## License

This project is currently for internal or private use. Add a license if you plan to publish it.

## Author

Built for a modern multi-tenant housing society management solution.