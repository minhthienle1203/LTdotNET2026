using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace HotelManagement.Infrastructure.Data.Repositories;
internal static class Db
{
    private const string DtFormat = "yyyy-MM-dd HH:mm:ss";

    public static SqliteConnection Open(DbConnectionFactory factory)
    {
        var conn = factory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        return conn;
    }

    public static string Dt(DateTime v) => v.ToString(DtFormat, CultureInfo.InvariantCulture);

    public static SqliteCommand Cmd(SqliteConnection conn, string sql, SqliteTransaction? tx = null,
        params (string Name, object? Value)[] args)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Transaction = tx;
        foreach (var (name, value) in args)
            cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return cmd;
    }

    public static object? Scalar(SqliteConnection conn, string sql, SqliteTransaction? tx = null,
        params (string Name, object? Value)[] args)
    {
        using var cmd = Cmd(conn, sql, tx, args);
        var v = cmd.ExecuteScalar();
        return v is DBNull ? null : v;
    }

    public static bool Exists(SqliteConnection conn, string sql, SqliteTransaction? tx = null,
        params (string Name, object? Value)[] args) => Scalar(conn, sql, tx, args) is not null;

    public static int Exec(SqliteConnection conn, string sql, SqliteTransaction? tx = null,
        params (string Name, object? Value)[] args)
    {
        using var cmd = Cmd(conn, sql, tx, args);
        return cmd.ExecuteNonQuery();
    }
    public static string? Str(SqliteDataReader r, string col)
    {
        int i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : r.GetString(i);
    }

    public static int Int(SqliteDataReader r, string col) => r.GetInt32(r.GetOrdinal(col));

    public static int? IntN(SqliteDataReader r, string col)
    {
        int i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : r.GetInt32(i);
    }

    public static bool Bool(SqliteDataReader r, string col) => r.GetInt32(r.GetOrdinal(col)) != 0;

    public static decimal Dec(SqliteDataReader r, string col)
    {
        int i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? 0m : Convert.ToDecimal(r.GetValue(i), CultureInfo.InvariantCulture);
    }

    public static DateTime Date(SqliteDataReader r, string col) =>
        DateTime.Parse(r.GetString(r.GetOrdinal(col)), CultureInfo.InvariantCulture);

    public static DateTime? DateN(SqliteDataReader r, string col)
    {
        int i = r.GetOrdinal(col);
        return r.IsDBNull(i) ? null : DateTime.Parse(r.GetString(i), CultureInfo.InvariantCulture);
    }
}

