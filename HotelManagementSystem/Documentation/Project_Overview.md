Project Overview — HotelManagementSystem

This document summarizes the project structure, each source file's purpose, notable issues, and a recommended file to study in depth. Save or open this file in your IDE for offline review.

Overall project type
- ASP.NET Core web app using MVC controllers + Views and Entity Framework Core (EF Core).
- Dependency injection for services, EF Core DbContext for data access, and static assets under `wwwroot`.

Key files and what they do

- `Program.cs`
  - App startup: registers `ApplicationDbContext` with SQL Server using `DefaultConnection`, registers scoped services (`IRoomService`, `IReservationService`, `IBillingService`, `IHousekeepingService`, `IReportService`), enables static files and routing, and sets the default controller route to `Report/Index`.

- `Data/ApplicationDbContext.cs`
  - EF Core `DbContext` declaring `DbSet<T>` for domain entities: `Rooms`, `Guests`, `Reservations`, `Invoices`, `HousekeepingTasks`, `OccupancyReports`.

Models
- `Models/Room.cs` — `RoomId`, `RoomNumber`, `RoomType`, `RatePerNight`, `Status`. Uses `[Key]` and decimal column annotation.
- `Models/Guest.cs` — `GuestId`, `Name`, `Email`, `ContactInfo`.
- `Models/Reservation.cs` — `ReservationId`, `GuestId`, `RoomId`, `CheckInDate`, `CheckOutDate`, `ReservationStatus`.
- `Models/Invoice.cs` — `InvoiceId`, `ReservationId`, `InvoiceDate`, `TotalAmount`, `PaymentStatus`.
- `Models/HousekeepingTask.cs` — `TaskId`, `RoomId`, `AssignedStaffId`, `TaskDate`, `TaskStatus`.
- `Models/OccupancyReport.cs` — `ReportId`, `ReportDate`, `TotalRooms`, `OccupiedRooms`, `RevenueGenerated`.

Services (business logic)
- `Services/RoomService.cs` (`IRoomService`) — list rooms, available rooms, add a room (sets `Status = "AVAILABLE"`).
- `Services/ReservationService.cs` (`IReservationService`) — `CreateReservation`, `CancelReservation`, `GetAllReservations`.
  - Overlap detection to prevent double-booking (LINQ `Any`).
  - Guest lookup or creation by email.
  - Sets `ReservationStatus = "BOOKED"` when saving.
- `Services/BillingService.cs` (`IBillingService`) — `GetPaidInvoices`, `GetActiveBookings`, `CheckInGuest`, `ProcessCheckout`.
  - `GetPaidInvoices` and `GetActiveBookings` use joins to project view records.
  - `ProcessCheckout` creates an `Invoice` (idempotent check), computes nights, marks `PaymentStatus = "PAID"`, and adds a housekeeping task.
- `Services/HousekeepingService.cs` (`IHousekeepingService`) — `GetPendingTasks`, `MarkClean` (marks task completed and room `AVAILABLE`).
- `Services/ReportService.cs` (`IReportService`) — `GetMetrics()` builds `DashboardViewModel` from aggregates: total revenue, pending tasks, room counts by status.

Controllers
- `Controllers/ReportController.cs` — injects `IReportService`, returns `View(dashboardData)` for `Index`.
- `Controllers/BillingController.cs` — injects `IBillingService` and `IReservationService`, exposes `Index` (view of invoices + active bookings), `CheckIn` (POST), `Cancel` (POST), `Checkout` (POST returns `Receipt`).

View models
- `ViewModels/DashboardViewModel.cs` — `TotalRevenue`, `PendingTasks`, `RoomsAvailable`, `RoomsOccupied`, `RoomsMaintenance`.
- `ViewModels/BillingViewModels.cs` — currently empty; the code references `FinancialRecord` and `BookingRecord` but they are not defined. Add these classes here to match `BillingService` projections.

Migrations
- `Migrations/20260325041944_InitialCreate.cs` — creates tables for Guests, HousekeepingTasks, Invoices, OccupancyReports, Reservations, Rooms.
- `Migrations/ApplicationDbContextModelSnapshot.cs` — EF Core model snapshot used by migrations.

Configuration & static assets
- `appsettings.json` and `appsettings.Development.json` — application configuration (DB connection string expected under `DefaultConnection`).
- `wwwroot/lib/...` — Bootstrap and jQuery validation static assets.

Notable issues and recommendations
- Missing view models: `FinancialRecord` and `BookingRecord` are referenced by `BillingService` but not defined. Define them in `ViewModels/BillingViewModels.cs`.
- Lack of navigation properties and explicit foreign keys in models. Consider adding navigation properties to enable EF relationships and include queries.
- Concurrency and transaction safety: operations like `ProcessCheckout` (create invoice + add housekeeping task) should be transactionally safe to avoid partial writes.
- Validation and error handling are minimal. Add input validation, model validation, and appropriate error responses.

Recommended file to study in depth
- `Services/ReservationService.cs`
  - Contains overlap detection logic, guest creation/lookup, status transitions, and SaveChanges calls — central to booking correctness and where race conditions are most likely.
  - Study for: correctness of overlap detection, potential race conditions, guest identity by email, and where to add transactions or locks.

Next steps (I can perform these for you)
- Add `FinancialRecord` and `BookingRecord` classes into `ViewModels/BillingViewModels.cs` so the project compiles.
- Create example Views for `Report/Index` and `Billing/Index`.
- Convert this markdown to PDF in the repo if you prefer a PDF document.

File location
- Created file: `HotelManagementSystem/Documentation/Project_Overview.md`

How to download
- In Visual Studio: open the file and use File > Save As to copy elsewhere, or right-click the repo file in Solution Explorer and choose "Open Containing Folder" then copy the file.
- From the git repository: commit and push, then download from GitHub web UI.

If you want a PDF instead, tell me and I will generate `HotelManagementSystem/Documentation/Project_Overview.pdf` as well.