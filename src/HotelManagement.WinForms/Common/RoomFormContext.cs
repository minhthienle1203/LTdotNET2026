namespace HotelManagement.WinForms.Common;

public sealed record RoomFormContext
{
    public int? RoomId = null;
    int? FloorId = null;
    int? RoomTypeId = null;
    public DashboardConText? ReturnContext = null;
}
