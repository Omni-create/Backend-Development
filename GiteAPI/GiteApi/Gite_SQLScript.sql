-- DROP TABLES IN CORRECT ORDER (to avoid foreign key constraints)
IF OBJECT_ID('dbo.ReservedFacility', 'U') IS NOT NULL DROP TABLE dbo.ReservedFacility;
IF OBJECT_ID('dbo.ReservedExtraOption', 'U') IS NOT NULL DROP TABLE dbo.ReservedExtraOption;
IF OBJECT_ID('dbo.Invoice', 'U') IS NOT NULL DROP TABLE dbo.Invoice;
IF OBJECT_ID('dbo.PaymentInfo', 'U') IS NOT NULL DROP TABLE dbo.PaymentInfo;
IF OBJECT_ID('dbo.Reservation', 'U') IS NOT NULL DROP TABLE dbo.Reservation;
IF OBJECT_ID('dbo.Facility', 'U') IS NOT NULL DROP TABLE dbo.Facility;
IF OBJECT_ID('dbo.ExtraOption', 'U') IS NOT NULL DROP TABLE dbo.ExtraOption;
IF OBJECT_ID('dbo.Bedroom', 'U') IS NOT NULL DROP TABLE dbo.Bedroom;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;

-- CREATE TABLES
CREATE TABLE Users (
    userID INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(100) NOT NULL,
    createdDate DATETIME DEFAULT GETDATE(),
    userRole NVARCHAR(20) NOT NULL,
    firstName NVARCHAR(50) NOT NULL,
    lastName NVARCHAR(50) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    phone NVARCHAR(20) NOT NULL
);

CREATE TABLE Bedroom (
    bedroomID INT IDENTITY(1,1) PRIMARY KEY,
    bedroomName NVARCHAR(50) NOT NULL,
    capacity INT NOT NULL,
    description NVARCHAR(500),
    availabilityStatus NVARCHAR(20) DEFAULT 'Available'
);

CREATE TABLE ExtraOption (
    extraOptionId INT IDENTITY(1,1) PRIMARY KEY,
    optionName NVARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL
);

CREATE TABLE Facility (
    facilityID INT IDENTITY(1,1) PRIMARY KEY,
    facilityName NVARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL
);

