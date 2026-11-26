using System.Data;
using System.Globalization;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;
using Microsoft.Data.Sqlite;
using HotelManagement.Infrastructure.Data;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class ActiveStayRepository(Func<SqliteConnection> createConnection) : IActiveStayRepository
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public ActiveStaySnapshotDto Load(DateTime now)
    {
        using var connection = createConnection();
        if (connection.State != ConnectionState.Open) connection.Open();
        using var transaction = connection.BeginTransaction();

        var services = Query(connection, transaction, """
            SELECT ss.StayServiceId, ss.StayId, h.ServiceName, h.UnitName,
                   ss.Quantity, ss.UnitPrice, ss.Amount, ss.UsedAt
            FROM StayService ss
            JOIN Stay s ON s.StayId = ss.StayId
            JOIN HotelService h ON h.ServiceId = ss.ServiceId
            WHERE s.Status = 'InProgress'
            ORDER BY ss.UsedAt DESC, ss.StayServiceId DESC
            """,
            r => new StayServiceLineDto(r.GetInt32(0), r.GetInt32(1), r.GetString(2), r.GetString(3),
                SqliteValueConverter.FromDecimalText(r, 4),
                SqliteValueConverter.FromVnd(r, 5), SqliteValueConverter.FromVnd(r, 6), Date(r, 7)));

        var byStay = services.ToLookup(x => x.StayId);

        var stays = Query(connection, transaction, """
            SELECT s.StayId, s.ReservationId, r.ReservationCode,
                   rm.RoomId, rm.RoomCode, f.FloorId, f.FloorName, t.RoomTypeId, t.TypeName,
                   c.CustomerId, c.FullName, c.IdentityType, c.IdentityNumber, c.Phone,
                   s.ActualCheckIn, s.ExpectedCheckOut, s.GuestCount,
                   s.HourlyRateAtCheckIn, s.DailyRateAtCheckIn,
                   IFNULL(r.DepositAmount, 0), s.Note
            FROM Stay s
            JOIN Room rm ON rm.RoomId = s.RoomId
            JOIN Floor f ON f.FloorId = rm.FloorId
            JOIN RoomType t ON t.RoomTypeId = rm.RoomTypeId
            JOIN Customer c ON c.CustomerId = s.CustomerId
            LEFT JOIN Reservation r ON r.ReservationId = s.ReservationId
            WHERE s.Status = 'InProgress'
            ORDER BY rm.RoomCode
            """,
            r =>
            {
                var stayId = r.GetInt32(0);
                var lines = byStay[stayId].ToList();
                return new ActiveStayDto(
                    stayId, r.IsDBNull(1) ? null : r.GetInt32(1), r.IsDBNull(2) ? null : r.GetString(2),
                    r.GetInt32(3), r.GetString(4), r.GetInt32(5), r.GetString(6), r.GetInt32(7), r.GetString(8),
                    r.GetInt32(9), r.GetString(10), Enum.Parse<IdentityType>(r.GetString(11)),
                    r.GetString(12), r.GetString(13),
                    Date(r, 14), Date(r, 15), r.GetInt32(16),
                    SqliteValueConverter.FromVnd(r, 17), SqliteValueConverter.FromVnd(r, 18),
                    SqliteValueConverter.FromVnd(r, 19),
                    lines.Sum(x => x.Amount), lines.Count,
                    r.IsDBNull(20) ? null : r.GetString(20));
            });

        var floors = Query(connection, transaction,
            "SELECT FloorId, FloorName FROM Floor WHERE IsActive = 1 ORDER BY SortOrder, FloorNumber",
            r => new LookupItemDto(r.GetInt32(0), r.GetString(1)));
        var types = Query(connection, transaction,
            "SELECT RoomTypeId, TypeName FROM RoomType WHERE IsActive = 1 ORDER BY TypeName",
            r => new LookupItemDto(r.GetInt32(0), r.GetString(1)));
        var roomCount = Query(connection, transaction,
            "SELECT COUNT(*) FROM Room WHERE IsActive = 1", r => r.GetInt32(0))[0];
        var pending = Query(connection, transaction,
            "SELECT COUNT(*) FROM Stay WHERE Status = 'CheckoutPending'", r => r.GetInt32(0))[0];

        transaction.Commit();
        return new ActiveStaySnapshotDto(now, stays, services, floors, types, roomCount, pending);
    }

    private static List<T> Query<T>(SqliteConnection connection, SqliteTransaction transaction,
                                    string sql, Func<SqliteDataReader, T> map)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        var list = new List<T>();
        while (reader.Read()) list.Add(map(reader));
        return list;
    }

    private static DateTime Date(SqliteDataReader r, int ordinal) =>
        DateTime.ParseExact(r.GetString(ordinal), DateFormat, CultureInfo.InvariantCulture);
}
