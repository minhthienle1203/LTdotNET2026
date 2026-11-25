namespace HotelManagement.Core.DTOs;

public class RoomTypeOptionDto
{
    public int RoomTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int MaxOccupancy { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DailyRate { get; set; }

    public string Text => $"{TypeName} - {HourlyRate:N0} đ/giờ | {DailyRate:N0} đ/ngày (tối đa {MaxOccupancy} khách)";
}
