namespace HotelManagement.Core.Entities;

public class StayService
{
    public int StayServiceId { get; set; }
    public int StayId { get; set; }
    public int ServiceId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public DateTime UsedAt { get; set; }
    public string? Note { get; set; }
}
