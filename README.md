# FPTU-RBS: Technical Documentation

## 1. System Overview

FPTU-RBS (FPT University Room Booking System) is a comprehensive web-based application designed to facilitate room reservation management at FPT University. The system consists of two main components:

1. **Admin Portal (MVC)**: Allows administrators to manage rooms, bookings, and users
2. **User Portal (Razor Pages)**: Enables students and faculty to browse available rooms and make reservations

The application implements a multi-tier architecture following clean design principles with separate layers for:
- Presentation (UI)
- Business Logic (Services)
- Data Access (Repositories)
- Domain Models

## 2. Architecture Overview

### 2.1 Overall Architecture

The FPTU-RBS application follows a layered architecture:

```
┌───────────────────────────────────────────────┐
│               Presentation Layer               │
│  ┌────────────────────┐ ┌──────────────────┐  │
│  │ Admin Portal (MVC) │ │ User Portal (RP) │  │
│  └────────────────────┘ └──────────────────┘  │
└───────────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────┐
│             Business Logic Layer               │
│  ┌────────────────────────────────────────┐   │
│  │          Application Services          │   │
│  └────────────────────────────────────────┘   │
└───────────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────┐
│               Data Access Layer                │
│  ┌────────────────────────────────────────┐   │
│  │              Repositories              │   │
│  └────────────────────────────────────────┘   │
└───────────────────────────────────────────────┘
                      │
                      ▼
┌───────────────────────────────────────────────┐
│                Database Layer                  │
│  ┌────────────────────────────────────────┐   │
│  │           SQL Server Database          │   │
│  └────────────────────────────────────────┘   │
└───────────────────────────────────────────────┘
```

### 2.2 Project Structure

The solution consists of four main projects:

1. **BookingManagement.Admin.MVC**: ASP.NET Core MVC application for administrators
2. **BookingManagement.User.Razor**: ASP.NET Core Razor Pages application for users
3. **BookingManagement.Services**: Business logic layer with services and DTOs
4. **BookingManagement.Repositories**: Data access layer with repositories and domain models

## 3. Database Schema

The application uses a SQL Server database with the following key tables:

- **Room**: Stores information about rooms (ID, name, capacity, type, etc.)
- **User**: Contains user information (ID, email, password, full name, role)
- **Role**: Defines user roles (Admin, User)
- **TimeSlot**: Defines available time slots for bookings
- **Booking**: Records room reservations with details like room, user, date, time slot, and status
- **Notification**: Stores system notifications for users

### 3.1 Entity Relationship Diagram

```
┌───────────┐      ┌───────────┐      ┌───────────┐
│   Role    │◄─────┤   User    │─────►│  Booking  │◄─────┐
└───────────┘      └───────────┘      └───────────┘      │
                                            │             │
                                            ▼             │
                                      ┌───────────┐       │
                                      │  TimeSlot │       │
                                      └───────────┘       │
                                                          │
┌───────────┐                                      ┌─────────────┐
│   Room    │◄─────────────────────────────────────┤ Notification│
└───────────┘                                      └─────────────┘
```

### 3.2 Key Database Constraints

- Room status has predefined values (1: Active, 2: Maintenance)
- Booking status has predefined values (1: Pending, 2: Approved, 3: Rejected, 4: Completed, 5: Cancelled)
- TimeSlot has a constraint ensuring EndTime > StartTime
- Recurring bookings require an EndRecurringDate value

## 4. Admin Portal (MVC)

### 4.1 Key Components

- **Controllers**: Handle HTTP requests and orchestrate application flow
  - BookingController: Manages booking approvals, rejections, and completions
  - RoomController: Handles room management operations
  - UserController: Manages user accounts
  - TimeSlotController: Configures available time slots

- **Views**: Render HTML UI for administrators
  - Booking views: Index, Details, Approve, Reject, Complete
  - Room views: Index, Create, Edit, Details, Delete, Archived
  - User views: Index, Create, Edit, Details, ChangePassword

- **Models**: Define data structures for views
  - ViewModels for complex views requiring multiple data types

- **Hubs**: SignalR hubs for real-time notifications
  - BookingHub: Manages real-time booking notifications

### 4.2 Authentication & Authorization

- Cookie-based authentication with role-based authorization
- Policy-based authorization with "AdminOnly" policy
- Authentication middleware configuration in Program.cs

### 4.3 Key Features

- **Room Management**: Create, edit, archive, and delete rooms
- **Booking Approval Workflow**: Review, approve, reject, and complete bookings
- **User Management**: Create and manage users with different roles
- **Real-time Notifications**: Push notifications via SignalR
- **Reporting**: Export booking data to CSV format

