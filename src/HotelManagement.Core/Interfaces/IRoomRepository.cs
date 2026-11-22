using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;

namespace HotelManagement.Core.Interfaces;

public partial interface IRoomRepository
{
    List<RoomDashboardDto> GetRoomsByFloor(int? floorId);
    RoomStatus?  GetRoomStatus(int roomId);
    List<RoomListDto> GetRooms(string? keyword, int? floorId, int? roomTypeId, RoomStatus? status, bool? isActive);
    Room? GetRoomById(int roomId);
}
