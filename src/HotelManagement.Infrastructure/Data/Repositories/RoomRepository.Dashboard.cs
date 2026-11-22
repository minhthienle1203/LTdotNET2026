using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;
using HotelManagement.Infrastructure.Data;

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
            isActive.HasValue ? (object)(isActive.Value ? 1 : 0) : DBNull.Value);
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
        command.Parameters.AddWithValue("$RoomId", roomId);
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

}
