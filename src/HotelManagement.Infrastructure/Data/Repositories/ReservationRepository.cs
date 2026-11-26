using System.Globalization;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly DbConnectionFactory connectionFactory;

    public ReservationRepository(DbConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public List<ReservationListItemDto> Search(ReservationFilter filter)
    {
        const string sql = @"
SELECT r.ReservationId, r.ReservationCode, c.FullName, c.Phone,
       r.ExpectedCheckIn, r.ExpectedCheckOut, rt.TypeName,
       r.RoomId, rm.RoomCode, r.DepositAmount, r.Status, r.Note,
       rt.HourlyRate, rt.DailyRate
FROM Reservation r
JOIN Customer c ON c.CustomerId = r.CustomerId
JOIN RoomType rt ON rt.RoomTypeId = r.RoomTypeId
LEFT JOIN Room rm ON rm.RoomId = r.RoomId
WHERE ($keyword IS NULL OR r.ReservationCode LIKE $like OR c.FullName LIKE $like OR c.Phone LIKE $like)
  AND ($from IS NULL OR r.ExpectedCheckIn >= $from)
  AND ($to IS NULL OR r.ExpectedCheckIn < $to)
  AND ($roomTypeId IS NULL OR r.RoomTypeId = $roomTypeId)
  AND ($status IS NULL OR r.Status = $status)
ORDER BY r.ExpectedCheckIn;";

        using var connection = connectionFactory.CreateConnection();
        connection.Open();


        using var command = CreateCommand(connection, null, sql,
            ("$keyword", filter.Keyword),
            ("$like", filter.Keyword == null ? null : $"%{filter.Keyword}%"),
            ("$from", filter.FromDate == null ? null : FormatDate(filter.FromDate.Value)),
            ("$to", filter.ToDate == null ? null : FormatDate(filter.ToDate.Value.AddDays(1))),
            ("$roomTypeId", filter.RoomTypeId),
            ("$status", filter.Status?.ToString()));

        var list = new List<ReservationListItemDto>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ReservationListItemDto
            {
                ReservationId = reader.GetInt32(0),
                ReservationCode = reader.GetString(1),
                CustomerName = reader.GetString(2).Trim(),
                CustomerPhone = reader.GetString(3),
                ExpectedCheckIn = ParseDate(reader.GetString(4)),
                ExpectedCheckOut = ParseDate(reader.GetString(5)),
                RoomTypeName = reader.GetString(6),
                RoomId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                RoomCode = reader.IsDBNull(8) ? null : reader.GetString(8),
                DepositAmount = SqliteValueConverter.FromVnd(reader, 9),
                Status = Enum.Parse<ReservationStatus>(reader.GetString(10)),
                Note = reader.IsDBNull(11) ? null : reader.GetString(11),
                HourlyRate = SqliteValueConverter.FromVnd(reader, 12),
                DailyRate = SqliteValueConverter.FromVnd(reader, 13)
            });
        }

        return list;
    }



    public List<RoomTypeOptionDto> GetRoomTypeOptions(bool activeOnly)
    {
        const string sql = @"
SELECT RoomTypeId, TypeName, MaxOccupancy, HourlyRate, DailyRate
FROM RoomType
WHERE ($activeOnly = 0 OR IsActive = 1)
ORDER BY TypeName;";

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, null, sql, ("$activeOnly", activeOnly ? 1 : 0));

        var list = new List<RoomTypeOptionDto>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new RoomTypeOptionDto
            {
                RoomTypeId = reader.GetInt32(0),
                TypeName = reader.GetString(1),
                MaxOccupancy = reader.GetInt32(2),
                HourlyRate = SqliteValueConverter.FromVnd(reader, 3),
                DailyRate = SqliteValueConverter.FromVnd(reader, 4)
            });
        }

        return list;
    }

    public List<CustomerOptionDto> GetActiveCustomers()
    {
        const string sql = @"
SELECT CustomerId, FullName, IdentityType, IdentityNumber, Phone, Email
FROM Customer
WHERE IsActive = 1
ORDER BY FullName;";

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, null, sql);

        var list = new List<CustomerOptionDto>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new CustomerOptionDto
            {
                CustomerId = reader.GetInt32(0),
                FullName = reader.GetString(1).Trim(),
                IdentityText = $"{reader.GetString(2)} {reader.GetString(3)}",
                Phone = reader.GetString(4),
                Email = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return list;
    }


    public List<RoomOptionDto> GetFreeRooms(int roomTypeId, DateTime from, DateTime to, int? excludeReservationId)
    {
        const string sql = @"
SELECT rm.RoomId, rm.RoomCode, rm.Status
FROM Room rm
WHERE rm.RoomTypeId = $roomTypeId AND rm.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM Reservation r
                  WHERE r.RoomId = rm.RoomId AND r.Status = 'Confirmed'
                    AND r.ReservationId <> $excludeId
                    AND r.ExpectedCheckIn < $to AND r.ExpectedCheckOut > $from)
  AND NOT EXISTS (SELECT 1 FROM Stay s
                  WHERE s.RoomId = rm.RoomId AND s.Status IN ('InProgress', 'CheckoutPending')
                    AND s.ActualCheckIn < $to AND MAX(s.ExpectedCheckOut, $now) > $from)
