using System.Globalization;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Interfaces;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";
    private readonly DbConnectionFactory _connectionFactory;
    public ReportRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public MonthlyReportDto GetMonthlyReport(int year, int month, DateTime asOf)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        // chỉ lấy phòng nào còn kinh doanh
        const string roomCountSql = @"SELECT COUNT(*) FROM Room WHERE IsActive = 1;";

        using var roomCommand = connection.CreateCommand();
        roomCommand.CommandText = roomCountSql;
        int activeRoomCount = Convert.ToInt32(roomCommand.ExecuteScalar());

        // Stay chưa thanh toán thì asOf làm điểm kết thúc tạm
        // Min(...,asOf, monthEnd) tránh tính giờ tương lai hoặc vượt qua tháng khác
        const string occupiedHoursSql = @"WITH StaySegments AS
                                           (
                                           SELECT MAX(s.ActualCheckIn, $monthStart) AS SegmentStart,
                                                  MIN( COALESCE(s.ActualCheckOut, $asOf), $asOf, $monthEnd) AS SegmentEnd
                                            FROM Stay s INNER JOIN Room r ON r.RoomId = s.RoomId
                                            WHERE r.IsActive = 1 AND s.Status IN('InProgress', 'CheckoutPending' , 'Completed')
                                                                 AND s.ActualCheckIn < $monthEnd AND COALESCE(s.ActualCheckOut, $asOf) > $monthStart
                                            )
                                           SELECT COALESCE(
                                                SUM(
                                                    CASE
                                                        WHEN julianday(SegmentEnd) > julianday(SegmentStart)
                                                        THEN (julianday(SegmentEnd) - julianday(SegmentStart)) * 24.0
                                                        ELSE 0
                                                    END
                                            ),0)
                                            FROM StaySegments;";

        using var occupiedCommand = connection.CreateCommand();
        occupiedCommand.CommandText = occupiedHoursSql;
        occupiedCommand.Parameters.AddWithValue("$monthStart",
            monthStart.ToString(DateFormat, CultureInfo.InvariantCulture));
        occupiedCommand.Parameters.AddWithValue("$monthEnd",
            monthEnd.ToString(DateFormat, CultureInfo.InvariantCulture));
        occupiedCommand.Parameters.AddWithValue("$asOf", asOf.ToString(DateFormat, CultureInfo.InvariantCulture));

        decimal occupiedRoomHours =
            Convert.ToDecimal(occupiedCommand.ExecuteScalar() ?? 0d, CultureInfo.InvariantCulture);

        // Lấy các lượt lưu trú trong tháng để xác định ngày có khách.
        var occupiedDays = new SortedSet<int>();
        const string occupiedDaysSql = @"SELECT s.ActualCheckIn,
                                                COALESCE(s.ActualCheckOut, $asOf)
                                         FROM Stay s
                                         INNER JOIN Room r ON r.RoomId = s.RoomId
                                         WHERE r.IsActive = 1
                                           AND s.Status IN ('InProgress', 'CheckoutPending', 'Completed')
                                           AND s.ActualCheckIn < $monthEnd
                                           AND COALESCE(s.ActualCheckOut, $asOf) > $monthStart;";

        using var daysCommand = connection.CreateCommand();
        daysCommand.CommandText = occupiedDaysSql;
        daysCommand.Parameters.AddWithValue("$monthStart",
            monthStart.ToString(DateFormat, CultureInfo.InvariantCulture));
        daysCommand.Parameters.AddWithValue("$monthEnd",
            monthEnd.ToString(DateFormat, CultureInfo.InvariantCulture));
        daysCommand.Parameters.AddWithValue("$asOf",
            asOf.ToString(DateFormat, CultureInfo.InvariantCulture));

        using (var daysReader = daysCommand.ExecuteReader())
        {
            while (daysReader.Read())
            {
                var checkIn = DateTime.Parse(daysReader.GetString(0), CultureInfo.InvariantCulture);
                var checkOut = DateTime.Parse(daysReader.GetString(1), CultureInfo.InvariantCulture);

                if (checkIn < monthStart) checkIn = monthStart;
                if (checkOut > asOf) checkOut = asOf;
                if (checkOut > monthEnd) checkOut = monthEnd;
                // lấy đến ngay lại 0h00 ngày checkout, bỏ ngày checkout
                for (var day = checkIn.Date; day < checkOut; day = day.AddDays(1))
                    occupiedDays.Add(day.Day);
            }
        }

        // dùng PrintedAt làm tháng ghi nhận doanh thu, hóa đơn in rồi mới tính vào doanh thu
        const string revenueSql = @"SELECT
                                        COALESCE(SUM(RoomCharge),0),
                                        COALESCE(SUM(ServiceCharge),0)
                                    FROM Invoice
                                    WHERE Status = 'Paid' AND PrintedAt IS NOT NULL AND PrintedAt >= $monthStart AND PrintedAt < $monthEnd;";
        using var revenueCommand = connection.CreateCommand();
        revenueCommand.CommandText = revenueSql;
        revenueCommand.Parameters.AddWithValue("$monthStart",
            monthStart.ToString(DateFormat, CultureInfo.InvariantCulture));
        revenueCommand.Parameters.AddWithValue("$monthEnd",
            monthEnd.ToString(DateFormat, CultureInfo.InvariantCulture));
        using var reader = revenueCommand.ExecuteReader();

        if (!reader.Read()) throw new InvalidOperationException("Không thể đọc dữ liệu doanh thu tháng");

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,
            ActiveRoomCount = activeRoomCount,
            OccupiedRoomHours = Math.Round(occupiedRoomHours, 2, MidpointRounding.AwayFromZero),
            OccupiedDays = occupiedDays.ToList(),
            RoomRevenue = reader.GetInt64(0),
            ServiceRevenue = reader.GetInt64(1)
        };
    }
}
