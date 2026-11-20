using HotelManagement.Core.Enums;

namespace HotelManagement.Core.DTOs;

public class RoomDashboardDto
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int FloorId { get; set; }
    public int FloorNumber { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
    public RoomStatus Status { get; set; }
    public string? Note { get; set; }
    public int? ReservationId { get; set; }
    public int? StayId { get; set; }
    public StayStatus? StayStatus { get; set; }

}