## 5. User Portal (Razor Pages)

### 5.1 Key Components

- **Pages**: Razor Pages combining UI and logic
  - BookingRoom: Create, Edit, Details, Delete, Index
  - RoomList: Browse and view available rooms
  - Login/Register: User authentication
  - Notifications: View system notifications

- **PageModels**: Handle page logic and data operations

- **Hubs**: SignalR hubs for real-time notifications
  - NotificationHub: Manages user notifications

### 5.2 Authentication & Authorization

- Cookie-based authentication with role-based authorization
- Policy-based authorization with "UserOnly" policy
- Authentication middleware configuration in Program.cs

### 5.3 Key Features

- **Room Browsing**: View available rooms with details and photos
- **Booking Creation**: Reserve rooms for specific dates and time slots
- **Booking Management**: View, edit, and cancel existing bookings
- **Real-time Notifications**: Receive booking status updates
- **User Registration**: Self-registration for students and faculty

## 6. Service Layer

### 6.1 Key Services

- **AuthService**: Handles user authentication and authorization
- **BookingService**: Manages booking operations with business rules
- **RoomService**: Provides room-related functionality
- **TimeSlotService**: Manages available time slots
- **NotificationService**: Handles system notifications
- **SignalRService**: Facilitates real-time communication

### 6.2 Data Transfer Objects (DTOs)

- Used to transfer data between service and presentation layers
- Includes: RoomDto, TimeSlotDto, UserDto, AuthDto, etc.

### 6.3 Business Rules

- **Booking Constraints**:
  - Maximum of 3 active bookings per user
  - Bookings limited to 2 weeks in advance
  - No overlapping bookings for the same room and time slot
  - No past date bookings
  - No bookings for time slots that have already passed

## 7. Repository Layer

### 7.1 Repository Pattern

- Implements the Generic Repository pattern
- Unit of Work pattern for transaction management
- Repositories for each entity: Room, User, Booking, TimeSlot, etc.

### 7.2 Data Access

- Entity Framework Core for database access
- SQL Server as the database provider
- Repository interfaces define data access contracts
- Repository implementations provide concrete data access

## 8. Real-time Communication

### 8.1 SignalR Integration

- Two SignalR hubs:
  - BookingHub in Admin portal
  - NotificationHub in User portal

### 8.2 Notification Types

- New booking notifications
- Booking status updates (approval, rejection, completion)
- System messages

## 9. Deployment Configuration

### 9.1 Environment Configuration

- appsettings.json files for each application with:
  - Connection strings
  - Logging configuration
  - Shared resources paths

### 9.2 Shared Resources

- SharedAssets folder contains shared images
- Configured as static file middleware in both applications

## 10. Security Considerations

### 10.1 Authentication Security

- Cookie-based authentication with secure flags
- Password protection (basic in current implementation, should be enhanced)
- Role-based access control

### 10.2 Authorization

- Policy-based authorization
- Role-based access restrictions
- Controller and page-level authorization attributes

## 11. Future Development Recommendations

### 11.1 Security Enhancements

- Implement proper password hashing and salting
- Add CSRF protection
- Enable HTTPS-only cookies
- Implement rate limiting for login attempts

### 11.2 Feature Enhancements

- Email notifications for booking status changes
- Calendar view for room availability
- Mobile application support
- Recurring booking pattern improvements
- Integration with university academic calendar

### 11.3 Performance Improvements

- Implement caching for frequently accessed data
- Optimize database queries with proper indexing
- Consider asynchronous processing for notifications

## 12. API Documentation

### 12.1 Admin API Endpoints

- POST /Booking/ApproveConfirmed/{id}: Approve a booking
- POST /Booking/RejectConfirmed/{id}: Reject a booking with reason
- POST /Booking/CompleteConfirmed/{id}: Mark booking as completed
- GET /Booking/Export: Export bookings to CSV

### 12.2 User API Endpoints

- GET /BookingRoom/AvailableTimeSlots: Get available time slots for a room
- POST /BookingRoom/Create: Create a new booking
- POST /BookingRoom/Delete: Cancel a booking

## 13. Testing Considerations

### 13.1 Key Testing Areas

- Booking validation rules
- Real-time notification delivery
- Authentication and authorization
- Concurrency handling for bookings
- Data consistency across application tiers

### 13.2 Testing Approaches

- Unit testing for service and repository layers
- Integration testing for API endpoints
- UI testing for critical user flows
- Security testing for authentication and authorization

## 14. Conclusion

The FPTU-RBS application provides a comprehensive solution for room booking management at FPT University with separate portals for administrators and users. The system implements a clean architecture with proper separation of concerns, making it maintainable and extensible for future enhancements