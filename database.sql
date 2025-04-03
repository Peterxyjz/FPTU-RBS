-- Tạo database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'fptu_room_booking')
BEGIN
    CREATE DATABASE fptu_room_booking;
END
GO

USE fptu_room_booking;
GO

-- Xóa bảng nếu đã tồn tại để tránh lỗi
IF OBJECT_ID('Notification', 'U') IS NOT NULL
    DROP TABLE Notification;
IF OBJECT_ID('Booking', 'U') IS NOT NULL
    DROP TABLE Booking;
IF OBJECT_ID('TimeSlot', 'U') IS NOT NULL
    DROP TABLE TimeSlot;
IF OBJECT_ID('Room', 'U') IS NOT NULL
    DROP TABLE Room;
IF OBJECT_ID('dbo.User', 'U') IS NOT NULL
    DROP TABLE [User];
IF OBJECT_ID('Role', 'U') IS NOT NULL
    DROP TABLE Role;
GO

-- Tạo bảng Role
CREATE TABLE Role (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Tạo bảng User
CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    RoleID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
);
GO

-- Tạo bảng Room
CREATE TABLE Room (
    RoomID INT PRIMARY KEY, -- Số phòng sẽ là ID và là unique
    RoomName NVARCHAR(100) NOT NULL,
    Capacity INT NOT NULL,
    RoomType NVARCHAR(50) NOT NULL,
    Building NVARCHAR(100),
    Description NVARCHAR(MAX),
    ImageUrl NVARCHAR(255),
    Status INT NOT NULL DEFAULT 1, -- 1: Hoạt động, 2: Bảo trì
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Tạo bảng TimeSlot
CREATE TABLE TimeSlot (
    TimeSlotID INT IDENTITY(1,1) PRIMARY KEY,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CHK_TimeSlot_EndAfterStart CHECK (EndTime > StartTime)
);
GO

-- Tạo bảng Booking
CREATE TABLE Booking (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    RoomID INT NOT NULL,
    BookingDate DATE NOT NULL,
    TimeSlotID INT NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 1: Chờ duyệt, 2: Đã duyệt, 3: Từ chối, 4: Đã hủy
    RejectReason NVARCHAR(255),
    IsRecurring BIT DEFAULT 0,
    EndRecurringDate DATE,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID),
    FOREIGN KEY (RoomID) REFERENCES Room(RoomID),
    FOREIGN KEY (TimeSlotID) REFERENCES TimeSlot(TimeSlotID),
    CONSTRAINT CHK_Booking_RecurringValid CHECK (IsRecurring = 0 OR (IsRecurring = 1 AND EndRecurringDate IS NOT NULL))
);
GO

-- Tạo bảng Notification
CREATE TABLE Notification (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    IsRead BIT DEFAULT 0,
    BookingID INT,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES [User](UserID),
    FOREIGN KEY (BookingID) REFERENCES Booking(BookingID)
);
GO

-- Tạo các chỉ mục
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_RoleID ON [User](RoleID);

CREATE INDEX IX_Room_RoomType ON Room(RoomType);
CREATE INDEX IX_Room_Building ON Room(Building);
CREATE INDEX IX_Room_Status ON Room(Status);

CREATE INDEX IX_Booking_UserID ON Booking(UserID);
CREATE INDEX IX_Booking_RoomID ON Booking(RoomID);
CREATE INDEX IX_Booking_Status ON Booking(Status);
CREATE INDEX IX_Booking_Date_TimeSlot ON Booking(BookingDate, TimeSlotID, RoomID);

CREATE INDEX IX_Notification_UserID ON Notification(UserID);
CREATE INDEX IX_Notification_UserID_IsRead ON Notification(UserID, IsRead);
GO

-- Tạo Trigger cập nhật UpdatedAt khi dữ liệu thay đổi
CREATE TRIGGER trg_Role_Update ON Role AFTER UPDATE AS 
BEGIN
    UPDATE Role SET UpdatedAt = GETDATE() 
    FROM Role INNER JOIN inserted ON Role.RoleID = inserted.RoleID
END
GO

CREATE TRIGGER trg_User_Update ON [User] AFTER UPDATE AS 
BEGIN
    UPDATE [User] SET UpdatedAt = GETDATE() 
    FROM [User] INNER JOIN inserted ON [User].UserID = inserted.UserID
END
GO

CREATE TRIGGER trg_Room_Update ON Room AFTER UPDATE AS 
BEGIN
    UPDATE Room SET UpdatedAt = GETDATE() 
    FROM Room INNER JOIN inserted ON Room.RoomID = inserted.RoomID
END
GO

CREATE TRIGGER trg_TimeSlot_Update ON TimeSlot AFTER UPDATE AS 
BEGIN
    UPDATE TimeSlot SET UpdatedAt = GETDATE() 
    FROM TimeSlot INNER JOIN inserted ON TimeSlot.TimeSlotID = inserted.TimeSlotID
END
GO

CREATE TRIGGER trg_Booking_Update ON Booking AFTER UPDATE AS 
BEGIN
    UPDATE Booking SET UpdatedAt = GETDATE() 
    FROM Booking INNER JOIN inserted ON Booking.BookingID = inserted.BookingID
END
GO

CREATE TRIGGER trg_Notification_Update ON Notification AFTER UPDATE AS 
BEGIN
    UPDATE Notification SET UpdatedAt = GETDATE() 
    FROM Notification INNER JOIN inserted ON Notification.NotificationID = inserted.NotificationID
END
GO

-- Thêm dữ liệu mẫu
-- Thêm vai trò (Role)
INSERT INTO Role (RoleName, Description) VALUES 
(N'Admin', N'Quản trị viên hệ thống'),
(N'User', N'Người dùng thông thường');
GO

-- Thêm người dùng mẫu (User)
INSERT INTO [User] (Email, Password, FullName, RoleID) VALUES 
(N'admin@fpt.edu.vn', N'123', N'Admin User', 1),
(N'user@fpt.edu.vn', N'123', N'Normal User', 2);
GO

-- Thêm phòng học mẫu (Room)
INSERT INTO Room (RoomID, RoomName, Capacity, RoomType, Building, Description, ImageUrl) VALUES 
(101, N'Phòng học 101', 40, N'Phòng học', N'Alpha', N'Phòng học tiêu chuẩn', N'/shared-images/rooms/101.png'),
(102, N'Phòng học 102', 40, N'Phòng học', N'Alpha', N'Phòng học tiêu chuẩn', N'/shared-images/rooms/102.png'),
(201, N'Phòng lab 201', 30, N'Phòng máy tính', N'Beta', N'Phòng máy tính với 30 máy', N'/shared-images/rooms/201.png'),
(301, N'Phòng hội thảo 301', 100, N'Phòng hội thảo', N'Gamma', N'Phòng hội thảo lớn', N'/shared-images/rooms/301.png');
GO

-- Thêm khung giờ mẫu (TimeSlot)
INSERT INTO TimeSlot (StartTime, EndTime) VALUES 
('07:00:00', '09:15:00'), -- Slot 1: 7:00 - 9:15
('09:30:00', '11:45:00'), -- Slot 2: 9:30 - 11:45
('12:30:00', '14:45:00'), -- Slot 3: 12:30 - 14:45
('15:00:00', '17:15:00'), -- Slot 4: 15:00 - 17:15
('17:30:00', '19:45:00'), -- Slot 5: 17:30 - 19:45
('20:00:00', '22:15:00'); -- Slot 6: 20:00 - 22:15
GO

SELECT * FROM Room

