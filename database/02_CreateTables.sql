-- HotelManagementDb - SQLite schema
-- Money: INTEGER, don vi VND (so nguyen); Entity C# van dung decimal.
-- Quantity: TEXT de giu chinh xac decimal; Service tinh Amount trong C#.

CREATE TABLE IF NOT EXISTS Floor (
                                     FloorId      INTEGER PRIMARY KEY AUTOINCREMENT,
                                     FloorNumber  INTEGER NOT NULL UNIQUE,
                                     FloorName    TEXT NOT NULL,
                                     SortOrder    INTEGER NOT NULL DEFAULT 0,
                                     IsActive     INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1))
    );

CREATE TABLE IF NOT EXISTS RoomType (
                                        RoomTypeId   INTEGER PRIMARY KEY AUTOINCREMENT,
                                        TypeCode     TEXT NOT NULL UNIQUE,
                                        TypeName     TEXT NOT NULL,
                                        Description  TEXT NULL,
                                        MaxOccupancy INTEGER NOT NULL CHECK (MaxOccupancy > 0),
    HourlyRate   INTEGER NOT NULL CHECK (HourlyRate >= 0),
    DailyRate    INTEGER NOT NULL CHECK (DailyRate >= 0),
    IsActive     INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1))
    );

CREATE TABLE IF NOT EXISTS Room (
                                    RoomId      INTEGER PRIMARY KEY AUTOINCREMENT,
                                    RoomCode    TEXT NOT NULL UNIQUE,
                                    FloorId     INTEGER NOT NULL,
                                    RoomTypeId  INTEGER NOT NULL,
                                    Status      TEXT NOT NULL DEFAULT 'Available'
                                    CHECK (Status IN ('Available', 'Reserved', 'Occupied', 'Cleaning')),
    Note        TEXT NULL,
    IsActive    INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
    FOREIGN KEY (FloorId) REFERENCES Floor(FloorId) ON UPDATE RESTRICT ON DELETE RESTRICT,
    FOREIGN KEY (RoomTypeId) REFERENCES RoomType(RoomTypeId) ON UPDATE RESTRICT ON DELETE RESTRICT
    );

CREATE TABLE IF NOT EXISTS Customer (
                                        CustomerId     INTEGER PRIMARY KEY AUTOINCREMENT,
                                        FullName       TEXT NOT NULL,
                                        IdentityType   TEXT NOT NULL CHECK (IdentityType IN ('CCCD', 'Passport')),
    IdentityNumber TEXT NOT NULL,
    Phone          TEXT NOT NULL,
    Email          TEXT NULL,
    Address        TEXT NULL,
    CreatedAt      TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    IsActive       INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1)),
    UNIQUE (IdentityType, IdentityNumber)
    );

CREATE TABLE IF NOT EXISTS Reservation (
                                           ReservationId     INTEGER PRIMARY KEY AUTOINCREMENT,
                                           ReservationCode   TEXT NOT NULL UNIQUE,
                                           CustomerId        INTEGER NOT NULL,
                                           RoomTypeId        INTEGER NOT NULL,
                                           RoomId            INTEGER NULL,
                                           ExpectedCheckIn   TEXT NOT NULL,
                                           ExpectedCheckOut  TEXT NOT NULL,
                                           DepositAmount     INTEGER NOT NULL DEFAULT 0 CHECK (DepositAmount >= 0),
    Note              TEXT NULL,
    Status            TEXT NOT NULL DEFAULT 'Confirmed'
    CHECK (Status IN ('Confirmed', 'CheckedIn', 'Completed', 'Cancelled', 'NoShow')),
    CreatedAt         TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    CHECK (ExpectedCheckOut > ExpectedCheckIn),
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId) ON UPDATE RESTRICT ON DELETE RESTRICT,
    FOREIGN KEY (RoomTypeId) REFERENCES RoomType(RoomTypeId) ON UPDATE RESTRICT ON DELETE RESTRICT,
    FOREIGN KEY (RoomId) REFERENCES Room(RoomId) ON UPDATE RESTRICT ON DELETE SET NULL
    );

