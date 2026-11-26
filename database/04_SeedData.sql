-- HotelManagementDb - sample data

INSERT OR IGNORE INTO Floor (FloorId, FloorNumber, FloorName, SortOrder, IsActive) VALUES
(1, 1, 'Tầng 1', 1, 1),
(2, 2, 'Tầng 2', 2, 1),
(3, 3, 'Tầng 3', 3, 1);

INSERT OR IGNORE INTO RoomType (RoomTypeId, TypeCode, TypeName, Description, MaxOccupancy, HourlyRate, DailyRate, IsActive) VALUES
(1, 'STD', 'Standard', 'Phòng tiêu chuẩn', 2, 100000, 500000, 1),
(2, 'DLX', 'Deluxe', 'Phòng cao cấp', 3, 150000, 750000, 1);

INSERT OR IGNORE INTO Room (RoomId, RoomCode, FloorId, RoomTypeId, Status, Note, IsActive) VALUES
(1, '101', 1, 1, 'Available', NULL, 1),
(2, '102', 1, 1, 'Occupied', NULL, 1),
(3, '103', 1, 2, 'Reserved', NULL, 1),
(4, '104', 1, 2, 'Cleaning', NULL, 1),
(5, '201', 2, 1, 'Available', NULL, 1),
(6, '202', 2, 1, 'Occupied', NULL, 1),
(7, '203', 2, 2, 'Reserved', NULL, 1),
(8, '204', 2, 2, 'Available', NULL, 1),
(9, '301', 3, 1, 'Occupied', NULL, 1),
(10, '302', 3, 1, 'Cleaning', 'Phòng đang bảo trì/dọn bổ sung', 1),
(11, '303', 3, 2, 'Reserved', NULL, 1),
(12, '304', 3, 2, 'Available', NULL, 1);

INSERT OR IGNORE INTO Customer (CustomerId, FullName, IdentityType, IdentityNumber, Phone, Email, Address, CreatedAt, IsActive) VALUES
(1, 'Nguyễn Hoàng ', 'CCCD', '079200000001', '0901000001', 'hoang.ng@gmail.com', 'TP. Hồ Chí Minh', '2026-11-01 08:00:00', 1),
(2, 'Trần Quốc Bảo', 'CCCD', '079200000002', '0901000002', NULL, 'Đồng Nai', '2026-11-02 09:00:00', 1),
(3, 'Lê Thu Hà', 'Passport', 'B12345678', '0901000003', NULL, 'Bình Dương', '2026-11-03 10:00:00', 1);

INSERT OR IGNORE INTO Reservation (ReservationId, ReservationCode, CustomerId, RoomTypeId, RoomId, ExpectedCheckIn, ExpectedCheckOut, DepositAmount, Note, Status, CreatedAt) VALUES
(1, 'RSV-202611-001', 1, 2, 3, '2026-11-16 14:00:00', '2026-11-17 12:00:00', 100000, 'Đặt trước phòng 103', 'Confirmed', '2026-11-11 08:30:00'),
(2, 'RSV-202611-002', 2, 2, 7, '2026-11-18 14:00:00', '2026-11-19 12:00:00', 150000, 'Đặt trước phòng 203', 'Confirmed', '2026-11-11 09:00:00'),
(3, 'RSV-202611-003', 3, 2, 11, '2026-11-20 14:00:00', '2026-11-21 12:00:00', 100000, 'Đặt trước phòng 303', 'Confirmed', '2026-11-11 09:30:00');

INSERT OR IGNORE INTO Stay (StayId, ReservationId, CustomerId, RoomId, ActualCheckIn, ExpectedCheckOut, ActualCheckOut, GuestCount, HourlyRateAtCheckIn, DailyRateAtCheckIn, Status, Note) VALUES
(1, NULL, 1, 2, '2026-11-12 08:30:00', '2026-11-13 12:00:00', NULL, 2, 100000, 500000, 'InProgress', 'Khách vãng lai'),
(2, NULL, 2, 6, '2026-11-12 09:15:00', '2026-11-13 12:00:00', NULL, 1, 100000, 500000, 'InProgress', 'Khách vãng lai'),
(3, NULL, 3, 9, '2026-11-12 10:00:00', '2026-11-13 12:00:00', NULL, 2, 100000, 500000, 'InProgress', 'Khách vãng lai'),
(4, NULL, 1, 4, '2026-11-10 09:00:00', '2026-11-10 10:00:00', '2026-11-10 10:30:00', 2, 150000, 750000, 'Completed', 'Lượt lưu trú đã hoàn tất để minh họa hóa đơn');

