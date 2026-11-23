namespace HotelManagement.WinForms.Common;

public sealed record RoomFormContext(
    int? RoomId = null,
    int? FloorId = null,
    int? RoomTypeId = null,
    RoomListContext? ReturnContext = null
);
