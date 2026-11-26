namespace HotelManagement.Core.DTOs;

public class AvailabilityDto
{
    public int TotalRooms { get; set; }
    public int FreeRooms { get; set; }
    public int HeldByType { get; set; }
    public int Available => FreeRooms - HeldByType;
    public bool IsAvailable => Available > 0;
}

