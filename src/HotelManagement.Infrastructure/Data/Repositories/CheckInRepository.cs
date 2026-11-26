using System.Data;
using System.Globalization;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;
using Microsoft.Data.Sqlite;
using HotelManagement.Infrastructure.Data;

namespace HotelManagement.Infrastructure.Data.Repositories;
public sealed class CheckInRepository(Func<SqliteConnection> createConnection) : ICheckInRepository
{
    public T Read<T>(Func<ICheckInSession, T> work)
    {
        using var connection = Open();
        return work(new Session(connection, null));
    }

    public T Write<T>(Func<ICheckInSession, T> work)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var result = work(new Session(connection, transaction));
        transaction.Commit();
        return result;
    }

    private SqliteConnection Open()
    {
        var connection = createConnection();
        if (connection.State != ConnectionState.Open) connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
        return connection;
    }

    private sealed class Session(SqliteConnection connection, SqliteTransaction? transaction) : ICheckInSession
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        private const string ReservationSql = """
            SELECT r.ReservationId, r.ReservationCode, r.CustomerId, c.FullName, c.Phone,
                   r.RoomTypeId, t.TypeName, r.RoomId, rm.RoomCode,
                   r.ExpectedCheckIn, r.ExpectedCheckOut, r.DepositAmount, r.Note,
                   r.Status, r.CreatedAt, s.StayId
            FROM Reservation r
            JOIN Customer c ON c.CustomerId = r.CustomerId
            JOIN RoomType t ON t.RoomTypeId = r.RoomTypeId
            LEFT JOIN Room rm ON rm.RoomId = r.RoomId
            LEFT JOIN Stay s ON s.ReservationId = r.ReservationId
            """;

        private const string RoomSql = """
            SELECT r.RoomId, r.RoomCode, f.FloorName, r.Status, t.RoomTypeId, t.TypeCode, t.TypeName,
                   t.MaxOccupancy, t.HourlyRate, t.DailyRate,
                   (r.IsActive = 1 AND f.IsActive = 1 AND t.IsActive = 1)
            FROM Room r
            JOIN Floor f ON f.FloorId = r.FloorId
            JOIN RoomType t ON t.RoomTypeId = r.RoomTypeId
            """;

        private const string CustomerSql = """
            SELECT CustomerId, FullName, IdentityType, IdentityNumber, Phone,
                   Email, Address, CreatedAt, IsActive
            FROM Customer
            """;

        public IReadOnlyList<CheckInReservationDto> GetReservations(ReservationStatus? status, string? keyword)
        {
            var rows = Query(
                ReservationSql + " WHERE ($status IS NULL OR r.Status = $status) ORDER BY r.ExpectedCheckIn",
                MapReservation, ("$status", status?.ToString()));
            if (string.IsNullOrWhiteSpace(keyword)) return rows;
            return rows.Where(r => Contains(r.ReservationCode, keyword) || Contains(r.CustomerName, keyword)
                                || Contains(r.Phone, keyword) || Contains(r.RoomCode, keyword)).ToList();
        }

        public CheckInReservationDto? GetReservation(int reservationId) =>
            Query(ReservationSql + " WHERE r.ReservationId = $id", MapReservation, ("$id", reservationId))
                .FirstOrDefault();

        public IReadOnlyList<CheckInRoomDto> GetRooms() => Query(RoomSql + " ORDER BY r.RoomCode", MapRoom);

        public CheckInRoomDto? GetRoom(int roomId) =>
            Query(RoomSql + " WHERE r.RoomId = $id", MapRoom, ("$id", roomId)).FirstOrDefault();
        public Customer? GetCustomer(int customerId) =>
            Query(CustomerSql + " WHERE CustomerId = $id", MapCustomer, ("$id", customerId)).FirstOrDefault();

        public IReadOnlyList<Customer> SearchCustomers(string keyword) =>
            Query(CustomerSql + " WHERE IsActive = 1 ORDER BY FullName", MapCustomer)
                .Where(c => keyword.Length == 0 || Contains(c.FullName, keyword)
                         || Contains(c.Phone, keyword) || Contains(c.IdentityNumber, keyword))
                .Take(100).ToList();

        public bool IdentityExists(IdentityType type, string number) => Count(
            "SELECT COUNT(*) FROM Customer WHERE IdentityType = $type AND UPPER(IdentityNumber) = UPPER($number)",
            ("$type", type.ToString()), ("$number", number)) > 0;

        public int InsertCustomer(Customer c) => Insert("""
            INSERT INTO Customer (FullName, IdentityType, IdentityNumber, Phone, Email, Address, CreatedAt, IsActive)
            VALUES ($name, $type, $number, $phone, $email, $address, $created, $active);
            """,
            ("$name", c.FullName), ("$type", c.IdentityType.ToString()), ("$number", c.IdentityNumber),
            ("$phone", c.Phone), ("$email", c.Email), ("$address", c.Address),
            ("$created", Text(c.CreatedAt)), ("$active", c.IsActive ? 1 : 0));
        public bool HasActiveStay(int roomId) => Count(
            "SELECT COUNT(*) FROM Stay WHERE RoomId = $room AND Status IN ('InProgress','CheckoutPending')",
            ("$room", roomId)) > 0;

        public bool HasRoomBookingOverlap(int roomId, DateTime start, DateTime end, int? excludeReservationId) =>
            Count("""
                SELECT COUNT(*) FROM Reservation
                WHERE RoomId = $room AND Status = 'Confirmed'
                  AND ReservationId <> IFNULL($exclude, -1)
                  AND ExpectedCheckIn < $end AND ExpectedCheckOut > $start
                """,
                ("$room", roomId), ("$exclude", excludeReservationId),
                ("$start", Text(start)), ("$end", Text(end))) > 0;

        public int CountUnassignedBookings(int roomTypeId, DateTime start, DateTime end, int? excludeReservationId) =>
            (int)Count("""
                SELECT COUNT(*) FROM Reservation
                WHERE RoomId IS NULL AND RoomTypeId = $type AND Status = 'Confirmed'
                  AND ReservationId <> IFNULL($exclude, -1)
                  AND ExpectedCheckIn < $end AND ExpectedCheckOut > $start
                """,
                ("$type", roomTypeId), ("$exclude", excludeReservationId),
                ("$start", Text(start)), ("$end", Text(end)));

        public bool HasPendingBooking(int roomId, DateTime now) => Count(
            "SELECT COUNT(*) FROM Reservation WHERE RoomId = $room AND Status = 'Confirmed' AND ExpectedCheckOut > $now",
            ("$room", roomId), ("$now", Text(now))) > 0;
        public int InsertStay(Stay s) => Insert("""
            INSERT INTO Stay (ReservationId, CustomerId, RoomId, ActualCheckIn, ExpectedCheckOut, ActualCheckOut,
                              GuestCount, HourlyRateAtCheckIn, DailyRateAtCheckIn, Status, Note)
            VALUES ($reservation, $customer, $room, $in, $out, NULL, $guests, $hourly, $daily, $status, $note);
            """,
            ("$reservation", s.ReservationId), ("$customer", s.CustomerId), ("$room", s.RoomId),
            ("$in", Text(s.ActualCheckIn)), ("$out", Text(s.ExpectedCheckOut)), ("$guests", s.GuestCount),
            ("$hourly", SqliteValueConverter.ToVnd(s.HourlyRateAtCheckIn)),
            ("$daily", SqliteValueConverter.ToVnd(s.DailyRateAtCheckIn)),
            ("$status", s.Status.ToString()), ("$note", s.Note));

        public bool MarkCheckedIn(int reservationId, int roomId) => Execute(
            "UPDATE Reservation SET Status = 'CheckedIn', RoomId = $room WHERE ReservationId = $id AND Status = 'Confirmed'",
            ("$id", reservationId), ("$room", roomId)) == 1;

        public bool MarkCancelled(int reservationId, string? note) => Execute(
            "UPDATE Reservation SET Status = 'Cancelled', Note = $note WHERE ReservationId = $id AND Status = 'Confirmed'",
            ("$id", reservationId), ("$note", note)) == 1;

        public bool MarkRoomOccupied(int roomId) => Execute(
            "UPDATE Room SET Status = 'Occupied' WHERE RoomId = $id AND Status IN ('Available','Reserved')",
            ("$id", roomId)) == 1;

        public void SetRoomStatus(int roomId, RoomStatus status) => Execute(
            "UPDATE Room SET Status = $status WHERE RoomId = $id",
            ("$id", roomId), ("$status", status.ToString()));
        private static CheckInReservationDto MapReservation(SqliteDataReader r) => new(
            r.GetInt32(0), r.GetString(1), r.GetInt32(2), r.GetString(3), r.GetString(4),
            r.GetInt32(5), r.GetString(6), NullableInt(r, 7), NullableText(r, 8),
            Date(r, 9), Date(r, 10), SqliteValueConverter.FromVnd(r, 11), NullableText(r, 12),
            Enum.Parse<ReservationStatus>(r.GetString(13)), Date(r, 14), NullableInt(r, 15));

        private static CheckInRoomDto MapRoom(SqliteDataReader r) => new(
            r.GetInt32(0), r.GetString(1), r.GetString(2), Enum.Parse<RoomStatus>(r.GetString(3)),
            r.GetInt32(4), r.GetString(5), r.GetString(6), r.GetInt32(7),
            SqliteValueConverter.FromVnd(r, 8), SqliteValueConverter.FromVnd(r, 9), r.GetBoolean(10));

        private static Customer MapCustomer(SqliteDataReader r) => new()
        {
            CustomerId = r.GetInt32(0),
            FullName = r.GetString(1),
            IdentityType = Enum.Parse<IdentityType>(r.GetString(2)),
            IdentityNumber = r.GetString(3),
            Phone = r.GetString(4),
            Email = NullableText(r, 5),
            Address = NullableText(r, 6),
            CreatedAt = Date(r, 7),
            IsActive = r.GetBoolean(8)
        };
        private SqliteCommand Command(string sql, (string Name, object? Value)[] parameters)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            foreach (var (name, value) in parameters)
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            return command;
        }

        private List<T> Query<T>(string sql, Func<SqliteDataReader, T> map, params (string, object?)[] parameters)
        {
            using var command = Command(sql, parameters);
            using var reader = command.ExecuteReader();
            var list = new List<T>();
            while (reader.Read()) list.Add(map(reader));
            return list;
        }

        private int Execute(string sql, params (string, object?)[] parameters)
        {
            using var command = Command(sql, parameters);
            return command.ExecuteNonQuery();
        }

        private long Count(string sql, params (string, object?)[] parameters)
        {
            using var command = Command(sql, parameters);
            return (long)command.ExecuteScalar()!;
        }

        private int Insert(string sql, params (string, object?)[] parameters) =>
            checked((int)Count(sql + " SELECT last_insert_rowid();", parameters));

        private static bool Contains(string? source, string keyword) =>
            source?.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) == true;

        private static string Text(DateTime value) => value.ToString(DateFormat, CultureInfo.InvariantCulture);
        private static DateTime Date(SqliteDataReader r, int i) =>
            DateTime.ParseExact(r.GetString(i), DateFormat, CultureInfo.InvariantCulture);
        private static int? NullableInt(SqliteDataReader r, int i) => r.IsDBNull(i) ? null : r.GetInt32(i);
        private static string? NullableText(SqliteDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);
    }
}
