using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Entities;

public class Room
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public int FloorId { get; set; }
    public int RoomTypeId { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}
