using HotelManagement.Core.DTOs;
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
}
