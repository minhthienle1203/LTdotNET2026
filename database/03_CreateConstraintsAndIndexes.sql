-- HotelManagementDb - indexes
-- UNIQUE/FOREIGN KEY/CHECK da duoc khai bao ngay trong CREATE TABLE.

CREATE INDEX IF NOT EXISTS IX_Room_Floor_Status
    ON Room (FloorId, Status, IsActive);

CREATE INDEX IF NOT EXISTS IX_Room_RoomType
    ON Room (RoomTypeId);

CREATE INDEX IF NOT EXISTS IX_Reservation_Room_Time_Status
    ON Reservation (RoomId, ExpectedCheckIn, ExpectedCheckOut, Status);

CREATE INDEX IF NOT EXISTS IX_Reservation_Customer_CheckIn
    ON Reservation (CustomerId, ExpectedCheckIn);

CREATE UNIQUE INDEX IF NOT EXISTS UX_Stay_Reservation
    ON Stay (ReservationId)
    WHERE ReservationId IS NOT NULL;

CREATE INDEX IF NOT EXISTS IX_Stay_Room_Status
    ON Stay (RoomId, Status);

CREATE INDEX IF NOT EXISTS IX_Stay_Customer
    ON Stay (CustomerId);

CREATE INDEX IF NOT EXISTS IX_StayService_Stay_UsedAt
    ON StayService (StayId, UsedAt);

CREATE INDEX IF NOT EXISTS IX_Invoice_Status_CreatedAt
    ON Invoice (Status, CreatedAt);

CREATE INDEX IF NOT EXISTS IX_Payment_Invoice_PaidAt
    ON Payment (InvoiceId, PaidAt);
