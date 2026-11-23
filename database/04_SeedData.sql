-- HotelManagementDb - sample data
-- INSERT OR IGNORE de co the chay lai ma khong tao ban ghi mau trung.

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