ORDER BY rm.RoomCode;";

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, null, sql,
            ("$roomTypeId", roomTypeId),
            ("$excludeId", excludeReservationId ?? 0),
            ("$from", FormatDate(from)),
            ("$to", FormatDate(to)),
            ("$now", FormatDate(DateTime.Now)));

        var list = new List<RoomOptionDto>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new RoomOptionDto
            {
                RoomId = reader.GetInt32(0),
                RoomCode = reader.GetString(1),
                Status = Enum.Parse<RoomStatus>(reader.GetString(2))
            });
        }

        return list;
    }

    public int CountActiveRooms(int roomTypeId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, null,
            "SELECT COUNT(*) FROM Room WHERE RoomTypeId = $roomTypeId AND IsActive = 1;",
            ("$roomTypeId", roomTypeId));
        return Convert.ToInt32(command.ExecuteScalar());
    }


    public int CountUnassignedReservations(int roomTypeId, DateTime from, DateTime to, int? excludeReservationId)
    {
        const string sql = @"
SELECT COUNT(*) FROM Reservation
WHERE RoomTypeId = $roomTypeId AND RoomId IS NULL AND Status = 'Confirmed'
  AND ReservationId <> $excludeId
  AND ExpectedCheckIn < $to AND ExpectedCheckOut > $from;";

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, null, sql,
            ("$roomTypeId", roomTypeId),
            ("$excludeId", excludeReservationId ?? 0),
            ("$from", FormatDate(from)),
            ("$to", FormatDate(to)));
        return Convert.ToInt32(command.ExecuteScalar());
    }


    public Reservation? GetById(int reservationId)
    {
        const string sql = @"
SELECT ReservationId, ReservationCode, CustomerId, RoomTypeId, RoomId,
       ExpectedCheckIn, ExpectedCheckOut, DepositAmount, Note, Status, CreatedAt
FROM Reservation
WHERE ReservationId = $id;";

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var command = CreateCommand(connection, null, sql, ("$id", reservationId));

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new Reservation
        {
            ReservationId = reader.GetInt32(0),
            ReservationCode = reader.GetString(1),
            CustomerId = reader.GetInt32(2),
            RoomTypeId = reader.GetInt32(3),
            RoomId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            ExpectedCheckIn = ParseDate(reader.GetString(5)),
            ExpectedCheckOut = ParseDate(reader.GetString(6)),
            DepositAmount = SqliteValueConverter.FromVnd(reader, 7),
            Note = reader.IsDBNull(8) ? null : reader.GetString(8),
            Status = Enum.Parse<ReservationStatus>(reader.GetString(9)),
            CreatedAt = ParseDate(reader.GetString(10))
        };
    }

    public int Insert(Reservation r)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var prefix = $"RSV-{DateTime.Now:yyyyMM}-";
        int next;
        using (var command = CreateCommand(connection, transaction,
                   "SELECT COALESCE(MAX(CAST(SUBSTR(ReservationCode, $start) AS INTEGER)), 0) + 1 " +
                   "FROM Reservation WHERE ReservationCode LIKE $prefix || '%';",
                   ("$start", prefix.Length + 1), ("$prefix", prefix)))
        {
            next = Convert.ToInt32(command.ExecuteScalar());
        }

        const string insertSql = @"
INSERT INTO Reservation (ReservationCode, CustomerId, RoomTypeId, RoomId, ExpectedCheckIn,
                         ExpectedCheckOut, DepositAmount, Note, Status, CreatedAt)
