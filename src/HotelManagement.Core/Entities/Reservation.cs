using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Entities;

public class Reservation
{
    public int ReservationId { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int RoomTypeId { get; set; }
    public int? RoomId { get; set; }
    public DateTime ExpectedCheckIn { get; set; }
    public DateTime ExpectedCheckOut { get; set; }
    public decimal DepositAmount { get; set; }
    public string? Note { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public DateTime CreatedAt { get; set; }
}
