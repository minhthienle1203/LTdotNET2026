namespace HotelManagement.Core.Services;

public static class RoomPriceCalculator
{
    public static readonly TimeSpan CheckOutDeadline = TimeSpan.FromHours(12);

    public static int CountHours(DateTime checkIn, DateTime checkOut)
    {
        var span = checkOut - checkIn;
        return span.TotalHours > 0 ? Math.Max(1, (int)Math.Ceiling(span.TotalHours)) : 0;
    }

    public static int CountDays(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn) return 0;

        var days = (checkOut.Date - checkIn.Date).Days;
        if (checkOut.TimeOfDay > CheckOutDeadline) days++;
        return Math.Max(1, days);
    }

    public static decimal ByHour(DateTime checkIn, DateTime checkOut, decimal hourlyRate)
        => CountHours(checkIn, checkOut) * hourlyRate;

    public static decimal ByDay(DateTime checkIn, DateTime checkOut, decimal dailyRate)
        => CountDays(checkIn, checkOut) * dailyRate;
}
