using System.Globalization;
using Microsoft.Data.Sqlite;

namespace HotelManagement.Infrastructure.Data;


public static class SqliteValueConverter
{
    // convert VND qua integer.
    public static long ToVnd(decimal amount)
    {
        if (amount != decimal.Truncate(amount))
            throw new ArgumentException("Khong dung so thap phan voi VND", nameof(amount));

        return checked((long)amount);
    }

    public static decimal FromVnd(SqliteDataReader reader, int ordinal)
        => reader.GetInt64(ordinal);

    // lưu quatity thanh String.
    public static string ToDecimalText(decimal value)
        => value.ToString(CultureInfo.InvariantCulture);

    public static decimal FromDecimalText(SqliteDataReader reader, int ordinal)
        => decimal.Parse(reader.GetString(ordinal), CultureInfo.InvariantCulture);
}