CREATE TABLE Reservation (
    reservationID INT IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL,
    reservationType NVARCHAR(20) NOT NULL,
    bedroomID INT,
    startDate DATE NOT NULL,
    endDate DATE NOT NULL,
    numberOfPersons INT NOT NULL,
    reservationStatus NVARCHAR(20) NOT NULL,
    FOREIGN KEY (userID) REFERENCES Users(userID)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    FOREIGN KEY (bedroomID) REFERENCES Bedroom(bedroomID)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

CREATE TABLE PaymentInfo (
    paymentInfoID INT IDENTITY(1,1) PRIMARY KEY,
    userID INT NOT NULL,
    lastFourDigits NVARCHAR(4),
    bankHolderName NVARCHAR(100),
    paymentMethod NVARCHAR(20) NOT NULL,
    paymentToken NVARCHAR(200),
    FOREIGN KEY (userID) REFERENCES Users(userID)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

-- FIXED: Changed ON DELETE SET NULL to ON DELETE NO ACTION
CREATE TABLE Invoice (
    invoiceID INT IDENTITY(1,1) PRIMARY KEY,
    reservationID INT NOT NULL,
    paymentInfoID INT,
    description NVARCHAR(500),
    totalCost DECIMAL(10,2) NOT NULL,
    paymentStatus NVARCHAR(20) NOT NULL,
    issueDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (reservationID) REFERENCES Reservation(reservationID)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    FOREIGN KEY (paymentInfoID) REFERENCES PaymentInfo(paymentInfoID)
        ON DELETE NO ACTION  -- FIXED: Changed from SET NULL to NO ACTION
        ON UPDATE NO ACTION
);

CREATE TABLE ReservedExtraOption (
    reservationID INT NOT NULL,
    extraOptionId INT NOT NULL,
    PRIMARY KEY (reservationID, extraOptionId),
    FOREIGN KEY (reservationID) REFERENCES Reservation(reservationID)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    FOREIGN KEY (extraOptionId) REFERENCES ExtraOption(extraOptionId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE ReservedFacility (
    reservationID INT NOT NULL,
    facilityID INT NOT NULL,
    PRIMARY KEY (reservationID, facilityID),
    FOREIGN KEY (reservationID) REFERENCES Reservation(reservationID)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    FOREIGN KEY (facilityID) REFERENCES Facility(facilityID)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
-- INSERT DUMMY DATA

-- Insert 6 Users
INSERT INTO Users (username, password, userRole, firstName, lastName, email, phone, createdDate) VALUES
('john_doe', 'password123', 'Customer', 'John', 'Doe', 'john.doe@email.com', '1234567890', '2023-01-15'),
('jane_smith', 'password456', 'Admin', 'Jane', 'Smith', 'jane.smith@email.com', '0987654321', '2023-02-20'),
('bob_wilson', 'password789', 'Customer', 'Bob', 'Wilson', 'bob.wilson@email.com', '5551234567', '2023-03-10'),
('alice_jones', 'password101', 'Customer', 'Alice', 'Jones', 'alice.jones@email.com', '5559876543', '2023-04-05'),
('charlie_brown', 'password202', 'Customer', 'Charlie', 'Brown', 'charlie.brown@email.com', '5554567890', '2023-05-12'),
('emma_davis', 'password303', 'Admin', 'Emma', 'Davis', 'emma.davis@email.com', '5557890123', '2023-06-18');

-- Insert Bedrooms (12 bedrooms)
INSERT INTO Bedroom (bedroomName, capacity, description, availabilityStatus) VALUES
('Sunrise Room', 2, 'Cozy room with morning sun, perfect for couples', 'Available'),
('Mountain View', 4, 'Large room with breathtaking mountain view', 'Available'),
('Forest Retreat', 3, 'Quiet room facing the peaceful forest', 'Occupied'),
('Ocean Breeze', 2, 'Room with sea view and fresh ocean air', 'Maintenance'),
('Royal Suite', 6, 'Luxurious suite with king-size bed and jacuzzi', 'Available'),
('Garden Room', 3, 'Room overlooking the beautiful garden', 'Available'),
('Cosy Corner', 2, 'Small but comfortable room for budget travelers', 'Occupied'),
('Family Suite', 8, 'Spacious suite perfect for large families', 'Available'),
('Honeymoon Suite', 2, 'Romantic room with champagne and roses', 'Available'),
('Business Class', 2, 'Room with work desk and high-speed internet', 'Maintenance'),
('Penthouse', 4, 'Top-floor room with panoramic views', 'Available'),
('Budget Bunker', 1, 'Economical single room', 'Available');

-- Insert ExtraOptions (10 options)
INSERT INTO ExtraOption (optionName, price) VALUES
('Breakfast Buffet', 25.00),
('Airport Transfer', 45.00),
('Daily Cleaning Service', 30.00),
('Bike Rental (per day)', 15.00),
('Car Rental (per day)', 75.00),
('Spa Access', 60.00),
('Evening Dinner', 40.00),
('Laundry Service', 20.00),
('Tour Guide', 100.00),
('Pet Accommodation', 35.00);

-- Insert Facilities (8 facilities)
INSERT INTO Facility (facilityName, price) VALUES
('Swimming Pool Access', 10.00),
('Conference Room (per hour)', 50.00),
('Gym Access', 15.00),
('Sauna Session', 25.00),
('Tennis Court (per hour)', 30.00),
('Business Center', 5.00),
('Game Room', 8.00),
('BBQ Area Reservation', 20.00);

-- Insert Reservations (20 reservations)
INSERT INTO Reservation (userID, reservationType, bedroomID, startDate, endDate, numberOfPersons, reservationStatus) VALUES
(1, 'Vacation', 1, '2024-03-01', '2024-03-07', 2, 'Confirmed'),
(1, 'Business', 5, '2024-04-10', '2024-04-15', 1, 'Confirmed'),
(2, 'Vacation', 2, '2024-03-15', '2024-03-20', 4, 'Confirmed'),
(2, 'Honeymoon', 9, '2024-05-01', '2024-05-07', 2, 'Pending'),
(3, 'Family Holiday', 8, '2024-06-10', '2024-06-20', 6, 'Confirmed'),
(3, 'Business', 10, '2024-04-05', '2024-04-08', 2, 'Confirmed'),
(4, 'Weekend Getaway', 3, '2024-03-22', '2024-03-24', 2, 'Cancelled'),
(4, 'Vacation', 6, '2024-07-15', '2024-07-25', 3, 'Confirmed'),
(5, 'Business', 10, '2024-04-12', '2024-04-14', 1, 'Confirmed'),
(5, 'Family Holiday', 8, '2024-08-01', '2024-08-10', 7, 'Pending'),
(6, 'Vacation', 4, '2024-05-20', '2024-05-27', 2, 'Confirmed'),
(6, 'Business', 2, '2024-04-18', '2024-04-20', 3, 'Confirmed'),
(1, 'Vacation', 7, '2024-09-01', '2024-09-10', 2, 'Pending'),
(2, 'Family Holiday', 8, '2024-10-05', '2024-10-12', 5, 'Confirmed'),
(3, 'Business', 5, '2024-11-15', '2024-11-17', 1, 'Confirmed'),
(4, 'Honeymoon', 9, '2024-12-20', '2024-12-27', 2, 'Pending'),
(5, 'Vacation', 11, '2024-07-01', '2024-07-07', 3, 'Confirmed'),
(6, 'Business', 10, '2024-05-10', '2024-05-12', 2, 'Confirmed'),
(1, 'Weekend Getaway', 12, '2024-06-07', '2024-06-09', 1, 'Confirmed'),
(2, 'Family Holiday', 8, '2024-08-15', '2024-08-25', 8, 'Pending');

-- Insert PaymentInfo (12 payment methods)
INSERT INTO PaymentInfo (userID, lastFourDigits, bankHolderName, paymentMethod, paymentToken) VALUES
(1, '1234', 'John Doe', 'Credit Card', 'tok_123456789'),
(1, '5678', 'John Doe', 'PayPal', 'paypal_987654321'),
(2, '9876', 'Jane Smith', 'Credit Card', 'tok_654321987'),
(2, '4321', 'Jane Smith', 'Debit Card', 'tok_111222333'),
(3, '5555', 'Bob Wilson', 'Credit Card', 'tok_444555666'),
(4, '7777', 'Alice Jones', 'Credit Card', 'tok_777888999'),
(5, '8888', 'Charlie Brown', 'Bank Transfer', 'bank_123456'),
(6, '9999', 'Emma Davis', 'Credit Card', 'tok_999000111'),
(1, '1111', 'John Doe', 'Apple Pay', 'apple_12345'),
(3, '2222', 'Bob Wilson', 'Google Pay', 'google_67890'),
(4, '3333', 'Alice Jones', 'PayPal', 'paypal_112233'),
(5, '4444', 'Charlie Brown', 'Credit Card', 'tok_4455667788');

-- Insert Invoices (25 invoices)
INSERT INTO Invoice (reservationID, paymentInfoID, description, totalCost, paymentStatus, issueDate) VALUES
(1, 1, 'Week vacation with breakfast', 850.00, 'Paid', '2024-02-28'),
(1, 1, 'Extra services', 120.00, 'Paid', '2024-03-02'),
(2, 2, 'Business trip conference', 1200.00, 'Pending', '2024-04-09'),
(3, 3, 'Family mountain vacation', 1500.00, 'Paid', '2024-03-14'),
(3, 3, 'Extra activities', 300.00, 'Paid', '2024-03-16'),
(4, 4, 'Honeymoon deposit', 500.00, 'Pending', '2024-04-20'),
(5, 5, 'Family holiday booking', 2200.00, 'Paid', '2024-06-05'),
(5, 5, 'Additional services', 450.00, 'Paid', '2024-06-12'),
(6, NULL, 'Business trip invoice', 600.00, 'Unpaid', '2024-04-04'),
(7, 6, 'Cancelled weekend fee', 150.00, 'Refunded', '2024-03-20'),
(8, 6, 'Summer vacation', 1800.00, 'Pending', '2024-07-10'),
(9, 7, 'Business conference', 400.00, 'Paid', '2024-04-11'),
(10, 7, 'Family holiday deposit', 1000.00, 'Pending', '2024-07-20'),
(11, 8, 'Beach vacation', 1400.00, 'Paid', '2024-05-18'),
(12, 8, 'Business meeting', 550.00, 'Paid', '2024-04-17'),
(13, 9, 'Autumn vacation deposit', 300.00, 'Pending', '2024-08-20'),
(14, 3, 'October family holiday', 2100.00, 'Paid', '2024-10-01'),
(15, 10, 'November business trip', 450.00, 'Paid', '2024-11-14'),
(16, 11, 'Christmas honeymoon deposit', 800.00, 'Pending', '2024-12-01'),
(17, 5, 'July penthouse vacation', 2100.00, 'Paid', '2024-06-28'),
(18, 8, 'May business conference', 500.00, 'Paid', '2024-05-09'),
(19, 1, 'Weekend budget stay', 120.00, 'Paid', '2024-06-06'),
(20, 4, 'August family holiday deposit', 1500.00, 'Pending', '2024-08-01'),
(8, 6, 'Additional charges', 200.00, 'Unpaid', '2024-07-20'),
(5, 5, 'Late check-out fee', 50.00, 'Paid', '2024-06-21');

-- Insert ReservedExtraOptions (30 records)
INSERT INTO ReservedExtraOption (reservationID, extraOptionId) VALUES
(1, 1), (1, 2), (1, 3),
(2, 1), (2, 6),
(3, 1), (3, 3), (3, 9),
(4, 1), (4, 2), (4, 6), (4, 7),
(5, 1), (5, 3), (5, 8), (5, 10),
(6, 1), (6, 5),
(7, 1),
(8, 1), (8, 3), (8, 9),
(9, 1),
(10, 1), (10, 3), (10, 10),
(11, 1), (11, 6),
(12, 1),
(13, 1), (13, 2);

-- Insert ReservedFacilities (25 records)
INSERT INTO ReservedFacility (reservationID, facilityID) VALUES
(1, 1), (1, 3),
(2, 2), (2, 6),
(3, 1), (3, 4), (3, 5),
(4, 1), (4, 4),
(5, 1), (5, 3), (5, 7), (5, 8),
(6, 2), (6, 6),
(7, 1),
(8, 1), (8, 3), (8, 5),
(9, 2), (9, 6),
(10, 1), (10, 7), (10, 8),
(11, 1), (11, 4);

