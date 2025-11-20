USE WorkSync_db;
GO


-- =============================================
-- Room Booking System Mock Data Generator
-- For SQL Server Management Studio (SSMS)
-- =============================================

SET NOCOUNT ON;

-- Clear existing data (optional - uncomment if needed)
-- DELETE FROM BookingLog;
-- DELETE FROM RoomLog;
-- DELETE FROM Booking;
-- DELETE FROM RoomAmenity;
-- DELETE FROM Room;

PRINT 'Starting mock data generation...';

-- =============================================
-- 1. INSERT ROOMS (20 rooms)
-- =============================================
PRINT 'Inserting Rooms...';

INSERT INTO ws.Rooms (RoomId, Name, Code, Seats, Location, Level, SizeLabel, Status, OperatingHours, ImageUrl, CreatedAt, UpdatedAt)
VALUES
-- Conference Rooms (Large)
('RM001', 'Executive Board Room', 'EXB-301', 20, 'North Wing', '3rd Floor', 'Large', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/1.webp', '2025-01-15 09:00:00', '2025-01-15 09:00:00'),
('RM002', 'Innovation Hub', 'INH-302', 18, 'South Wing', '3rd Floor', 'Large', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/2.jpg', '2025-01-15 09:15:00', '2025-01-15 09:15:00'),
('RM003', 'Grand Conference Hall', 'GCH-401', 50, 'Central Building', '4th Floor', 'Extra Large', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/3.webp', '2025-01-15 09:30:00', '2025-01-15 09:30:00'),

-- Meeting Rooms (Medium)
('RM004', 'Strategy Room A', 'STA-201', 12, 'East Wing', '2nd Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/4.jpg', '2025-01-15 10:00:00', '2025-01-15 10:00:00'),
('RM005', 'Strategy Room B', 'STB-202', 12, 'East Wing', '2nd Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/5.jpg', '2025-01-15 10:15:00', '2025-01-15 10:15:00'),
('RM006', 'Collaboration Space 1', 'CS1-203', 10, 'West Wing', '2nd Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/6.jpg', '2025-01-15 10:30:00', '2025-01-15 10:30:00'),
('RM007', 'Collaboration Space 2', 'CS2-204', 10, 'West Wing', '2nd Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/7.jpg', '2025-01-15 10:45:00', '2025-01-15 10:45:00'),
('RM008', 'Training Center Alpha', 'TCA-305', 15, 'North Wing', '3rd Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/8.jpg', '2025-01-15 11:00:00', '2025-01-15 11:00:00'),

-- Small Meeting Rooms
('RM009', 'Huddle Room 1', 'HR1-101', 6, 'North Wing', '1st Floor', 'Small', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/9.jpg', '2025-01-16 09:00:00', '2025-01-16 09:00:00'),
('RM010', 'Huddle Room 2', 'HR2-102', 6, 'South Wing', '1st Floor', 'Small', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/10.webp', '2025-01-16 09:15:00', '2025-01-16 09:15:00'),
('RM011', 'Focus Pod A', 'FPA-103', 4, 'East Wing', '1st Floor', 'Small', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/11.jpg', '2025-01-16 09:30:00', '2025-01-16 09:30:00'),
('RM012', 'Focus Pod B', 'FPB-104', 4, 'West Wing', '1st Floor', 'Small', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/12.webp', '2025-01-16 09:45:00', '2025-01-16 09:45:00'),
('RM013', 'Quick Meet 1', 'QM1-105', 4, 'Central Building', '1st Floor', 'Small', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/13.jpg', '2025-01-16 10:00:00', '2025-01-16 10:00:00'),

-- Special Purpose Rooms
('RM014', 'Video Conference Suite', 'VCS-402', 8, 'Central Building', '4th Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/14.jpg', '2025-01-16 10:30:00', '2025-01-16 10:30:00'),
('RM015', 'Client Presentation Room', 'CPR-303', 14, 'South Wing', '3rd Floor', 'Medium', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/15.jpg', '2025-01-16 11:00:00', '2025-01-16 11:00:00'),
('RM016', 'Workshop Studio', 'WKS-205', 25, 'East Wing', '2nd Floor', 'Large', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/20.jpg', '2025-01-16 11:30:00', '2025-01-16 11:30:00'),

-- Some inactive/maintenance rooms
('RM017', 'Legacy Conference Room', 'LCR-206', 16, 'West Wing', '2nd Floor', 'Medium', 'inactive', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/16.webp', '2025-01-17 09:00:00', '2025-01-17 09:00:00'),
('RM018', 'Renovation Space', 'RNV-304', 12, 'North Wing', '3rd Floor', 'Medium', 'maintenance', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/17.jpg', '2025-01-17 09:30:00', '2025-01-17 09:30:00'),
('RM019', 'Break Room Conference', 'BRC-106', 8, 'Central Building', '1st Floor', 'Small', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/18.jpg', '2025-01-17 10:00:00', '2025-01-17 10:00:00'),
('RM020', 'Sky Lounge Meeting', 'SLM-501', 30, 'Central Building', '5th Floor', 'Large', 'active', '8:00 AM - 4:00 PM', 'C:/Development/WorkSync/WorkSync-frontend/src/assets/room-images/19.webp', '2025-01-17 10:30:00', '2025-01-17 10:30:00');

PRINT '20 Rooms inserted successfully.';

-- =============================================
-- 2. INSERT ROOM AMENITIES
-- =============================================
PRINT 'Inserting Room Amenities...';

INSERT INTO ws.RoomAmenities (RoomId, Amenity)
VALUES
-- RM001 - Executive Board Room
('RM001', 'Projector'), ('RM001', 'Whiteboard'), ('RM001', 'Video Conference'), ('RM001', 'Conference Phone'), ('RM001', 'Smart TV'),
-- RM002 - Innovation Hub
('RM002', 'Projector'), ('RM002', 'Whiteboard'), ('RM002', 'Video Conference'), ('RM002', 'Wireless Presentation'),
-- RM003 - Grand Conference Hall
('RM003', 'Projector'), ('RM003', 'Stage'), ('RM003', 'Audio System'), ('RM003', 'Video Conference'), ('RM003', 'Multiple Screens'), ('RM003', 'Podium'),
-- RM004 - Strategy Room A
('RM004', 'Whiteboard'), ('RM004', 'TV Display'), ('RM004', 'Conference Phone'),
-- RM005 - Strategy Room B
('RM005', 'Whiteboard'), ('RM005', 'TV Display'), ('RM005', 'Video Conference'),
-- RM006 - Collaboration Space 1
('RM006', 'Whiteboard'), ('RM006', 'TV Display'), ('RM006', 'Wireless Presentation'),
-- RM007 - Collaboration Space 2
('RM007', 'Whiteboard'), ('RM007', 'TV Display'), ('RM007', 'Apple TV'),
-- RM008 - Training Center Alpha
('RM008', 'Projector'), ('RM008', 'Whiteboard'), ('RM008', 'Audio System'), ('RM008', 'Flip Charts'),
-- RM009 - Huddle Room 1
('RM009', 'TV Display'), ('RM009', 'Whiteboard'),
-- RM010 - Huddle Room 2
('RM010', 'TV Display'), ('RM010', 'Wireless Presentation'),
-- RM011 - Focus Pod A
('RM011', 'Monitor'), ('RM011', 'Whiteboard'),
-- RM012 - Focus Pod B
('RM012', 'Monitor'), ('RM012', 'Whiteboard'),
-- RM013 - Quick Meet 1
('RM013', 'TV Display'),
-- RM014 - Video Conference Suite
('RM014', 'Video Conference'), ('RM014', 'Multiple Cameras'), ('RM014', 'Smart TV'), ('RM014', 'Conference Phone'), ('RM014', 'Recording Equipment'),
-- RM015 - Client Presentation Room
('RM015', 'Projector'), ('RM015', 'Whiteboard'), ('RM015', 'Video Conference'), ('RM015', 'Wireless Presentation'), ('RM015', 'Coffee Station'),
-- RM016 - Workshop Studio
('RM016', 'Projector'), ('RM016', 'Multiple Whiteboards'), ('RM016', 'Flip Charts'), ('RM016', 'Movable Furniture'),
-- RM017 - Legacy Conference Room
('RM017', 'Projector'), ('RM017', 'Whiteboard'),
-- RM018 - Renovation Space
('RM018', 'Whiteboard'),
-- RM019 - Break Room Conference
('RM019', 'TV Display'), ('RM019', 'Coffee Machine'),
-- RM020 - Sky Lounge Meeting
('RM020', 'Projector'), ('RM020', 'Whiteboard'), ('RM020', 'Video Conference'), ('RM020', 'Smart TV'), ('RM020', 'Catering Setup'), ('RM020', 'Outdoor Access');

PRINT 'Room Amenities inserted successfully.';

-- =============================================
-- 3. INSERT ROOM LOGS (Creation logs for all rooms)
-- =============================================
PRINT 'Inserting Room Logs...';

INSERT INTO ws.RoomLogs (RoomIdString, RoomName, AuthorId, AuthorName, EventType, CurrentStatus, Timestamp)
VALUES
('RM001', 'Executive Board Room', 1, 'Admin User', 'create', 'Room created with 20 seats in North Wing, 3rd Floor', '2025-01-15 09:00:00'),
('RM002', 'Innovation Hub', 1, 'Admin User', 'create', 'Room created with 18 seats in South Wing, 3rd Floor', '2025-01-15 09:15:00'),
('RM003', 'Grand Conference Hall', 1, 'Admin User', 'create', 'Room created with 50 seats in Central Building, 4th Floor', '2025-01-15 09:30:00'),
('RM004', 'Strategy Room A', 1, 'Admin User', 'create', 'Room created with 12 seats in East Wing, 2nd Floor', '2025-01-15 10:00:00'),
('RM005', 'Strategy Room B', 1, 'Admin User', 'create', 'Room created with 12 seats in East Wing, 2nd Floor', '2025-01-15 10:15:00'),
('RM006', 'Collaboration Space 1', 1, 'Admin User', 'create', 'Room created with 10 seats in West Wing, 2nd Floor', '2025-01-15 10:30:00'),
('RM007', 'Collaboration Space 2', 1, 'Admin User', 'create', 'Room created with 10 seats in West Wing, 2nd Floor', '2025-01-15 10:45:00'),
('RM008', 'Training Center Alpha', 1, 'Admin User', 'create', 'Room created with 15 seats in North Wing, 3rd Floor', '2025-01-15 11:00:00'),
('RM009', 'Huddle Room 1', 1, 'Admin User', 'create', 'Room created with 6 seats in North Wing, 1st Floor', '2025-01-16 09:00:00'),
('RM010', 'Huddle Room 2', 1, 'Admin User', 'create', 'Room created with 6 seats in South Wing, 1st Floor', '2025-01-16 09:15:00'),
('RM011', 'Focus Pod A', 1, 'Admin User', 'create', 'Room created with 4 seats in East Wing, 1st Floor', '2025-01-16 09:30:00'),
('RM012', 'Focus Pod B', 1, 'Admin User', 'create', 'Room created with 4 seats in West Wing, 1st Floor', '2025-01-16 09:45:00'),
('RM013', 'Quick Meet 1', 1, 'Admin User', 'create', 'Room created with 4 seats in Central Building, 1st Floor', '2025-01-16 10:00:00'),
('RM014', 'Video Conference Suite', 1, 'Admin User', 'create', 'Room created with 8 seats in Central Building, 4th Floor', '2025-01-16 10:30:00'),
('RM015', 'Client Presentation Room', 1, 'Admin User', 'create', 'Room created with 14 seats in South Wing, 3rd Floor', '2025-01-16 11:00:00'),
('RM016', 'Workshop Studio', 1, 'Admin User', 'create', 'Room created with 25 seats in East Wing, 2nd Floor', '2025-01-16 11:30:00'),
('RM017', 'Legacy Conference Room', 1, 'Admin User', 'create', 'Room created with 16 seats in West Wing, 2nd Floor (initially inactive)', '2025-01-17 09:00:00'),
('RM018', 'Renovation Space', 1, 'Admin User', 'create', 'Room created with 12 seats in North Wing, 3rd Floor (under maintenance)', '2025-01-17 09:30:00'),
('RM019', 'Break Room Conference', 1, 'Admin User', 'create', 'Room created with 8 seats in Central Building, 1st Floor', '2025-01-17 10:00:00'),
('RM020', 'Sky Lounge Meeting', 1, 'Admin User', 'create', 'Room created with 30 seats in Central Building, 5th Floor', '2025-01-17 10:30:00');

PRINT 'Room Logs inserted successfully.';

-- =============================================
-- 4. INSERT BOOKINGS (Nov 2-29, 2025)
-- =============================================
PRINT 'Inserting Bookings for November 2-29, 2025...';

-- Helper table for booking titles and descriptions
DECLARE @BookingTitles TABLE (Title NVARCHAR(200), Description NVARCHAR(500));
INSERT INTO @BookingTitles VALUES
('Team Standup', 'Daily team synchronization meeting'),
('Project Planning', 'Strategic planning session for upcoming project milestones'),
('Client Presentation', 'Product demonstration and proposal presentation to client'),
('Training Session', 'Professional development and skill enhancement workshop'),
('Sprint Review', 'Review of completed sprint tasks and deliverables'),
('Budget Review', 'Quarterly budget analysis and financial planning'),
('Brainstorming Session', 'Creative ideation and problem-solving discussion'),
('Performance Review', 'Employee performance evaluation and goal setting'),
('Design Review', 'Product design critique and feedback session'),
('Technical Discussion', 'Deep dive into technical architecture and implementation'),
('Marketing Strategy', 'Marketing campaign planning and strategy alignment'),
('HR Interview', 'Candidate interview for open position'),
('Board Meeting', 'Executive board quarterly review meeting'),
('All Hands Meeting', 'Company-wide update and announcement session'),
('Department Sync', 'Cross-department coordination meeting'),
('Retrospective', 'Team retrospective and process improvement discussion'),
('Onboarding Session', 'New employee orientation and training'),
('Sales Pitch', 'Sales presentation to prospective client'),
('Product Demo', 'Product feature demonstration and Q&A'),
('Workshop', 'Hands-on training and skill-building workshop');

-- Generate bookings for each day from Nov 2 to Nov 29, 2025
DECLARE @CurrentDate DATE = '2025-11-02';
DECLARE @EndDate DATE = '2025-11-29';
DECLARE @BookingsPerDay INT;
DECLARE @DayCounter INT;
DECLARE @BookingId INT;
SET @BookingId = 0;

WHILE @CurrentDate <= @EndDate
BEGIN
    -- Random number of bookings per day (5-20)
    SET @BookingsPerDay = 5 + ABS(CHECKSUM(NEWID())) % 16; -- Random between 5-20
    SET @DayCounter = 0;
    
    
    WHILE @DayCounter < @BookingsPerDay
    BEGIN
        -- Random room (only active rooms for most bookings)
        DECLARE @RandomRoom NVARCHAR(10) = (
            SELECT TOP 1 RoomId 
            FROM ws.Rooms 
            WHERE Status = 'active' 
            ORDER BY NEWID()
        );
        
        -- Random user (1-5)
        DECLARE @RandomUser INT = 1 + ABS(CHECKSUM(NEWID())) % 2;
        
        -- Random hour (8-15, since 8am-4pm and meetings can be 1 hour)
        DECLARE @RandomHour INT = 8 + ABS(CHECKSUM(NEWID())) % 8;
        
        -- Random duration (30 min, 1 hour, 1.5 hours, or 2 hours)
        DECLARE @DurationOptions TABLE (Minutes INT);
        INSERT INTO @DurationOptions VALUES (30), (60), (90), (120);
        DECLARE @Duration INT = (SELECT TOP 1 Minutes FROM @DurationOptions ORDER BY NEWID());
        
        -- Build datetime
        DECLARE @StartTime DATETIME = DATEADD(HOUR, @RandomHour, CAST(@CurrentDate AS DATETIME));
        DECLARE @EndTime DATETIME = DATEADD(MINUTE, @Duration, @StartTime);
        
        -- Ensure we don't exceed 4pm
        IF DATEPART(HOUR, @EndTime) < 16
        BEGIN
            -- Random title and description
            DECLARE @Title NVARCHAR(200);
            DECLARE @Description NVARCHAR(500);
            SELECT TOP 1 @Title = Title, @Description = Description 
            FROM @BookingTitles 
            ORDER BY NEWID();
            
            -- Random status (95% approved, 5% declined)
            DECLARE @Status NVARCHAR(20) = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 95 THEN 'approved' ELSE 'declined' END;
            
            -- Random expected attendees (2-room capacity)
            DECLARE @RoomSeats INT = (SELECT Seats FROM ws.Rooms WHERE RoomId = @RandomRoom);
            DECLARE @ExpectedAttendees INT = 2 + ABS(CHECKSUM(NEWID())) % (@RoomSeats - 1);
            
            -- Insert booking
            INSERT INTO ws.Bookings (BookingId, RoomId, UserRefId, Title, Description, StartDatetime, EndDatetime, Recurrence, Status, ExpectedAttendees, CreatedAt, UpdatedAt)
            VALUES (
                @BookingId,
                @RandomRoom,
                @RandomUser,
                @Title,
                @Description,
                @StartTime,
                @EndTime,
                NULL, -- No recurrence for this data
                @Status,
                @ExpectedAttendees,
                DATEADD(DAY, -1, @StartTime), -- Created day before
                DATEADD(DAY, -1, @StartTime)
            );
        END
        
        SET @BookingId = @BookingId + 1;
        SET @DayCounter = @DayCounter + 1;
    END
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

PRINT 'Bookings inserted successfully.';

-- =============================================
-- 5. INSERT BOOKING LOGS
-- =============================================
PRINT 'Inserting Booking Logs...';

-- Create booking log entries for each booking
INSERT INTO ws.BookingLogs (BookingIdString, BookingName, AuthorId, AuthorName, EventType, CurrentStatus, Timestamp)
SELECT 
    CAST(b.BookingId AS NVARCHAR(50)),
    b.Title,
    b.UserRefId,
    CASE b.UserRefId
        WHEN 1 THEN 'Sample User'
        WHEN 2 THEN 'Sample User2'
        WHEN 3 THEN 'Sample Admin'
    END,
    'create',
    'Booking created for ' + r.Name + ' from ' + 
    CONVERT(NVARCHAR, b.StartDatetime, 120) + ' to ' + 
    CONVERT(NVARCHAR, b.EndDatetime, 120),
    b.CreatedAt
FROM ws.Bookings b
INNER JOIN ws.Rooms r ON b.RoomId = r.RoomId;

-- Add some approval/decline logs
INSERT INTO ws.BookingLogs (BookingIdString, BookingName, AuthorId, AuthorName, EventType, CurrentStatus, Timestamp)
SELECT 
    CAST(b.BookingId AS NVARCHAR(50)),
    b.Title,
    1, -- Admin approved/declined
    'Admin User',
    CASE b.Status
        WHEN 'approved' THEN 'approve'
        WHEN 'declined' THEN 'decline'
    END,
    CASE b.Status
        WHEN 'approved' THEN 'Booking approved'
        WHEN 'declined' THEN 'Booking declined - room conflict or maintenance'
    END,
    DATEADD(HOUR, 2, b.CreatedAt) -- Approved/declined 2 hours after creation
FROM ws.Bookings b;

PRINT 'Booking Logs inserted successfully.';

-- =============================================
-- SUMMARY STATISTICS
-- =============================================
PRINT '';
PRINT '=== Mock Data Generation Complete ===';
PRINT 'Rooms: ' + CAST((SELECT COUNT(*) FROM Room) AS NVARCHAR(10));
PRINT 'Room Amenities: ' + CAST((SELECT COUNT(*) FROM RoomAmenity) AS NVARCHAR(10));
PRINT 'Room Logs: ' + CAST((SELECT COUNT(*) FROM RoomLog) AS NVARCHAR(10));
PRINT 'Bookings: ' + CAST((SELECT COUNT(*) FROM Booking) AS NVARCHAR(10));
PRINT 'Booking Logs: ' + CAST((SELECT COUNT(*) FROM BookingLog) AS NVARCHAR(10));
PRINT '';
PRINT 'Date Range: November 2-29, 2025';
PRINT 'Active Rooms: ' + CAST((SELECT COUNT(*) FROM Room WHERE Status = 'active') AS NVARCHAR(10));
PRINT 'Approved Bookings: ' + CAST((SELECT COUNT(*) FROM Booking WHERE Status = 'approved') AS NVARCHAR(10));
PRINT 'Declined Bookings: ' + CAST((SELECT COUNT(*) FROM Booking WHERE Status = 'declined') AS NVARCHAR(10));

SET NOCOUNT OFF;

DECLARE @QueryDate DATE = '2025-11-15'; -- Change this date as needed

SELECT 
    b.BookingId,
    r.RoomId,
    r.Name AS RoomName,
    r.Code AS RoomCode,
    b.Title AS BookingTitle,
    b.StartDatetime,
    b.EndDatetime,
    b.Status
FROM ws.Bookings b
INNER JOIN ws.Rooms r ON b.RoomId = r.RoomId
WHERE CAST(b.StartDatetime AS DATE) = @QueryDate
    AND b.Status = 'approved'
ORDER BY r.Name, b.StartDatetime;