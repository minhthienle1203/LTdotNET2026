using HotelManagement.Core.DTOs;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;

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
}
