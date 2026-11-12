using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Entities;

public class Stay
{
    public int StayId { get; set; }
    public int? ReservationId { get; set; }
    public int CustomerId { get; set; }
    public int RoomId { get; set; }
    public DateTime ActualCheckIn { get; set; }
    public DateTime ExpectedCheckOut { get; set; }
    public DateTime? ActualCheckOut { get; set; }
    public int GuestCount { get; set; }
    public decimal HourlyRateAtCheckIn { get; set; }
    public decimal DailyRateAtCheckIn { get; set; }
    public StayStatus Status { get; set; } = StayStatus.InProgress;
    public string? Note { get; set; }
}
