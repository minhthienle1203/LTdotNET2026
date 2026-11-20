using HotelManagement.Core.DTOs;
using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Interfaces;

public partial interface IRoomRepository
{
    List<RoomDashboardDto> GetRoomsByFloor(int? floorId);
    RoomStatus?  GetRoomStatus(int roomId);
}
