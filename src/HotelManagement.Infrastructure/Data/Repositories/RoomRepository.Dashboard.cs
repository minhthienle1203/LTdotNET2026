using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed partial class RoomRepository : IRoomRepository
{
    private readonly DbConnectionFactory _connectionFactory;
    public RoomRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    //lay phong theo tang
    public List<RoomDashboardDto> GetRoomsByFloor(int? floorId)
    {
        const string sql = @"SELECT r.RoomId, r.RoomCode, r.FloorId, f.FloorNumber, f.FloorName,
                                    r.RoomTypeId, rt.TypeName, rt.HourlyRate, rt.DailyRate, r.Status, r.Note,
                                    (SELECT rs.ReservationId
                                     FROM Reservation rs
                                     WHERE rs.RoomId = rs.RoomId AND rs.Status = 'Confirmed' AND rs.ExpectedCheckOut > datetime('now','localtime')
                                     ORDER BY rs.ExpectedCheckIn, rs.ReservationId
                                     LIMIT 1) AS ReservationId,
                                     (SELECT st.StayId
                                     FROM Stay st
                                     WHERE st.RoomId = r.RoomId AND st.Status IN ('InProgress', 'CheckOutPending')
                                     ORDER BY st.ActualCheckIn DESC, st.StayId DESC
                                     LIMIT 1
                                     ) AS StayId,
                                     (SELECT st.Status
                                     FROM Stay st
                                     WHERE st.RoomId = r.RoomId AND st.Status IN ('InProgress', 'CheckOutPending')
                                     ORDER BY st.ActualCheckIn DESC, st.StayId DESC
                                     LIMIT 1
                                     ) AS StayStatus

                            FROM Room r INNER JOIN Floor f ON f.FloorId = r.FloorId
                                        INNER JOIN RoomType rt ON rt.RoomTypeId = r.RoomTypeId
                            WHERE r.IsActive = 1 AND f.IsActive = 1 AND($floorId IS NULL OR r.FloorId = $floorId)
                            ORDER BY f.SortOrder, f.FloorNumber, r.RoomCode;";
        var rooms = new List<RoomDashboardDto>();
        using var connection = _connectionFactory.CreateConnection() ;
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$floorId", (object?) floorId ??  DBNull.Value);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rooms.Add(new RoomDashboardDto
            {
                RoomId = reader.GetInt32(0),
                RoomCode = reader.GetString(1),
                FloorId = reader.GetInt32(2),
                FloorNumber =  reader.GetInt32(3),
                FloorName = reader.GetString(4),
                RoomTypeId = reader.GetInt32(5),
                TypeName = reader.GetString(6),
                HourlyRate = SqliteValueConverter.FromVnd(reader,7),
                DailyRate = SqliteValueConverter.FromVnd(reader,8),
                Status = Enum.Parse<RoomStatus>(reader.GetString(9), ignoreCase: false),
                Note = reader.IsDBNull(10) ? null : reader.GetString(10),
                ReservationId =  reader.IsDBNull(11) ? null : reader.GetInt32(11),
                StayId = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                StayStatus = reader.IsDBNull(13) ? null : Enum.Parse<StayStatus>(reader.GetString(13), ignoreCase: false)
            });
        }
        return rooms;
    }
    // lay phong theo trang thai
    public RoomStatus? GetRoomStatus(int roomId)
    {
        const string sql = "SELECT Status FROM Room WHERE RoomId = $roomId LIMIT 1;";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$roomId", roomId);
        object? value = command.ExecuteScalar();

        if (value is null || value == DBNull.Value)
            return null;
        return Enum.Parse<RoomStatus>(Convert.ToString(value)!, ignoreCase: false);
    }
    //lay phong cho trang quan ly
    public List<RoomListDto> GetRooms(string? keyword, int? floorId, int? roomTypeId, RoomStatus? status,
        bool? isActive)
    {
        const string sql = @"SELECT r.RoomId, r.RoomCode, r.FloorId, f.FloorNumber, f.FloorName,
                                r.RoomTypeId, rt.TypeCode, rt.TypeName, r.Status, r.Note, r.IsActive,
                                rt.HourlyRate, rt.DailyRate
                                FROM Room r
                                INNER JOIN Floor f ON f.FloorId = r.FloorId
                                INNER JOIN RoomType rt ON rt.RoomTypeId = r.RoomTypeId
                                WHERE ($keyword IS NULL OR r.RoomCode LIKE '%' || $keyword || '%')
                                AND ($floorId IS NULL OR r.FloorId = $floorId)
                                AND ($roomTypeId IS NULL OR r.RoomTypeId = $roomTypeId)
                                AND ($status IS NULL OR r.Status = $status)
                                AND ($isActive IS NULL OR r.IsActive = $isActive)
                                ORDER BY f.SortOrder, f.FloorNumber, r.RoomCode;";

        var result = new List<RoomListDto>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$keyword", (object?)keyword ?? DBNull.Value);
        command.Parameters.AddWithValue("$floorId", (object?)floorId ?? DBNull.Value);
        command.Parameters.AddWithValue("$roomTypeId", (object?)roomTypeId ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", (object?)status?.ToString() ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive",
            isActive.HasValue ? isActive.Value ? 1 : 0 : DBNull.Value);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new RoomListDto
            {
                RoomId = reader.GetInt32(0),
                RoomCode = reader.GetString(1),
                FloorId = reader.GetInt32(2),
                FloorNumber = reader.GetInt32(3),
                FloorName = reader.GetString(4),
                RoomTypeId = reader.GetInt32(5),
                TypeCode =  reader.GetString(6),
                TypeName = reader.GetString(7),
                Status = Enum.Parse<RoomStatus>(reader.GetString(8)),
                Note  = reader.IsDBNull(9) ? null : reader.GetString(9),
                IsActive = reader.GetInt64(10) == 1,
                HourlyRate = SqliteValueConverter.FromVnd(reader, 11),
                DailyRate = SqliteValueConverter.FromVnd(reader, 12),
            });
        }
        return result;
    }
    //lay room theo id
    public Room? GetRoomById(int roomId)
    {
        const string sql = @" SELECT RoomId, RoomCode, FloorId, RoomTypeId, Status, Note, IsActive
                            FROM Room
                            WHERE RoomId = $roomId LIMIT 1;";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$roomId", roomId);
        using var reader = command.ExecuteReader();
        if(!reader.Read()) return null;
        return new Room
        {
            RoomId = reader.GetInt32(0),
            RoomCode = reader.GetString(1),
            FloorId = reader.GetInt32(2),
            RoomTypeId = reader.GetInt32(3),
            Status = Enum.Parse<RoomStatus>(reader.GetString(4)),
            Note = reader.IsDBNull(5) ? null : reader.GetString(5),
            IsActive = reader.GetInt64(6) == 1
        };

    }
    // lay cac loai phong con hoat dong
    public List<RoomType> GetActiveRoomTypes()
    {
        const string sql = @" SELECT RoomTypeId, TypeCode, TypeName FROM RoomType WHERE IsActive = 1
    ORDER BY TypeName, TypeCode;";
        var re = new List<RoomType>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            re.Add(new RoomType
            {
                RoomTypeId = reader.GetInt32(0),
                TypeCode = reader.GetString(1),
                TypeName = reader.GetString(2),
            });
        }
        return re;
    }
    // kiem tra ma phong da ton tai chua, tru phong dang chon
    public bool IsRoomCodeExists(string roomCode, int? excludeRoomId = null)
    {
        const string sql = @"SELECT EXISTS (SELECT 1 FROM Room
                        WHERE RoomCode = $roomCode AND ($excludeRoomId IS NULL OR RoomId <> $excludeRoomId));";
        using var  connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$roomCode", roomCode);
        command.Parameters.AddWithValue("$excludeRoomId", (object?)excludeRoomId ?? DBNull.Value);
        return Convert.ToInt64(command.ExecuteScalar()) ==1;

    }
    // kiem tra tang con hoat dong khong
    public bool IsFloorActive(int floorId)
    {
        const string sql = @" SELECT EXISTS  (SELECT 1 FROM Floor WHERE FloorId = $id AND IsActive = 1);";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", floorId);
        return Convert.ToInt64(command.ExecuteScalar()) ==1;
    }
    // kiem loai phong con hoat dong khong
    public bool IsRoomTypeActive(int roomTypeId)
    {
        const string sql = @"SELECT EXISTS(SELECT 1 FROM RoomType WHERE RoomTypeId = $id AND IsActive = 1);";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$id", roomTypeId);
        return Convert.ToInt64(command.ExecuteScalar()) ==1;
    }
    // them phong vao db, mặc định là đang trống
    public int InsertRoom(Room room)
    {
        const string sql = @"INSERT INTO Room (RoomCode, FloorId, RoomTypeId, Status, Note, IsActive)
                    VALUES ($roomCode, $floorId, $roomTypeId, $status, $note, $isActive);";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$roomCode", room.RoomCode);
        command.Parameters.AddWithValue("$floorId", room.FloorId);
        command.Parameters.AddWithValue("$roomTypeId", room.RoomTypeId);
        command.Parameters.AddWithValue("$status", room.Status.ToString());
        command.Parameters.AddWithValue("$note", (object?)room.Note ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive", room.IsActive ? 1 :0);
        command.ExecuteNonQuery();
        // trả về id phòng mới thêm
        using var idCommand = connection.CreateCommand();
        idCommand.CommandText = "SELECT last_insert_rowid();";
        return Convert.ToInt32(idCommand.ExecuteScalar());

    }
    // cập nhật, sửa phòng mà không thay đổi trạng thái
    public void UpdateRoom(Room room)
    {
        const string sql = @" UPDATE Room
                        SET RoomCode = $roomCode,
                            FloorId = $floorId,
                            RoomTypeId = $roomTypeId,
                            Note = $note,
                            IsActive = $isActive WHERE RoomId = $roomId;";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$roomCode", room.RoomCode);
        command.Parameters.AddWithValue("$floorId", room.FloorId);
        command.Parameters.AddWithValue("$roomTypeId", room.RoomTypeId);
        command.Parameters.AddWithValue("$note", (object?)room.Note ?? DBNull.Value);
        command.Parameters.AddWithValue("$isActive", room.IsActive ? 1 :0);
        command.Parameters.AddWithValue("$roomId", room.RoomId);

        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Lỗi cập nhật phòng, vui lòng thử lại sau!");
    }
    // lay danh sach phong dang don dep
    public List<RoomListDto> GetRoomsForCleaning()
    {
        const string sql = @" SELECT r.RoomId, r.RoomCode, r.FloorId, f.FloorNumber, f.FloorName, r.RoomTypeId, rt.TypeCode, rt.TypeName,
                            r.Status, r.Note, r.IsActive, rt.HourlyRate, rt.DailyRate
                            FROM  Room r INNER JOIN Floor f ON f.FloorId = r.FloorId
                                          INNER JOIN RoomType rt ON rt.RoomTypeId = r.RoomTypeId
                            WHERE r.Status = 'Cleaning' AND r.IsActive = 1
                            ORDER BY f.SortOrder, f.FloorNumber, r.RoomCode;";
        var re = new List<RoomListDto>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            re.Add(new RoomListDto
            {
                RoomId = reader.GetInt32(0),
                RoomCode = reader.GetString(1),
                FloorId = reader.GetInt32(2),
                FloorNumber = reader.GetInt32(3),
                FloorName = reader.GetString(4),
                RoomTypeId = reader.GetInt32(5),
                TypeCode = reader.GetString(6),
                TypeName = reader.GetString(7),
                Status = Enum.Parse<RoomStatus>(reader.GetString(8)),
                Note = reader.IsDBNull(9) ? null : reader.GetString(9),
                IsActive = reader.GetInt64(10 )== 1,
                HourlyRate = SqliteValueConverter.FromVnd(reader,11),
                DailyRate = SqliteValueConverter.FromVnd(reader,12)
            });
        }
        return re;
    }
    // chinh sua trang thai da don dep xong
    public void CompleteCleaning(int roomId)
    {
        const string nextStatus = "Available";
        const string sql = @" UPDATE Room
                              SET Status = $nextStatus
                              WHERE roomId = $roomId AND Status = 'Cleaning' AND IsActive = 1;";
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$nextStatus", nextStatus);
        command.Parameters.AddWithValue("$roomId", roomId);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("Trạng thái phòng đã thay đổi. Hãy tải lại danh sách");
    }
}
