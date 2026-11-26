using HotelManagement.Core.Enums;

namespace HotelManagement.Core.DTOs;

public enum CheckInMode { Reservation, WalkIn }
public sealed record CheckInReservationDto(
    int ReservationId, string ReservationCode,
    int CustomerId, string CustomerName, string Phone,
    int RoomTypeId, string TypeName,
    int? RoomId, string? RoomCode,
    DateTime ExpectedCheckIn, DateTime ExpectedCheckOut,
    decimal DepositAmount, string? Note,
    ReservationStatus Status, DateTime CreatedAt,
    int? StayId);

public sealed record CheckInRoomDto(
    int RoomId, string RoomCode, string FloorName, RoomStatus Status,
    int RoomTypeId, string TypeCode, string TypeName,
    int MaxOccupancy, decimal HourlyRate, decimal DailyRate,
    bool IsUsable );


public sealed record CheckInDraftDto(
    CheckInMode Mode, int? ReservationId, int? CustomerId, int? RoomId,
    int GuestCount, DateTime ExpectedCheckOut, string? Note);

public sealed record CheckItemDto(string Title, bool Passed, string Detail);

public sealed record CheckInEvaluationDto(IReadOnlyList<CheckItemDto> Items)
{
    public bool CanConfirm => Items.Count > 0 && Items.All(i => i.Passed);
    public int PassedCount => Items.Count(i => i.Passed);
    public string FailureText => string.Join(Environment.NewLine,
        Items.Where(i => !i.Passed).Select(i => $"• {i.Title}: {i.Detail}"));
}

public sealed record CheckInResultDto(int StayId, string RoomCode, DateTime ActualCheckIn);
