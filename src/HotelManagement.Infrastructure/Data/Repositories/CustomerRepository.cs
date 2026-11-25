using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace HotelManagement.Infrastructure.Data.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly DbConnectionFactory _factory;

    public CustomerRepository(DbConnectionFactory factory) => _factory = factory;

    public List<Customer> Search(string keyword)
    {
        using var conn = Db.Open(_factory);
        using var cmd = Db.Cmd(conn, @"
            SELECT CustomerId, FullName, IdentityType, IdentityNumber, Phone, Email, Address, CreatedAt, IsActive
            FROM Customer
            WHERE IsActive = 1
              AND (@k = '' OR FullName LIKE @like ESCAPE '\' OR Phone LIKE @like ESCAPE '\'
                   OR IdentityNumber LIKE @like ESCAPE '\' OR Address LIKE @like ESCAPE '\')
            ORDER BY CustomerId", null,
            ("@k", keyword), ("@like", "%" + Escape(keyword) + "%"));
        using var rd = cmd.ExecuteReader();
        var list = new List<Customer>();
        while (rd.Read()) list.Add(Map(rd));
        return list;
    }

    public Customer? GetById(int customerId)
    {
        using var conn = Db.Open(_factory);
        using var cmd = Db.Cmd(conn, @"
            SELECT CustomerId, FullName, IdentityType, IdentityNumber, Phone, Email, Address, CreatedAt, IsActive
            FROM Customer WHERE CustomerId = @id", null, ("@id", customerId));
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? Map(rd) : null;
    }

    public bool IdentityExists(string identityNumber, int? excludeCustomerId)
    {
        using var conn = Db.Open(_factory);
        return Db.Exists(conn, @"
            SELECT 1 FROM Customer
            WHERE UPPER(IdentityNumber) = UPPER(@n) AND CustomerId <> @ex LIMIT 1", null,
            ("@n", identityNumber), ("@ex", excludeCustomerId ?? -1));
    }

    public int Insert(Customer c)
    {
        using var conn = Db.Open(_factory);
        using var cmd = Db.Cmd(conn, @"
            INSERT INTO Customer (FullName, IdentityType, IdentityNumber, Phone, Email, Address, CreatedAt, IsActive)
            VALUES (@name, @type, @num, @phone, @email, @addr, @created, 1);
            SELECT last_insert_rowid();", null,
            ("@name", c.FullName), ("@type", IdentityToDb(c.IdentityType)), ("@num", c.IdentityNumber),
            ("@phone", c.Phone), ("@email", c.Email), ("@addr", c.Address), ("@created", Db.Dt(c.CreatedAt)));
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Customer c)
    {
        using var conn = Db.Open(_factory);
        Db.Exec(conn, @"
            UPDATE Customer SET FullName=@name, IdentityType=@type, IdentityNumber=@num,
                   Phone=@phone, Email=@email, Address=@addr
            WHERE CustomerId=@id", null,
            ("@name", c.FullName), ("@type", IdentityToDb(c.IdentityType)), ("@num", c.IdentityNumber),
            ("@phone", c.Phone), ("@email", c.Email), ("@addr", c.Address), ("@id", c.CustomerId));
    }

    public int PeekNextId()
    {
        using var conn = Db.Open(_factory);
        var seq = Db.Scalar(conn, "SELECT seq FROM sqlite_sequence WHERE name='Customer'");
        return seq is null ? 1 : Convert.ToInt32(seq) + 1;
    }

    private static Customer Map(SqliteDataReader r) => new()
    {
        CustomerId = Db.Int(r, "CustomerId"),
        FullName = Db.Str(r, "FullName") ?? string.Empty,
        IdentityType = Enum.Parse<IdentityType>(Db.Str(r, "IdentityType") ?? "CCCD", true),
        IdentityNumber = Db.Str(r, "IdentityNumber") ?? string.Empty,
        Phone = Db.Str(r, "Phone") ?? string.Empty,
        Email = Db.Str(r, "Email"),
        Address = Db.Str(r, "Address"),
        CreatedAt = Db.Date(r, "CreatedAt"),
        IsActive = Db.Bool(r, "IsActive")
    };

    private static string IdentityToDb(IdentityType t) => t == IdentityType.Passport ? "Passport" : "CCCD";

    private static string Escape(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
