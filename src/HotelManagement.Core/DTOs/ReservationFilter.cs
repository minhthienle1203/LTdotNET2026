using HotelManagement.Core.Enums;

namespace HotelManagement.Core.DTOs;

public class ReservationFilter
{
    public string? Keyword { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? RoomTypeId { get; set; }
    public ReservationStatus? Status { get; set; }
}