INSERT OR IGNORE INTO HotelService (ServiceId, ServiceCode, ServiceName, Category, UnitName, UnitPrice, Description, IsActive) VALUES
(1, 'DRINK-01', 'Nước suối', 'FoodDrink', 'chai', 10000, 'Nước suối đóng chai', 1),
(2, 'DRINK-02', 'Nước ngọt', 'FoodDrink', 'lon', 15000, 'Nước ngọt đóng lon', 1),
(3, 'LAUNDRY-01', 'Giặt ủi', 'Laundry', 'kg', 30000, 'Dịch vụ giặt ủi nội bộ', 1),
(4, 'TRANSPORT-01', 'Thuê xe', 'Transport', 'lượt', 100000, 'Ghi nhận phí thuê xe thủ công, không kết nối đối tác', 1),
(5, 'OTHER-01', 'Dịch vụ khác', 'Other', 'lượt', 50000, 'Khoản dịch vụ nội bộ khác', 1);

INSERT OR IGNORE INTO StayService (StayServiceId, StayId, ServiceId, Quantity, UnitPrice, Amount, UsedAt, Note) VALUES
(1, 1, 1, '2', 10000, 20000, '2026-11-12 11:00:00', NULL),
(2, 2, 3, '1.5', 30000, 45000, '2026-11-12 12:00:00', NULL),
(3, 4, 2, '4', 15000, 60000, '2026-11-10 09:45:00', 'Dịch vụ đã được tính vào hóa đơn mẫu');

INSERT OR IGNORE INTO Invoice (InvoiceId, InvoiceCode, StayId, CreatedAt, ChargeBasis, ChargedUnits, AppliedRate, RoomCharge, ServiceCharge, SubTotal, DepositDeducted, AmountDue, RefundDue, Status, PrintedAt) VALUES
(1, 'INV-202611-001', 4, '2026-11-10 10:40:00', 'Day', 1, 750000, 750000, 60000, 810000, 100000, 710000, 0, 'Paid', '2026-11-10 10:45:00');

INSERT OR IGNORE INTO Payment (PaymentId, InvoiceId, PaymentCode, Amount, Method, Status, PaidAt, Note) VALUES
(1, 1, 'PAY-202611-001', 710000, 'Cash', 'Completed', '2026-11-10 10:42:00', 'Thanh toán mô phỏng bằng tiền mặt');

-- DU LIEU DEMO BAO CAO THANG (09/2026 - 11/2026)
-- Quy uoc bao cao:
-- Cong suat tinh theo gio Stay giao voi thang bao cao.
-- Doanh thu chi tinh Invoice.Status='Paid' va PrintedAt
-- Thang doanh thu duoc xac dinh theo PrintedAt.


-- Phong demo ngung kinh doanh
INSERT OR IGNORE INTO Room
    (RoomId, RoomCode, FloorId, RoomTypeId, Status, Note, IsActive)
VALUES
    (13, '399', 3, 2, 'Available', 'Phong demo ngung kinh doanh - khong tinh cong suat', 0);

-- Reservation lich su de kiem tra du lieu Stay co tien coc.
INSERT OR IGNORE INTO Reservation
    (ReservationId, ReservationCode, CustomerId, RoomTypeId, RoomId,
     ExpectedCheckIn, ExpectedCheckOut, DepositAmount, Note, Status, CreatedAt)
VALUES
    (4, 'RSV-202609-004', 2, 1, 5,
     '2026-09-10 14:00:00', '2026-09-12 14:00:00', 100000,
     'Demo bao cao thang 09 - da hoan tat', 'Completed', '2026-09-01 08:00:00'),
    (5, 'RSV-202610-005', 2, 1, 6,
     '2026-10-10 08:00:00', '2026-10-11 08:00:00', 50000,
     'Demo bao cao thang 10 - da hoan tat', 'Completed', '2026-10-01 08:00:00'),
    (6, 'RSV-202611-006', 3, 2, 4,
     '2026-11-22 20:00:00', '2026-11-23 08:00:00', 200000,
     'Demo bao cao thang 11 - da hoan tat', 'Completed', '2026-11-15 08:00:00');