CREATE TABLE IF NOT EXISTS Stay (
                                    StayId                 INTEGER PRIMARY KEY AUTOINCREMENT,
                                    ReservationId          INTEGER NULL,
                                    CustomerId             INTEGER NOT NULL,
                                    RoomId                 INTEGER NOT NULL,
                                    ActualCheckIn          TEXT NOT NULL,
                                    ExpectedCheckOut       TEXT NOT NULL,
                                    ActualCheckOut         TEXT NULL,
                                    GuestCount             INTEGER NOT NULL CHECK (GuestCount > 0),
    HourlyRateAtCheckIn    INTEGER NOT NULL CHECK (HourlyRateAtCheckIn >= 0),
    DailyRateAtCheckIn     INTEGER NOT NULL CHECK (DailyRateAtCheckIn >= 0),
    Status                 TEXT NOT NULL DEFAULT 'InProgress'
    CHECK (Status IN ('InProgress', 'CheckoutPending', 'Completed')),
    Note                   TEXT NULL,
    CHECK (ExpectedCheckOut > ActualCheckIn),
    CHECK (ActualCheckOut IS NULL OR ActualCheckOut >= ActualCheckIn),
    FOREIGN KEY (ReservationId) REFERENCES Reservation(ReservationId) ON UPDATE RESTRICT ON DELETE RESTRICT,
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId) ON UPDATE RESTRICT ON DELETE RESTRICT,
    FOREIGN KEY (RoomId) REFERENCES Room(RoomId) ON UPDATE RESTRICT ON DELETE RESTRICT
    );

CREATE TABLE IF NOT EXISTS HotelService (
                                            ServiceId    INTEGER PRIMARY KEY AUTOINCREMENT,
                                            ServiceCode  TEXT NOT NULL UNIQUE,
                                            ServiceName  TEXT NOT NULL,
                                            Category     TEXT NOT NULL CHECK (Category IN ('FoodDrink', 'Laundry', 'Transport', 'Other')),
    UnitName     TEXT NOT NULL,
    UnitPrice    INTEGER NOT NULL CHECK (UnitPrice >= 0),
    Description  TEXT NULL,
    IsActive     INTEGER NOT NULL DEFAULT 1 CHECK (IsActive IN (0, 1))
    );

CREATE TABLE IF NOT EXISTS StayService (
                                           StayServiceId INTEGER PRIMARY KEY AUTOINCREMENT,
                                           StayId        INTEGER NOT NULL,
                                           ServiceId     INTEGER NOT NULL,
                                           Quantity      TEXT NOT NULL CHECK (CAST(Quantity AS REAL) > 0),
    UnitPrice     INTEGER NOT NULL CHECK (UnitPrice >= 0),
    Amount        INTEGER NOT NULL CHECK (Amount >= 0),
    UsedAt        TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    Note          TEXT NULL,
    FOREIGN KEY (StayId) REFERENCES Stay(StayId) ON UPDATE RESTRICT ON DELETE RESTRICT,
    FOREIGN KEY (ServiceId) REFERENCES HotelService(ServiceId) ON UPDATE RESTRICT ON DELETE RESTRICT
    );

CREATE TABLE IF NOT EXISTS Invoice (
                                       InvoiceId       INTEGER PRIMARY KEY AUTOINCREMENT,
                                       InvoiceCode     TEXT NOT NULL UNIQUE,
                                       StayId          INTEGER NOT NULL UNIQUE,
                                       CreatedAt       TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    ChargeBasis     TEXT NOT NULL CHECK (ChargeBasis IN ('Hour', 'Day')),
    ChargedUnits    INTEGER NOT NULL CHECK (ChargedUnits > 0),
    AppliedRate     INTEGER NOT NULL CHECK (AppliedRate >= 0),
    RoomCharge      INTEGER NOT NULL CHECK (RoomCharge >= 0),
    ServiceCharge   INTEGER NOT NULL CHECK (ServiceCharge >= 0),
    SubTotal        INTEGER NOT NULL CHECK (SubTotal >= 0),
    DepositDeducted INTEGER NOT NULL CHECK (DepositDeducted >= 0),
    AmountDue       INTEGER NOT NULL CHECK (AmountDue >= 0),
    RefundDue       INTEGER NOT NULL CHECK (RefundDue >= 0),
    Status          TEXT NOT NULL DEFAULT 'Unpaid' CHECK (Status IN ('Unpaid', 'Paid')),
    PrintedAt       TEXT NULL,
    CHECK (SubTotal = RoomCharge + ServiceCharge),
    CHECK (DepositDeducted <= SubTotal),
    FOREIGN KEY (StayId) REFERENCES Stay(StayId) ON UPDATE RESTRICT ON DELETE RESTRICT
    );

CREATE TABLE IF NOT EXISTS Payment (
                                       PaymentId   INTEGER PRIMARY KEY AUTOINCREMENT,
                                       InvoiceId   INTEGER NOT NULL,
                                       PaymentCode TEXT NOT NULL UNIQUE,
                                       Amount      INTEGER NOT NULL CHECK (Amount > 0),
    Method      TEXT NOT NULL CHECK (Method IN ('Cash', 'Simulated')),
    Status      TEXT NOT NULL DEFAULT 'Completed' CHECK (Status IN ('Completed')),
    PaidAt      TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    Note        TEXT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoice(InvoiceId) ON UPDATE RESTRICT ON DELETE RESTRICT
    );
