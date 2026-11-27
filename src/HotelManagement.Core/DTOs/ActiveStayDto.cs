using HotelManagement.Core.Enums;

namespace HotelManagement.Core.DTOs;

public sealed record ActiveStayDto(
    int StayId, int? ReservationId, string? ReservationCode,
    int RoomId, string RoomCode, int FloorId, string FloorName,
    int RoomTypeId, string TypeName,
    int CustomerId, string CustomerName, IdentityType IdentityType, string IdentityNumber, string Phone,
    DateTime ActualCheckIn, DateTime ExpectedCheckOut, int GuestCount,
    decimal HourlyRate, decimal DailyRate,
    decimal DepositAmount,
    decimal ServiceTotal, int ServiceLines, string? Note);

public sealed record StayServiceLineDto(
    int StayServiceId, int StayId, string ServiceName, string UnitName,
    decimal Quantity, decimal UnitPrice, decimal Amount, DateTime UsedAt);

public sealed record LookupItemDto(int Id, string Name);

public sealed record ActiveStaySnapshotDto(
    DateTime LoadedAt,
    IReadOnlyList<ActiveStayDto> Stays,
    IReadOnlyList<StayServiceLineDto> Services,
    IReadOnlyList<LookupItemDto> Floors,
    IReadOnlyList<LookupItemDto> RoomTypes,
    int ActiveRoomCount,
    int CheckoutPendingCount);

public enum DueFilter { All, Today, Overdue, Later }

public sealed record ActiveStayFilter(string? Keyword, int? FloorId, int? RoomTypeId, DueFilter Due);

public sealed record ActiveStaySummaryDto(
    int OccupiedRooms, int ActiveRoomCount, decimal OccupancyPercent,
    int Guests, decimal ServiceTotal, int ServiceLines,
    int DueToday, int Overdue, int CheckoutPending);

public sealed record ChargeEstimateDto(
    ChargeBasis Basis, int Units, decimal Rate, decimal RoomCharge, decimal ServiceCharge,
    decimal SubTotal, decimal Deposit, decimal AmountDue, decimal RefundDue);
