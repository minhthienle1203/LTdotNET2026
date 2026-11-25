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
    // lấy sẵn mã phòng trước khi chỉnh sửa
    public IReadOnlyList<RoomType> GetActiveRoomTypes()
    {
        return _repository.GetActiveRoomTypes();
    }
    // tạo phòng mới. mặc định trạng thái Avaiable
    public int CreateRoom(Room room)
    {
        ValidateRoom(room);
        if (_repository.IsRoomCodeExists(room.RoomCode))
            throw new InvalidOperationException("Mã phòng đã tồn tại");
        if (!_repository.IsFloorActive(room.FloorId))
            throw new InvalidOperationException("Tầng được chọn hiện không thể sử dụng");
        if (!_repository.IsRoomTypeActive(room.RoomTypeId))
            throw new InvalidOperationException("Loại phòng được chọn hiện không thể sử dụng");
        room.Status = RoomStatus.Available;
        return _repository.InsertRoom(room);
    }

    // kiểm tra dữ liệu trước khi chuyển tới Repo
    private static void ValidateRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        room.RoomCode = (room.RoomCode ?? string.Empty).Trim();
        room.Note = string.IsNullOrWhiteSpace(room.Note) ? null : room.Note.Trim();

        if (room.RoomCode.Length == 0)
            throw new ArgumentException(" Mã phòng không được bỏ trống");
        if (room.RoomCode.Length > 30)
            throw new ArgumentException(" Mã phòng không dài quá 30 kí tự ");
        if (room.FloorId <= 0)
            throw new ArgumentException(" Phải chọn tầng");
        if (room.RoomTypeId <= 0)
            throw new ArgumentException(" Phải chọn loại phòng");
    }
    // cập nhật thông tin phòng đang có
    public void UpdateRoom(Room room)
    {
        ValidateRoom(room);
        if (room.RoomId <= 0)
            throw new ArgumentException("Mã phòng không hợp lệ");
        var current = _repository.GetRoomById(room.RoomId)
                      ?? throw new InvalidOperationException("Không tìm thấy phòng cần cập nhật");
        if (_repository.IsRoomCodeExists(room.RoomCode, room.RoomId))
            throw new InvalidOperationException(" Mã phòng bị trùng với phòng khác");
        if (!_repository.IsFloorActive(room.FloorId))
            throw new InvalidOperationException("Tầng được chọn hiện không khả dụng hoặc không tồn tại ");
        if (!_repository.IsRoomTypeActive(room.RoomTypeId))
            throw new InvalidOperationException("Loại phòng được chọn hiện không khả dụng hoặc không tồn tại ");
        //ngăn thay đổi trạng thái phòng đang có khách
        if (!room.IsActive && current.Status != RoomStatus.Available)
            throw new InvalidOperationException(" Không được ngưng sử dụng phòng này khi phòng đang trống");
        room.Status = current.Status;
        _repository.UpdateRoom(room);
    }
    //gọi danh sách phòng đang dọn
    public IReadOnlyList<RoomListDto> GetRoomsForCleaning()
    {
        return _repository.GetRoomsForCleaning();
    }
    //đổi trạng thái thành đã dọn
    public void CompleteCleaning(int roomId)
    {
        _repository.CompleteCleaning(roomId);
    }
}
