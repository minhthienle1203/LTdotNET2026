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
    // lay cac loai phong con hoat dong
    List<RoomType> GetActiveRoomTypes();
    // kiem tra ma phong da ton tai chua
    bool IsRoomCodeExists(string roomCode, int? excludeRoomId = null);
    // kiem tra tang con hoat dong khong
    bool IsFloorActive(int roomTypeId);
    // kiem loai phong con hoat dong khong
    bool IsRoomTypeActive(int roomTypeId);
    // them phong vao db
    int InsertRoom(Room room);
    // cap nhat. chinh sua phong
    void UpdateRoom(Room room);
    // lay danh sach phong dang don dep
    List<RoomListDto> GetRoomsForCleaning();
    // chinh sua trang thai da don dep xong
    void CompleteCleaning(int roomId);
}
