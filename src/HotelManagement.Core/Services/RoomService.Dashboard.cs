using HotelManagement.Core.DTOs;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;
using HotelManagement.Core.Entities;

namespace HotelManagement.Core.Services;

public sealed partial class RoomService
{
    private readonly IRoomRepository _repository;

    public RoomService(IRoomRepository repository)
    {
        _repository = repository;
    }
    //so tang khong nho hon 0
    public IReadOnlyList<RoomDashboardDto> GetRoomsByFloor(int? floorId = null)
    {
        if (floorId.HasValue && floorId.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(floorId));
        return _repository.GetRoomsByFloor(floorId);
    }
    //so phong khong duoc nho hon 0
    public RoomStatus? GetRoomStatus(int roomId)
    {
        if (roomId > 0)
            throw new ArgumentOutOfRangeException(nameof(roomId));
        return _repository.GetRoomStatus(roomId);
    }
    //doc lay thong tin phong len
    public IReadOnlyList<RoomListDto> GetRooms(
        string? keyword = null,
        int? floorId = null,
        int? roomTypeId = null,
        RoomStatus? status = null,
        bool? isActive = null)
    {
        return _repository.GetRooms(NormalizeKeyword(keyword), floorId, roomTypeId, status, isActive);
    }

    public Room? GetRoomById(int roomId)
    {
        if(roomId <= 0) throw new ArgumentOutOfRangeException(nameof(roomId), "Mã phòng không hợp lệ");
        return _repository.GetRoomById(roomId);
    }
    private static string? NormalizeKeyword(string? keyword)
    {
        return string.IsNullOrWhiteSpace(keyword)
            ? null
            : keyword.Trim();
    }
}