INSERT OR IGNORE INTO Stay
    (StayId, ReservationId, CustomerId, RoomId, ActualCheckIn, ExpectedCheckOut,
     ActualCheckOut, GuestCount, HourlyRateAtCheckIn, DailyRateAtCheckIn, Status, Note)
VALUES
    (5, NULL, 1, 1, '2026-09-01 00:00:00', '2026-09-01 05:00:00',
     '2026-09-01 04:00:00', 2, 100000, 500000, 'Completed',
     'Demo cong suat giao thang 08 va 09'),
    (6, 4, 2, 5, '2026-09-10 14:00:00', '2026-09-12 14:00:00',
     '2026-09-12 14:00:00', 1, 100000, 500000, 'Completed',
     'Demo 48 gio trong thang 09'),
    (7, NULL, 3, 8, '2026-09-30 22:00:00', '2026-10-01 03:00:00',
     '2026-10-01 02:00:00', 2, 150000, 750000, 'CheckoutPending',
     'Demo Stay giao thang 09 va 10 - da thanh toan chua in'),
    (8, NULL, 1, 12, '2026-09-30 23:00:00', '2026-10-01 04:00:00',
     '2026-10-01 03:00:00', 1, 100000, 500000, 'Completed',
     'Demo Stay giao thang 09 va 10'),
    (9, 5, 2, 6, '2026-10-10 08:00:00', '2026-10-11 08:00:00',
     '2026-10-11 08:00:00', 1, 100000, 500000, 'Completed',
     'Demo 24 gio trong thang 10'),
    (10, NULL, 3, 7, '2026-10-20 18:00:00', '2026-10-21 08:00:00',
     '2026-10-21 06:00:00', 2, 150000, 750000, 'Completed',
     'Demo 12 gio trong thang 10'),
    (11, NULL, 1, 10, '2026-10-31 23:00:00', '2026-11-01 03:00:00',
     '2026-11-01 02:00:00', 1, 100000, 500000, 'Completed',
     'Demo Stay giao thang 10 va 11'),
    (12, NULL, 1, 12, '2026-11-18 09:00:00', '2026-11-19 12:00:00',
     '2026-11-19 09:00:00', 2, 150000, 750000, 'Completed',
     'Demo 24 gio trong thang 11'),
    (13, 6, 3, 4, '2026-11-22 20:00:00', '2026-11-23 12:00:00',
     '2026-11-23 08:00:00', 2, 150000, 750000, 'Completed',
     'Demo 12 gio trong thang 11, co tien coc'),
    (14, NULL, 2, 5, '2026-11-24 12:00:00', '2026-11-26 12:00:00',
     NULL, 1, 100000, 500000, 'InProgress',
     'Demo Stay dang mo - tinh toi thoi diem xem bao cao');

-- Cap nhat trang thai hien tai cho cac phong co Stay dang mo/cho in
-- hoac da hoan tat checkout trong bo du lieu demo.
UPDATE Room SET Status = 'Occupied' WHERE RoomId IN (5, 8);
UPDATE Room SET Status = 'Cleaning' WHERE RoomId = 12;

-- Cac khoan dich vu; Amount = Quantity x UnitPrice.
INSERT OR IGNORE INTO StayService
    (StayServiceId, StayId, ServiceId, Quantity, UnitPrice, Amount, UsedAt, Note)
VALUES
    (4, 5, 1, '2', 10000, 20000, '2026-09-01 01:00:00', 'Demo dich vu thang 09'),
    (5, 6, 3, '2', 30000, 60000, '2026-09-11 10:00:00', 'Demo dich vu thang 09'),
    (6, 7, 5, '1', 50000, 50000, '2026-09-30 20:00:00', 'Stay chua in hoa don'),
    (7, 8, 2, '1', 15000, 15000, '2026-10-01 01:00:00', 'Ca kiem thu hoa don Unpaid'),
    (8, 9, 2, '2', 15000, 30000, '2026-10-10 12:00:00', 'Demo dich vu thang 10'),
    (9, 10, 3, '1', 30000, 30000, '2026-10-20 22:00:00', 'Demo dich vu thang 10'),
    (10, 10, 1, '1', 10000, 10000, '2026-10-21 01:00:00', 'Demo dich vu thang 10'),
    (11, 12, 4, '1', 100000, 100000, '2026-11-18 15:00:00', 'Demo dich vu thang 11'),
    (12, 13, 2, '2', 15000, 30000, '2026-11-22 22:00:00', 'Demo dich vu thang 11');

