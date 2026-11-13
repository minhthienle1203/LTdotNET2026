namespace HotelManagement.Core.Entities;

public class Floor
{
    public int FloorId { get; set; }
    public int FloorNumber { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
