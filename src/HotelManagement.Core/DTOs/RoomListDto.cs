using HotelManagement.Core.Enums;

namespace HotelManagement.Core.DTOs;

public class RoomListDto
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int FloorId { get; set; }
    public int FloorNumber { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string TypeCode { get; set; }   =  string.Empty;
    public string TypeName { get; set; }   =  string.Empty;
    public RoomStatus Status { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }
}