-- Hoa don:
INSERT OR IGNORE INTO Invoice
    (InvoiceId, InvoiceCode, StayId, CreatedAt, ChargeBasis, ChargedUnits,
     AppliedRate, RoomCharge, ServiceCharge, SubTotal, DepositDeducted,
     AmountDue, RefundDue, Status, PrintedAt)
VALUES
    (2, 'INV-202609-002', 5, '2026-09-01 02:20:00', 'Hour', 4,
     100000, 400000, 20000, 420000, 0, 420000, 0, 'Paid', '2026-09-01 02:30:00'),
    (3, 'INV-202609-003', 6, '2026-09-12 14:10:00', 'Day', 2,
     500000, 1000000, 60000, 1060000, 100000, 960000, 0, 'Paid', '2026-09-12 14:20:00'),
    (4, 'INV-202609-004', 7, '2026-09-30 22:50:00', 'Hour', 4,
     150000, 600000, 50000, 650000, 0, 650000, 0, 'Paid', NULL),
    (5, 'INV-202610-005', 8, '2026-10-01 03:10:00', 'Hour', 4,
     100000, 400000, 15000, 415000, 0, 415000, 0, 'Unpaid', '2026-10-01 03:30:00'),
    (6, 'INV-202610-006', 9, '2026-10-11 08:10:00', 'Day', 1,
     500000, 500000, 30000, 530000, 50000, 480000, 0, 'Paid', NULL),
    (7, 'INV-202610-007', 10, '2026-10-21 06:10:00', 'Hour', 12,
     150000, 1800000, 40000, 1840000, 0, 1840000, 0, 'Paid', '2026-10-21 06:30:00'),
    (8, 'INV-202610-008', 11, '2026-10-31 23:50:00', 'Hour', 3,
     100000, 300000, 0, 300000, 0, 300000, 0, 'Paid', '2026-11-01 02:30:00'),
    (9, 'INV-202611-009', 12, '2026-11-19 09:10:00', 'Day', 1,
     750000, 750000, 100000, 850000, 0, 850000, 0, 'Paid', NULL),
    (10, 'INV-202611-010', 13, '2026-11-23 08:15:00', 'Hour', 12,
     150000, 1800000, 30000, 1830000, 200000, 1630000, 0, 'Paid', '2026-11-23 08:30:00');

-- Payment chi cho cac hoa don da thanh toan trong du lieu demo.
INSERT OR IGNORE INTO Payment
    (PaymentId, InvoiceId, PaymentCode, Amount, Method, Status, PaidAt, Note)
VALUES
    (2, 2, 'PAY-202609-002', 420000, 'Cash', 'Completed', '2026-09-01 02:25:00', 'Demo thang 09'),
    (3, 3, 'PAY-202609-003', 960000, 'Cash', 'Completed', '2026-09-12 14:15:00', 'Demo thang 09'),
    (4, 4, 'PAY-202609-004', 650000, 'Cash', 'Completed', '2026-10-01 02:10:00', 'Da thanh toan, cho in'),
    (5, 6, 'PAY-202610-006', 480000, 'Cash', 'Completed', '2026-10-11 08:20:00', 'Demo thang 10'),
    (6, 7, 'PAY-202610-007', 1840000, 'Cash', 'Completed', '2026-10-21 06:20:00', 'Demo thang 10'),
    (7, 8, 'PAY-202610-008', 300000, 'Cash', 'Completed', '2026-11-01 02:20:00', 'Hoa don tao thang 10, in thang 11'),
    (8, 9, 'PAY-202611-009', 850000, 'Cash', 'Completed', '2026-11-19 09:20:00', 'Demo thang 11'),
    (9, 10, 'PAY-202611-010', 1630000, 'Cash', 'Completed', '2026-11-23 08:25:00', 'Demo thang 11');
