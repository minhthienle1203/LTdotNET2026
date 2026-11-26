namespace HotelManagement.Core.DTOs;

public sealed class MonthlyReportDto
{
    public List<int> OccupiedDays { get; set; } = new ();
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysInMonth { get; set; }
    public int ActiveRoomCount { get; set; }
    //tổng số giờ phòng có khách trong 1 tháng
    public decimal OccupiedRoomHours { get; set; }
    //tổng giờ có thể khai thác = phòng*24*30
    public decimal TotalRoomHours { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal RoomRevenue { get; set; }
    public decimal ServiceRevenue { get; set; }
    public decimal TotalRevenue => RoomRevenue + ServiceRevenue;
}
