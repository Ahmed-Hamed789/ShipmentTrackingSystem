# ShipmentTrackingSystem

Web application to create, assign, and track shipments with real-time location updates.

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB on Windows is fine)
- A Google Maps JavaScript API key

## Configuration
Edit `appsettings.json`:
- `ConnectionStrings:DefaultConnection` → SQL Server connection (LocalDB by default)
- `GoogleMaps:ApiKey` → your Maps JavaScript API key
- `Admin:Username` / `Admin:Password` → admin credentials (defaults: `admin` / `admin123`)

Notes:
- Database schema is managed by EF Core migrations and is applied automatically at startup.
- HTTPS is enabled by default.

## Run

### Visual Studio 2022
1. Open the project: __File > Open > Project/Solution__ and select `ShipmentTrackingSystem.csproj` (or open the folder).
2. Set configuration to Debug.
3. Start the app: __Debug > Start Debugging__ (F5).
4. Use the URL shown in the ASP.NET Core debug output.

### .NET CLI
dotnet restore 
dotnet build 
dotnet run

Open the URL displayed in the console (e.g., `https://localhost:5xxx`).

## Accounts and URLs
- Admin portal:
  - Login: `/Admin/Login`
  - Default: `admin` / `admin123`
  - Shipments dashboard: `/Shipments`
  - Drivers management: `/Drivers`
- User portal:
  - Sign up: `/Users/Register`
  - Login: `/Users/Login`
  - My shipments: `/Users/MyShipments`
- Public tracking:
  - Search: `/Tracking/Find`
  - Short link: `/t/{TRACKING}` (e.g., `/t/EG123456`)

## Main Features
- Admin operations
  - Create shipments with origin/destination coordinates and addresses
  - Assign/change driver (when allowed), cancel, or delete shipments (with rules)
  - Manage drivers (National ID, phone, active status)
- User portal
  - Registration and login
  - View “My Shipments” with status and driver info
- Real-time tracking
  - SignalR broadcasts shipment updates to clients subscribed by tracking number
  - Public tracking page with Google Maps shows origin, destination, and live courier position
- Shipment lifecycle (business rules)
  - Created/Assigned → InTransit when leaving origin
  - → Delivered when within destination radius
  - → Cancelled if the path deviates beyond an off-route corridor
- Data and persistence
  - EF Core (SQL Server) with migrations and unique constraints (TrackingNumber, Driver NationalId, User Email)
- Security
  - Session-based auth for users/admin
  - Anti-forgery tokens on login/register
  - HTTPS redirection

## Tech Stack
- Backend: ASP.NET Core MVC (.NET 8), SignalR
- Data: EF Core (SqlServer), Migrations
- UI: Razor Views, Bootstrap 5, SignalR JS, Google Maps JS API
- Hosting: Kestrel (Visual Studio/IIS Express or CLI)

## Quick Start
1. Set your Google Maps API key in `appsettings.json`.
2. Run the app (VS or CLI).
3. Login as Admin at `/Admin/Login` to create drivers and shipments.
4. Users can register at `/Users/Register`.
5. Track shipments at `/t/{TRACKING}` and see live updates on the map.
