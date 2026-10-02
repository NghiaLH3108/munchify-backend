-- ============================================================================
-- PROJECT: FOOD & BEVERAGE STORE ORDER MANAGEMENT SYSTEM
-- SCRIPT: DATABASE CREATION & MOCK DATA INSERTION
-- DATE RANGE OF MOCK DATA: MAY 2026 - SEPTEMBER 2026
-- ============================================================================

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'FnBStoreDB')
BEGIN
    ALTER DATABASE FnBStoreDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FnBStoreDB;
END
GO

CREATE DATABASE FnBStoreDB;
GO

USE FnBStoreDB;
GO

-- ============================================================================
-- 1. CREATE TABLES (SCHEMA DESIGN)
-- ============================================================================

-- 1. Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NULL,
    Email VARCHAR(100) NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'STAFF', -- 'ADMIN', 'STAFF'
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 2. Categories Table
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 3. Units Table
CREATE TABLE Units (
    UnitId INT IDENTITY(1,1) PRIMARY KEY,
    UnitName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 4. Products Table
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
    UnitId INT NOT NULL FOREIGN KEY REFERENCES Units(UnitId),
    ProductCode VARCHAR(50) NOT NULL UNIQUE,
    ProductName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    StockQuantity INT NOT NULL DEFAULT 0,
    MinStockLevel INT NOT NULL DEFAULT 5,
    ImageUrl VARCHAR(500) NULL,
    IsCombo BIT NOT NULL DEFAULT 0, -- 0: Single, 1: Combo
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 5. ProductCombos Table
CREATE TABLE ProductCombos (
    ComboId INT IDENTITY(1,1) PRIMARY KEY,
    ParentProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    ComponentProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL DEFAULT 1
);

-- 6. Customers Table
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20) NOT NULL UNIQUE,
    LoyaltyPoints INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 7. Orders Table
CREATE TABLE Orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode VARCHAR(30) NOT NULL UNIQUE,
    StaffId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    CustomerId INT NULL FOREIGN KEY REFERENCES Customers(CustomerId),
    Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PointsEarned INT NOT NULL DEFAULT 0,
    PointsRedeemed INT NOT NULL DEFAULT 0,
    OrderStatus VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- 'PENDING', 'COMPLETED', 'CANCELLED'
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- 'PENDING', 'PAID', 'FAILED'
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL,
    CancelledAt DATETIME2 NULL
);

-- 8. OrderDetails Table
CREATE TABLE OrderDetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    ProductName NVARCHAR(150) NOT NULL,
    UnitName NVARCHAR(50) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL
);

-- 9. Payments Table
CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId),
    PaymentMethod VARCHAR(30) NOT NULL, -- 'CASH', 'QR_CODE', 'BANK_TRANSFER'
    Amount DECIMAL(18,2) NOT NULL,
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- 'PENDING', 'PAID', 'FAILED', 'REFUNDED'
    TransactionCode VARCHAR(100) NULL,
    PaidAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 10. StockMovements Table