VALUES ($code, $customerId, $roomTypeId, $roomId, $checkIn, $checkOut, $deposit, $note, $status, $createdAt);";

        using (var command = CreateCommand(connection, transaction, insertSql,
                   ("$code", prefix + next.ToString("D3")),
                   ("$customerId", r.CustomerId),
                   ("$roomTypeId", r.RoomTypeId),
                   ("$roomId", r.RoomId),
                   ("$checkIn", FormatDate(r.ExpectedCheckIn)),
                   ("$checkOut", FormatDate(r.ExpectedCheckOut)),
                   ("$deposit", SqliteValueConverter.ToVnd(r.DepositAmount)),
                   ("$note", r.Note),
                   ("$status", r.Status.ToString()),
                   ("$createdAt", FormatDate(r.CreatedAt))))
        {
            command.ExecuteNonQuery();
        }

        int newId;
        using (var command = CreateCommand(connection, transaction, "SELECT last_insert_rowid();"))
        {
            newId = Convert.ToInt32(command.ExecuteScalar());
        }

        if (r.RoomId.HasValue)
            MarkRoomReserved(connection, transaction, r.RoomId.Value);

        transaction.Commit();
        return newId;
    }

    public bool Update(Reservation r)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();


        int? oldRoomId;
        using (var command = CreateCommand(connection, transaction,
                   "SELECT RoomId FROM Reservation WHERE ReservationId = $id AND Status = 'Confirmed';",
                   ("$id", r.ReservationId)))
        {
            var result = command.ExecuteScalar();
            if (result == null)
                return false;
            oldRoomId = result is DBNull ? null : Convert.ToInt32(result);
        }

        const string updateSql = @"
UPDATE Reservation
SET CustomerId = $customerId, RoomTypeId = $roomTypeId, RoomId = $roomId,
    ExpectedCheckIn = $checkIn, ExpectedCheckOut = $checkOut,
    DepositAmount = $deposit, Note = $note
WHERE ReservationId = $id AND Status = 'Confirmed';";

        using (var command = CreateCommand(connection, transaction, updateSql,
                   ("$id", r.ReservationId),
                   ("$customerId", r.CustomerId),
                   ("$roomTypeId", r.RoomTypeId),
                   ("$roomId", r.RoomId),
                   ("$checkIn", FormatDate(r.ExpectedCheckIn)),
                   ("$checkOut", FormatDate(r.ExpectedCheckOut)),
                   ("$deposit", SqliteValueConverter.ToVnd(r.DepositAmount)),
                   ("$note", r.Note)))
        {
            command.ExecuteNonQuery();
        }

        if (oldRoomId != r.RoomId)
        {
            if (oldRoomId.HasValue)
                RecalculateRoomStatus(connection, transaction, oldRoomId.Value);
            if (r.RoomId.HasValue)
                MarkRoomReserved(connection, transaction, r.RoomId.Value);
        }

        transaction.Commit();
        return true;
    }

    public bool Cancel(int reservationId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        int? roomId;
        using (var command = CreateCommand(connection, transaction,
                   "SELECT RoomId FROM Reservation WHERE ReservationId = $id AND Status = 'Confirmed';",
                   ("$id", reservationId)))
        {
            var result = command.ExecuteScalar();
            if (result == null)
                return false;
            roomId = result is DBNull ? null : Convert.ToInt32(result);
        }

        using (var command = CreateCommand(connection, transaction,
                   "UPDATE Reservation SET Status = 'Cancelled' WHERE ReservationId = $id;",
                   ("$id", reservationId)))
        {
            command.ExecuteNonQuery();
        }

        if (roomId.HasValue)
            RecalculateRoomStatus(connection, transaction, roomId.Value);

        transaction.Commit();
        return true;
    }


    private static void MarkRoomReserved(SqliteConnection connection, SqliteTransaction transaction, int roomId)
    {
        using var command = CreateCommand(connection, transaction,
            "UPDATE Room SET Status = 'Reserved' WHERE RoomId = $roomId AND Status = 'Available';",
            ("$roomId", roomId));
        command.ExecuteNonQuery();
    }


    private static void RecalculateRoomStatus(SqliteConnection connection, SqliteTransaction transaction, int roomId)
    {
        const string sql = @"
UPDATE Room
SET Status = CASE WHEN EXISTS (SELECT 1 FROM Reservation WHERE RoomId = $roomId AND Status = 'Confirmed')
                  THEN 'Reserved' ELSE 'Available' END
WHERE RoomId = $roomId AND Status IN ('Reserved', 'Available');";

        using var command = CreateCommand(connection, transaction, sql, ("$roomId", roomId));
        command.ExecuteNonQuery();
    }


    private static SqliteCommand CreateCommand(SqliteConnection connection, SqliteTransaction? transaction,
        string sql, params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }

    private static string FormatDate(DateTime value)
        => value.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static DateTime ParseDate(string text)
        => DateTime.ParseExact(text, DateFormat, CultureInfo.InvariantCulture);
}
