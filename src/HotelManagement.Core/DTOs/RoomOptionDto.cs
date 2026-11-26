using HotelManagement.Core.Enums;

namespace HotelManagement.Core.DTOs;

public class RoomOptionDto
{
    public int? RoomId { get; set; }
    public string? RoomCode { get; set; }
    public RoomStatus? Status { get; set; }

    public string Text => RoomCode == null ? "(Không gán phòng cụ thể)" : $"Phòng {RoomCode} ({Status})";
}