CREATE TABLE StockMovements (
    StockMovementId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    MovementType VARCHAR(30) NOT NULL, -- 'IMPORT', 'SALE', 'COMBO_SALE', 'ADJUSTMENT', 'DAMAGE'
    Quantity INT NOT NULL,
    PreviousQuantity INT NOT NULL,
    NewQuantity INT NOT NULL,
    ReferenceType VARCHAR(30) NULL, -- 'ORDER', 'STOCK_IMPORT', 'MANUAL'
    ReferenceId INT NULL,
    Reason NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 11. Suppliers Table
CREATE TABLE Suppliers (
    SupplierId INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(150) NOT NULL,
    ContactName NVARCHAR(100) NULL,
    Phone VARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 11.5 SupplierCategories Table
CREATE TABLE SupplierCategories (
    SupplierId INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
    PRIMARY KEY (SupplierId, CategoryId)
);

-- 12. StockImports Table
CREATE TABLE StockImports (
    ImportId INT IDENTITY(1,1) PRIMARY KEY,
    ImportCode VARCHAR(30) NOT NULL UNIQUE,
    SupplierId INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status VARCHAR(20) NOT NULL DEFAULT 'DRAFT', -- 'DRAFT', 'COMPLETED', 'CANCELLED'
    ImportDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Notes NVARCHAR(255) NULL
);

-- 13. StockImportDetails Table
CREATE TABLE StockImportDetails (
    ImportDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ImportId INT NOT NULL FOREIGN KEY REFERENCES StockImports(ImportId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    Quantity INT NOT NULL,
    ImportPrice DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL
);

-- 14. Promotions Table
CREATE TABLE Promotions (
    PromotionId INT IDENTITY(1,1) PRIMARY KEY,
    PromotionCode VARCHAR(50) NOT NULL UNIQUE,
    PromotionName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(500) NULL,
    DiscountType VARCHAR(20) NOT NULL, -- 'PERCENTAGE', 'FIXED_AMOUNT'
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinimumOrderAmount DECIMAL(18,2) NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- 15. PromotionProducts Table
CREATE TABLE PromotionProducts (
    PromotionId INT NOT NULL FOREIGN KEY REFERENCES Promotions(PromotionId),
    ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId),
    PRIMARY KEY (PromotionId, ProductId)
);
GO

-- ============================================================================
-- 2. MOCK DATA INSERTION
-- ============================================================================

-- 2.1 Users (5 records)
INSERT INTO Users (Username, PasswordHash, FullName, Phone, Email, Role, IsActive, CreatedAt)
VALUES 
('admin', 'hash_admin_123', N'Nguyễn Văn Quản Lý', '0909000001', 'admin@fnbstore.com', 'ADMIN', 1, '2026-04-01 08:00:00'),
('staff_mai', 'hash_staff_123', N'Trần Thị Mai', '0909000002', 'mai.tt@fnbstore.com', 'STAFF', 1, '2026-04-15 08:00:00'),
('staff_nam', 'hash_staff_456', N'Lê Hoàng Nam', '0909000003', 'nam.lh@fnbstore.com', 'STAFF', 1, '2026-04-15 08:00:00'),
('staff_tuan', 'hash_staff_789', N'Phạm Anh Tuấn', '0909000004', 'tuan.pa@fnbstore.com', 'STAFF', 1, '2026-05-01 08:00:00'),
('staff_hoa', 'hash_staff_999', N'Võ Thị Hoa', '0909000005', 'hoa.vt@fnbstore.com', 'STAFF', 1, '2026-05-01 08:00:00');

-- 2.2 Categories (5 records)
INSERT INTO Categories (CategoryName, Description, IsActive, CreatedAt)
VALUES 
(N'Bánh Tráng & Snacks', N'Các loại bánh tráng trộn, cuộn và snacks đóng gói', 1, '2026-04-01 08:00:00'),
(N'Thịt Khố & Đồ Ăn Vặt', N'Khô gà, khô bò, các loại hạt', 1, '2026-04-01 08:00:00'),
(N'Nước Đóng Chai & Lon', N'Nước ngọt, trà xanh đóng lon/chai', 1, '2026-04-01 08:00:00'),
(N'Trà Sữa & Nước Pha Chế', N'Trà sữa, trà trái cây chế biến tại quầy', 1, '2026-04-01 08:00:00'),
(N'Gói Combo Tiết Kiệm', N'Các gói kết hợp đồ ăn và thức uống', 1, '2026-04-01 08:00:00');

-- 2.3 Units (5 records - đã bổ sung đủ 3 giá trị)
INSERT INTO Units (UnitName, Description, IsActive)
VALUES 
(N'Bịch', N'Đơn vị bịch ni-lông/túi hàn', 1),
(N'Gói', N'Đơn vị gói đóng sẵn', 1),
(N'Chai', N'Đơn vị chai đóng sẵn', 1),
(N'Lon', N'Đơn vị lon kim loại', 1),
(N'Ly', N'Đơn vị ly nhựa/ly giấy', 1);

-- 2.4 Suppliers (5 records)
INSERT INTO Suppliers (SupplierName, ContactName, Phone, Address, IsActive, CreatedAt)
VALUES 
(N'Công Ty TNHH Thực Phẩm An An', N'Nguyễn Văn An', '02838111111', N'123 Lê Văn Sỹ, Q.3, TP.HCM', 1, '2026-04-01 08:00:00'),
(N'Nhà Cung Cấp Đồ Ăn Vặt Sài Gòn', N'Lê Thị Bích', '02838222222', N'45 Hùng Vương, Q.5, TP.HCM', 1, '2026-04-01 08:00:00'),
(N'Đại Lý Nước Giải Khát Tân Bình', N'Trần Đình Trọng', '02838333333', N'78 Lý Thường Kiệt, Q. Tân Bình, TP.HCM', 1, '2026-04-01 08:00:00'),
(N'Công Ty Nông Sản Xanh', N'Phạm Minh Hoàng', '02838444444', N'12 Hoàng Diệu, Q.4, TP.HCM', 1, '2026-04-01 08:00:00'),
(N'Nhà Phân Phối Trà & Nguyên Liệu F&B', N'Vũ Thị Hà', '02838555555', N'99 Nguyễn Trãi, Q.1, TP.HCM', 1, '2026-04-01 08:00:00');

-- 2.4.5 SupplierCategories (Mock data mapping)
INSERT INTO SupplierCategories (SupplierId, CategoryId)
VALUES 
(1, 1), -- An An supplies Bánh Tráng & Snacks
(2, 2), -- Sài Gòn supplies Thịt Khô
(3, 3), -- Tân Bình supplies Nước Đóng Chai
(4, 4), -- Nông Sản Xanh supplies Nước Pha Chế
(1, 2); -- An An also supplies Thịt Khô

-- 2.5 Customers (5 records)
INSERT INTO Customers (CustomerName, Phone, LoyaltyPoints, CreatedAt, UpdatedAt)
VALUES 
(N'Nguyễn Văn A', '0901234567', 4, '2026-05-01 10:00:00', '2026-09-12 14:20:00'),
(N'Trần Thị B', '0912345678', 3, '2026-05-05 11:30:00', '2026-08-08 15:00:00'),
(N'Lê Văn C', '0923456789', 1, '2026-06-01 09:15:00', '2026-09-03 12:15:00'),
(N'Phạm Thị D', '0934567890', 6, '2026-06-15 14:00:00', '2026-09-10 17:00:00'),
(N'Hoàng Văn E', '0945678901', 2, '2026-07-01 16:20:00', '2026-09-13 16:00:00');

-- 2.6 Promotions (5 records)
INSERT INTO Promotions (PromotionCode, PromotionName, Description, DiscountType, DiscountValue, MinimumOrderAmount, StartDate, EndDate, IsActive, CreatedAt)
VALUES 
('PROMO_HE2026', N'Chào Hè Giảm 10k', N'Giảm 10,000 VNĐ cho đơn từ 100,000 VNĐ', 'FIXED_AMOUNT', 10000.00, 100000.00, '2026-05-01 00:00:00', '2026-06-30 23:59:59', 1, '2026-04-25 08:00:00'),
('PROMO_JUNE15', N'Ưu Đãi Tháng 6', N'Giảm 15,000 VNĐ cho đơn từ 80,000 VNĐ', 'FIXED_AMOUNT', 15000.00, 80000.00, '2026-06-01 00:00:00', '2026-06-30 23:59:59', 1, '2026-05-28 08:00:00'),
('PROMO_JULY10', N'Giải Nhiệt Mùa Hè', N'Giảm 10% cho đơn từ 50,000 VNĐ', 'PERCENTAGE', 10.00, 50000.00, '2026-07-01 00:00:00', '2026-07-31 23:59:59', 1, '2026-06-28 08:00:00'),
('PROMO_AUG20', N'Bứt Phá Tháng 8', N'Giảm 20,000 VNĐ cho đơn từ 150,000 VNĐ', 'FIXED_AMOUNT', 20000.00, 150000.00, '2026-08-01 00:00:00', '2026-08-31 23:59:59', 1, '2026-07-28 08:00:00'),
('PROMO_THANG9', N'Chào Thu Giảm 5%', N'Giảm 5% cho tất cả các đơn hàng từ 50,000 VNĐ', 'PERCENTAGE', 5.00, 50000.00, '2026-09-01 00:00:00', '2026-09-30 23:59:59', 1, '2026-08-28 08:00:00');

-- 2.7 Products (13 records)
INSERT INTO Products (CategoryId, UnitId, ProductCode, ProductName, Description, Price, StockQuantity, MinStockLevel, IsCombo, IsActive, CreatedAt)
VALUES 
(1, 1, 'P001', N'Bánh tráng trộn Sài Gòn', N'Bánh tráng trộn muối tắc, bò khô, trứng cút', 25000.00, 188, 20, 0, 1, '2026-04-20 08:00:00'),
(1, 1, 'P002', N'Bánh tráng cuộn bơ', N'Bánh tráng cuộn sốt bơ béo ngậy', 30000.00, 187, 20, 0, 1, '2026-04-20 08:00:00'),
(2, 2, 'P003', N'Khô gà lá chanh 100g', N'Khô gà xé phay đậm vị lá chanh', 35000.00, 120, 15, 0, 1, '2026-04-20 08:00:00'),
(2, 2, 'P004', N'Khô bò cháy tỏi 100g', N'Khô bò miếng cháy tỏi thơm giòn', 55000.00, 97, 10, 0, 1, '2026-04-20 08:00:00'),
(3, 4, 'P005', N'Coca Cola Lon 320ml', N'Nước ngọt Coca Cola lon 320ml', 15000.00, 385, 30, 0, 1, '2026-04-20 08:00:00'),
(3, 3, 'P006', N'Trà xanh C2 Chai 450ml', N'Trà xanh vị chanh C2', 12000.00, 271, 25, 0, 1, '2026-04-20 08:00:00'),
(4, 5, 'P007', N'Trà sữa truyền thống', N'Trà sữa đậm đà kèm trân châu đen', 35000.00, 191, 20, 0, 1, '2026-04-20 08:00:00'),
(4, 5, 'P008', N'Trà đào cam sả', N'Trà đào thanh mát vị cam sả', 40000.00, 169, 15, 0, 1, '2026-04-20 08:00:00'),
(1, 2, 'P009', N'Khoai tây chiên Lays', N'Snack khoai tây chiên vị tự nhiên', 20000.00, 208, 15, 0, 1, '2026-04-20 08:00:00'),
(3, 3, 'P010', N'Nước suối Aquafina 500ml', N'Nước tinh khiết Aquafina', 10000.00, 346, 50, 0, 1, '2026-04-20 08:00:00'),
(5, 2, 'CB01', N'Combo Ăn Vặt Vui Vẻ', N'1 Bánh tráng trộn + 1 Coca Cola Lon', 50000.00, 0, 0, 1, 1, '2026-04-25 08:00:00'),
(5, 5, 'CB02', N'Combo Trà Sữa Thượng Hạng', N'1 Khô gà lá chanh + 1 Trà sữa truyền thống', 65000.00, 0, 0, 1, 1, '2026-04-25 08:00:00'),
(5, 2, 'CB03', N'Combo Tiệc Nhỏ', N'1 Bánh tráng cuộn + 1 Khô bò + 2 Trà C2', 100000.00, 0, 0, 1, 1, '2026-04-25 08:00:00');

-- 2.8 ProductCombos (7 records)
INSERT INTO ProductCombos (ParentProductId, ComponentProductId, Quantity)
VALUES 
(11, 1, 1),
(11, 5, 1),
(12, 3, 1),
(12, 7, 1),
(13, 2, 1),
(13, 4, 1),
(13, 6, 2);

-- 2.9 PromotionProducts (6 records)
INSERT INTO PromotionProducts (PromotionId, ProductId)
VALUES 
(1, 1), (1, 2),
(2, 3), (2, 4),
(3, 7), (3, 8);

-- 2.10 StockImports (5 records)
INSERT INTO StockImports (ImportCode, SupplierId, UserId, TotalAmount, Status, ImportDate, Notes)
VALUES 
('IMP20260502', 1, 1, 4000000.00, 'COMPLETED', '2026-05-02 08:30:00', N'Nhập hàng bánh tráng đầu tháng 5'),
('IMP20260605', 2, 1, 2750000.00, 'COMPLETED', '2026-06-05 09:00:00', N'Nhập khô gà khô bò tháng 6'),
('IMP20260710', 3, 1, 2800000.00, 'COMPLETED', '2026-07-10 10:15:00', N'Nhập nước giải khát đợt hè'),
('IMP20260815', 5, 1, 5200000.00, 'COMPLETED', '2026-08-15 14:00:00', N'Nhập nguyên liệu trà sữa chuẩn bị tháng 8'),
('IMP20260901', 4, 1, 3000000.00, 'COMPLETED', '2026-09-01 08:00:00', N'Nhập snack và nước suối đầu tháng 9');

-- 2.11 StockImportDetails (10 records)
INSERT INTO StockImportDetails (ImportId, ProductId, Quantity, ImportPrice, TotalAmount)
VALUES 
(1, 1, 100, 15000.00, 1500000.00),
(1, 2, 100, 25000.00, 2500000.00),
(2, 3, 50, 25000.00, 1250000.00),
(2, 4, 30, 50000.00, 1500000.00),
(3, 5, 200, 10000.00, 2000000.00),
(3, 6, 100, 8000.00, 800000.00),
(4, 7, 100, 22000.00, 2200000.00),
(4, 8, 100, 30000.00, 3000000.00),
(5, 9, 100, 15000.00, 1500000.00),
(5, 10, 200, 7500.00, 1500000.00);

-- 2.12 Orders (12 records)
INSERT INTO Orders (OrderCode, StaffId, CustomerId, Subtotal, DiscountAmount, TotalAmount, PointsEarned, PointsRedeemed, OrderStatus, PaymentStatus, CreatedAt, CompletedAt)
VALUES 
('ORD2026051001', 2, 1, 80000.00, 0.00, 80000.00, 0, 0, 'COMPLETED', 'PAID', '2026-05-10 10:15:00', '2026-05-10 10:16:00'),
('ORD2026052002', 2, 2, 140000.00, 10000.00, 130000.00, 1, 0, 'COMPLETED', 'PAID', '2026-05-20 14:30:00', '2026-05-20 14:32:00'),
('ORD2026060201', 3, 3, 95000.00, 15000.00, 80000.00, 0, 0, 'COMPLETED', 'PAID', '2026-06-02 11:00:00', '2026-06-02 11:02:00'),
('ORD2026061802', 3, 4, 210000.00, 10000.00, 200000.00, 2, 0, 'COMPLETED', 'PAID', '2026-06-18 16:45:00', '2026-06-18 16:47:00'),
('ORD2026070501', 4, 5, 126000.00, 12600.00, 113400.00, 1, 0, 'COMPLETED', 'PAID', '2026-07-05 09:20:00', '2026-07-05 09:22:00'),
('ORD2026072202', 4, 1, 130000.00, 0.00, 130000.00, 1, 0, 'COMPLETED', 'PAID', '2026-07-22 19:10:00', '2026-07-22 19:12:00'),
('ORD2026080801', 5, 2, 175000.00, 20000.00, 155000.00, 1, 0, 'COMPLETED', 'PAID', '2026-08-08 15:00:00', '2026-08-08 15:03:00'),
('ORD2026082502', 5, 4, 240000.00, 20000.00, 220000.00, 2, 0, 'COMPLETED', 'PAID', '2026-08-25 18:30:00', '2026-08-25 18:33:00'),
('ORD2026090301', 2, 3, 140000.00, 7000.00, 133000.00, 1, 0, 'COMPLETED', 'PAID', '2026-09-03 12:15:00', '2026-09-03 12:17:00'),
('ORD2026091002', 3, 4, 300000.00, 15000.00, 285000.00, 2, 0, 'COMPLETED', 'PAID', '2026-09-10 17:00:00', '2026-09-10 17:04:00'),
('ORD2026091201', 4, 1, 225000.00, 11250.00, 213750.00, 2, 0, 'COMPLETED', 'PAID', '2026-09-12 14:20:00', '2026-09-12 14:22:00'),
('ORD2026091301', 5, 5, 150000.00, 7500.00, 142500.00, 1, 0, 'COMPLETED', 'PAID', '2026-09-13 16:00:00', '2026-09-13 16:02:00');

-- 2.13 OrderDetails (20 records)
INSERT INTO OrderDetails (OrderId, ProductId, ProductName, UnitName, UnitPrice, Quantity, DiscountAmount, TotalAmount)
VALUES 
(1, 1, N'Bánh tráng trộn Sài Gòn', N'Bịch', 25000.00, 2, 0.00, 50000.00),
(1, 5, N'Coca Cola Lon 320ml', N'Lon', 15000.00, 2, 0.00, 30000.00),
(2, 3, N'Khô gà lá chanh 100g', N'Gói', 35000.00, 2, 0.00, 70000.00),
(2, 7, N'Trà sữa truyền thống', N'Ly', 35000.00, 2, 0.00, 70000.00),
(3, 4, N'Khô bò cháy tỏi 100g', N'Gói', 55000.00, 1, 0.00, 55000.00),
(3, 8, N'Trà đào cam sả', N'Ly', 40000.00, 1, 0.00, 40000.00),
(4, 11, N'Combo Ăn Vặt Vui Vẻ', N'Gói', 50000.00, 2, 0.00, 100000.00),
(4, 4, N'Khô bò cháy tỏi 100g', N'Gói', 55000.00, 2, 0.00, 110000.00),
(5, 2, N'Bánh tráng cuộn bơ', N'Bịch', 30000.00, 3, 0.00, 90000.00),
(5, 6, N'Trà xanh C2 Chai 450ml', N'Chai', 12000.00, 3, 0.00, 36000.00),
(6, 12, N'Combo Trà Sữa Thượng Hạng', N'Ly', 65000.00, 2, 0.00, 130000.00),
(7, 3, N'Khô gà lá chanh 100g', N'Gói', 35000.00, 3, 0.00, 105000.00),
(7, 7, N'Trà sữa truyền thống', N'Ly', 35000.00, 2, 0.00, 70000.00),
(8, 13, N'Combo Tiệc Nhỏ', N'Gói', 100000.00, 2, 0.00, 200000.00),
(8, 9, N'Khoai tây chiên Lays', N'Gói', 20000.00, 2, 0.00, 40000.00),
(9, 1, N'Bánh tráng trộn Sài Gòn', N'Bịch', 25000.00, 4, 0.00, 100000.00),
(9, 10, N'Nước suối Aquafina 500ml', N'Chai', 10000.00, 4, 0.00, 40000.00),
(10, 4, N'Khô bò cháy tỏi 100g', N'Gói', 55000.00, 4, 0.00, 220000.00),
(10, 8, N'Trà đào cam sả', N'Ly', 40000.00, 2, 0.00, 80000.00),
(11, 2, N'Bánh tráng cuộn bơ', N'Bịch', 30000.00, 5, 0.00, 150000.00),
(11, 5, N'Coca Cola Lon 320ml', N'Lon', 15000.00, 5, 0.00, 75000.00),
(12, 11, N'Combo Ăn Vặt Vui Vẻ', N'Gói', 50000.00, 3, 0.00, 150000.00);

-- 2.14 Payments (12 records)
INSERT INTO Payments (OrderId, PaymentMethod, Amount, PaymentStatus, TransactionCode, PaidAt, CreatedAt)
VALUES 
(1, 'CASH', 80000.00, 'PAID', NULL, '2026-05-10 10:16:00', '2026-05-10 10:15:00'),
(2, 'QR_CODE', 130000.00, 'PAID', 'PAY202605200012', '2026-05-20 14:32:00', '2026-05-20 14:30:00'),
(3, 'BANK_TRANSFER', 80000.00, 'PAID', 'FT202606029981', '2026-06-02 11:02:00', '2026-06-02 11:00:00'),
(4, 'QR_CODE', 200000.00, 'PAID', 'PAY202606180045', '2026-06-18 16:47:00', '2026-06-18 16:45:00'),
(5, 'CASH', 113400.00, 'PAID', NULL, '2026-07-05 09:22:00', '2026-07-05 09:20:00'),
(6, 'QR_CODE', 130000.00, 'PAID', 'PAY202607220088', '2026-07-22 19:12:00', '2026-07-22 19:10:00'),
(7, 'BANK_TRANSFER', 155000.00, 'PAID', 'FT202608081123', '2026-08-08 15:03:00', '2026-08-08 15:00:00'),
(8, 'QR_CODE', 220000.00, 'PAID', 'PAY202608250102', '2026-08-25 18:33:00', '2026-08-25 18:30:00'),
(9, 'CASH', 133000.00, 'PAID', NULL, '2026-09-03 12:17:00', '2026-09-03 12:15:00'),
(10, 'QR_CODE', 285000.00, 'PAID', 'PAY202609100551', '2026-09-10 17:04:00', '2026-09-10 17:00:00'),
(11, 'CASH', 213750.00, 'PAID', NULL, '2026-09-12 14:20:00', '2026-09-12 14:22:00'),
(12, 'QR_CODE', 142500.00, 'PAID', 'PAY202609130009', '2026-09-13 16:02:00', '2026-09-13 16:00:00');

-- 2.15 StockMovements (8 records)
INSERT INTO StockMovements (ProductId, UserId, MovementType, Quantity, PreviousQuantity, NewQuantity, ReferenceType, ReferenceId, Reason, CreatedAt)
VALUES 
(1, 1, 'IMPORT', 100, 0, 100, 'STOCK_IMPORT', 1, N'Nhập hàng mới theo phiếu IMP20260502', '2026-05-02 08:30:00'),
(1, 2, 'SALE', -2, 100, 98, 'ORDER', 1, N'Xuất bán đơn hàng ORD2026051001', '2026-05-10 10:16:00'),
(1, 3, 'COMBO_SALE', -2, 98, 96, 'ORDER', 4, N'Xuất bán 2 gói Combo CB01 (đơn ORD2026061802)', '2026-06-18 16:47:00'),
(5, 1, 'IMPORT', 200, 0, 200, 'STOCK_IMPORT', 3, N'Nhập hàng Coca Cola theo phiếu IMP20260710', '2026-07-10 10:15:00'),
(3, 1, 'DAMAGE', -5, 125, 120, 'MANUAL', NULL, N'Hàng hỏng bao bì rách nhãn trong ca kiểm kho', '2026-08-01 17:00:00'),
(7, 5, 'SALE', -2, 193, 191, 'ORDER', 7, N'Xuất bán đơn hàng ORD2026080801', '2026-08-08 15:03:00'),
(9, 1, 'IMPORT', 100, 110, 210, 'STOCK_IMPORT', 5, N'Nhập hàng khoai tây theo phiếu IMP20260901', '2026-09-01 08:00:00'),
(2, 4, 'SALE', -5, 192, 187, 'ORDER', 11, N'Xuất bán đơn hàng ORD2026091201', '2026-09-12 14:22:00');
GO

-- ============================================================================
-- 3. CHECK INSERTED DATA SUMMARY
-- ============================================================================
SELECT 'Users' AS TableName, COUNT(*) AS TotalRows FROM Users
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'Units', COUNT(*) FROM Units
UNION ALL SELECT 'Suppliers', COUNT(*) FROM Suppliers
UNION ALL SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL SELECT 'Promotions', COUNT(*) FROM Promotions
UNION ALL SELECT 'Products', COUNT(*) FROM Products
UNION ALL SELECT 'ProductCombos', COUNT(*) FROM ProductCombos
UNION ALL SELECT 'PromotionProducts', COUNT(*) FROM PromotionProducts
UNION ALL SELECT 'StockImports', COUNT(*) FROM StockImports
UNION ALL SELECT 'StockImportDetails', COUNT(*) FROM StockImportDetails
UNION ALL SELECT 'Orders', COUNT(*) FROM Orders
UNION ALL SELECT 'OrderDetails', COUNT(*) FROM OrderDetails
UNION ALL SELECT 'Payments', COUNT(*) FROM Payments
UNION ALL SELECT 'StockMovements', COUNT(*) FROM StockMovements;
GO