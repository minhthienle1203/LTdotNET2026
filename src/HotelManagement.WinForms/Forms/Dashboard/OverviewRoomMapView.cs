using System.Drawing;
using System.Windows.Forms;

namespace HotelManagement.WinForms.Forms.Dashboard;

/// <summary>
/// S01 - Tổng quan và sơ đồ phòng.
/// Hiện tại chỉ dùng dữ liệu minh họa; chưa truy cập SQL Server.
/// </summary>
public sealed class OverviewRoomMapView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly List<RoomItem> rooms;
    private readonly ComboBox floorFilter = new();
    private readonly ComboBox statusFilter = new();
    private readonly FlowLayoutPanel roomList = new();
    private readonly FlowLayoutPanel roomActions = new();
    private readonly TableLayoutPanel statisticPanel = new();
    private readonly Label roomCount = new();
    private readonly Label roomDetails = new();
    private RoomItem? selectedRoom;

    public OverviewRoomMapView(Action<string, object?> navigate)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        rooms = LoadRooms();

        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        AutoScroll = true;
        Height = 650;
        MinimumSize = new Size(0, 650);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 390));

        layout.Controls.Add(BuildHeader(), 0, 0);
        layout.Controls.Add(BuildStatistics(), 0, 1);
        layout.Controls.Add(BuildFilters(), 0, 2);
        layout.Controls.Add(BuildLegend(), 0, 3);
        layout.Controls.Add(BuildRoomArea(), 0, 4);
        Controls.Add(layout);

        InitializeFilters();
        RefreshDashboard();

        #if DEBUG
        DebugPrintAllDatabaseData();
        #endif
    }

    private Control BuildHeader()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label
        {
            Text = "TỔNG QUAN VÀ SƠ ĐỒ PHÒNG",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Margin = new Padding(0, 5, 16, 0)
        });
        return panel;
    }

    private Control BuildStatistics()
    {
        var panel = statisticPanel;
        panel.Dock = DockStyle.Fill;
        panel.ColumnCount = 5;
        panel.RowCount = 1;
        for (var i = 0; i < 5; i++)
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

        string[] titles = { "Tổng phòng", "Phòng trống", "Đang có khách", "Đang dọn", "Đã đặt trước" };
        for (var i = 0; i < titles.Length; i++)
        {
            var item = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Text = titles[i] + "\n0",
                Margin = new Padding(2)
            };
            panel.Controls.Add(item, i, 0);
        }
        return panel;
    }

    private Control BuildFilters()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Tầng", AutoSize = true, Margin = new Padding(0, 8, 5, 0) });
        floorFilter.Width = 120;
        panel.Controls.Add(floorFilter);
        panel.Controls.Add(new Label { Text = "Trạng thái", AutoSize = true, Margin = new Padding(12, 8, 5, 0) });
        statusFilter.Width = 145;
        panel.Controls.Add(statusFilter);
        panel.Controls.Add(CreateButton("Quản lý tầng", () => navigate("S02", null)));
        panel.Controls.Add(CreateButton("Loại phòng", () => navigate("S03", null)));
        panel.Controls.Add(CreateButton("Danh sách phòng", () => navigate("S05", null)));
        return panel;
    }

    private static Control BuildLegend()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Chú giải:", AutoSize = true, Margin = new Padding(0, 5, 8, 0) });
        AddLegendItem(panel, "Trống", Color.LightGreen);
        AddLegendItem(panel, "Đang ở", Color.LightCoral);
        AddLegendItem(panel, "Đang dọn", Color.Khaki);
        AddLegendItem(panel, "Đặt trước", Color.PeachPuff);
        return panel;
    }

    private Control BuildRoomArea()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        roomCount.Text = "Danh sách phòng";
        roomCount.Dock = DockStyle.Fill;
        panel.Controls.Add(roomCount, 0, 0);

        var detailsTitle = new Label { Text = "Thông tin phòng", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        panel.Controls.Add(detailsTitle, 1, 0);

        roomList.Dock = DockStyle.Fill;
        roomList.AutoScroll = true;
        roomList.WrapContents = true;
        panel.Controls.Add(roomList, 0, 1);

        var details = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        roomDetails.Dock = DockStyle.Fill;
        roomDetails.Text = "Chọn một phòng để xem thông tin.";
        details.Controls.Add(roomDetails, 0, 0);
        details.Controls.Add(new Label { Text = "Thao tác", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        roomActions.Dock = DockStyle.Fill;
        roomActions.FlowDirection = FlowDirection.TopDown;
        roomActions.WrapContents = false;
        details.Controls.Add(roomActions, 0, 2);
        panel.Controls.Add(details, 1, 1);
        return panel;
    }

    private void InitializeFilters()
    {
        // TODO (tích hợp CSDL): nạp tầng đang hoạt động qua FloorService.
        floorFilter.Items.Add("Tất cả tầng");
        foreach (var floor in rooms.Select(room => room.FloorNumber).Distinct().OrderBy(number => number))
            floorFilter.Items.Add($"Tầng {floor}");
        floorFilter.SelectedIndex = 0;

        statusFilter.Items.Add("Tất cả trạng thái");
        statusFilter.Items.AddRange(new object[] { "Trống", "Đang có khách", "Đang dọn", "Đã đặt trước" });
        statusFilter.SelectedIndex = 0;

        floorFilter.SelectedIndexChanged += (_, _) => RefreshDashboard();
        statusFilter.SelectedIndexChanged += (_, _) => RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        // TODO (tích hợp CSDL): nếu tải theo bộ lọc, truyền FloorId/RoomStatus cho RoomService; không viết SQL tại đây.
        var floorText = floorFilter.SelectedItem?.ToString() ?? "Tất cả tầng";
        var statusText = statusFilter.SelectedItem?.ToString() ?? "Tất cả trạng thái";
        var filteredRooms = rooms.Where(room =>
            (floorText == "Tất cả tầng" || floorText == $"Tầng {room.FloorNumber}") &&
            (statusText == "Tất cả trạng thái" || statusText == GetStatusText(room.Status))).ToList();

        roomCount.Text = $"Danh sách phòng ({filteredRooms.Count})";
        UpdateStatistics();
        roomList.Controls.Clear();

        foreach (var room in filteredRooms)
        {
            var button = new Button
            {
                Text = $"{room.RoomCode}\n{room.RoomTypeName} - {room.HourlyRate:N0} đ/giờ\n{GetStatusText(room.Status)}",
                Size = new Size(155, 82),
                BackColor = GetStatusColor(room.Status),
                UseVisualStyleBackColor = false,
                Margin = new Padding(4)
            };
            button.Click += (_, _) => SelectRoom(room);
            roomList.Controls.Add(button);
        }

        if (selectedRoom != null && !filteredRooms.Contains(selectedRoom))
            SelectRoom(null);
    }

    private void UpdateStatistics()
    {
        var panel = statisticPanel;
        var names = new[] { "Tổng phòng", "Phòng trống", "Đang có khách", "Đang dọn", "Đã đặt trước" };
        var counts = new[]
        {
            rooms.Count,
            rooms.Count(room => room.Status == RoomDisplayStatus.Available),
            rooms.Count(room => room.Status == RoomDisplayStatus.Occupied),
            rooms.Count(room => room.Status == RoomDisplayStatus.Cleaning),
            rooms.Count(room => room.Status == RoomDisplayStatus.Reserved)
        };

        for (var i = 0; i < counts.Length; i++)
            ((Label)panel.Controls[i]).Text = $"{names[i]}\n{counts[i]}";
    }

    private void SelectRoom(RoomItem? room)
    {
        selectedRoom = room;
        roomActions.Controls.Clear();
        if (room == null)
        {
            roomDetails.Text = "Chọn một phòng để xem thông tin.";
            return;
        }

        var relatedRecord = room.ReservationId.HasValue
            ? $"Đặt phòng liên quan: {room.ReservationId}"
            : room.StayId.HasValue ? $"Lượt lưu trú: {room.StayId}" : "";
        roomDetails.Text = $"Mã phòng: {room.RoomCode}\nTầng: {room.FloorNumber}\nLoại phòng: {room.RoomTypeName}\nGiá giờ: {room.HourlyRate:N0} đ\nGiá ngày: {room.DailyRate:N0} đ\nTrạng thái: {GetStatusText(room.Status)}\n{relatedRecord}";

        switch (room.Status)
        {
            case RoomDisplayStatus.Available:
                AddAction("Nhận phòng", "S11", room.RoomId);
                AddAction("Đặt phòng", "S10", room.RoomTypeId);
                break;
            case RoomDisplayStatus.Reserved:
                if (room.ReservationId.HasValue)
                {
                    AddAction("Xem đặt phòng", "S09", room.ReservationId.Value);
                    AddAction("Check-in", "S11", room.RoomId);
                }
                break;
            case RoomDisplayStatus.Occupied:
                if (room.StayId.HasValue)
                {
                    AddAction("Xem lưu trú", "S12", room.StayId.Value);
                    AddAction("Ghi dịch vụ", "S15", room.StayId.Value);
                    AddAction("Trả phòng", "S16", room.StayId.Value);
                }
                break;
            case RoomDisplayStatus.Cleaning:
                AddAction("Mở dọn phòng", "S19", room.RoomId);
                break;
        }
    }

    private void AddAction(string text, string screenCode, int contextId)
    {
        roomActions.Controls.Add(CreateButton(text, () => navigate(screenCode, contextId)));
    }

    private List<RoomItem> LoadRooms()
    {
        // TODO (tích hợp CSDL): lấy danh sách phòng qua RoomService/DTO.
        // RoomId, RoomCode, FloorId/FloorNumber, RoomTypeId,
        // tên loại phòng, giá giờ/ngày, RoomStatus và ReservationId/StayId đang hiệu lực.
        return CreateSampleRooms();
    }

    private static List<RoomItem> CreateSampleRooms() => new()
    {
        new(1, "101", 1, 1, "Standard", 100000, 500000, RoomDisplayStatus.Available),
        new(2, "102", 1, 1, "Standard", 100000, 500000, RoomDisplayStatus.Occupied, StayId: 601),
        new(3, "103", 1, 2, "Deluxe", 150000, 750000, RoomDisplayStatus.Reserved, ReservationId: 501),
        new(4, "104", 1, 2, "Deluxe", 150000, 750000, RoomDisplayStatus.Cleaning),
        new(5, "201", 2, 1, "Standard", 100000, 500000, RoomDisplayStatus.Available),
        new(6, "202", 2, 1, "Standard", 100000, 500000, RoomDisplayStatus.Occupied, StayId: 602),
        new(7, "203", 2, 2, "Deluxe", 150000, 750000, RoomDisplayStatus.Reserved, ReservationId: 502),
        new(8, "204", 2, 2, "Deluxe", 150000, 750000, RoomDisplayStatus.Available),
        new(9, "301", 3, 1, "Standard", 100000, 500000, RoomDisplayStatus.Occupied, StayId: 603),
        new(10, "302", 3, 1, "Standard", 100000, 500000, RoomDisplayStatus.Cleaning),
        new(11, "303", 3, 2, "Deluxe", 150000, 750000, RoomDisplayStatus.Reserved, ReservationId: 503),
        new(12, "304", 3, 2, "Deluxe", 150000, 750000, RoomDisplayStatus.Available)
    };

    private static Button CreateButton(string text, Action onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(5) };
        button.Click += (_, _) => onClick();
        return button;
    }

    private static void AddLegendItem(FlowLayoutPanel panel, string text, Color color)
    {
        panel.Controls.Add(new Panel { BackColor = color, Size = new Size(14, 14), Margin = new Padding(8, 6, 3, 0) });
        panel.Controls.Add(new Label { Text = text, AutoSize = true, Margin = new Padding(0, 5, 4, 0) });
    }

    private static string GetStatusText(RoomDisplayStatus status) => status switch
    {
        RoomDisplayStatus.Available => "Trống",
        RoomDisplayStatus.Occupied => "Đang có khách",
        RoomDisplayStatus.Cleaning => "Đang dọn",
        RoomDisplayStatus.Reserved => "Đã đặt trước",
        _ => "Không xác định"
    };

    private static Color GetStatusColor(RoomDisplayStatus status) => status switch
    {
        RoomDisplayStatus.Available => Color.Green,
        RoomDisplayStatus.Occupied => Color.Red,
        RoomDisplayStatus.Cleaning => Color.Yellow,
        RoomDisplayStatus.Reserved => Color.DarkOrange,
        _ => SystemColors.Control
    };

    private sealed record RoomItem(
        int RoomId,
        string RoomCode,
        int FloorNumber,
        int RoomTypeId,
        string RoomTypeName,
        decimal HourlyRate,
        decimal DailyRate,
        RoomDisplayStatus Status,
        int? ReservationId = null,
        int? StayId = null);

    private enum RoomDisplayStatus { Available, Occupied, Cleaning, Reserved }




#if DEBUG
// Chỉ in Database một lần khi mở Dashboard.
private static bool databaseDumped;

[System.Runtime.InteropServices.DllImport(
    "kernel32.dll",
    SetLastError = true)]
private static extern bool AllocConsole();

private static void DebugPrintAllDatabaseData()
{
    if (databaseDumped)
        return;

    databaseDumped = true;


    System.Console.SetOut(
        new System.IO.StreamWriter(
            System.Console.OpenStandardOutput())
        { AutoFlush = true });

    System.Console.OutputEncoding =
        new System.Text.UTF8Encoding(false);

    System.Console.WriteLine("==============================================");
    System.Console.WriteLine(" HOTEL MANAGEMENT - SQLITE DATABASE DEBUG");
    System.Console.WriteLine("==============================================");

    try
    {
        // Đọc Connection String từ appsettings.json hiện tại.
        string settingsPath = System.IO.Path.Combine(
            AppContext.BaseDirectory,
            "appsettings.json");

        using var settings = System.Text.Json.JsonDocument.Parse(
            System.IO.File.ReadAllText(settingsPath));

        string connectionString = settings.RootElement
            .GetProperty("ConnectionStrings")
            .GetProperty("HotelManagementDb")
            .GetString()
            ?? throw new Exception("Connection String bị trống.");

        // Sử dụng chính DbConnectionFactory của ứng dụng.
        var factory =
            new HotelManagement.Infrastructure.Data.DbConnectionFactory(
                connectionString);

        using var connection = factory.CreateConnection();

        // Không tự tạo một Database rỗng nếu file chưa tồn tại.
        if (connection.DataSource != ":memory:" &&
            !System.IO.File.Exists(connection.DataSource))
        {
            throw new System.IO.FileNotFoundException(
                "Không tìm thấy file SQLite.",
                connection.DataSource);
        }

        connection.Open();

        System.Console.WriteLine(
            $"Database: {connection.DataSource}");

        System.Console.WriteLine(
            $"SQLite Version: {connection.ServerVersion}");

        // Lấy danh sách tất cả bảng nghiệp vụ.
        var tableNames = new System.Collections.Generic.List<string>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText =
                "SELECT name FROM sqlite_master " +
                "WHERE type = 'table' " +
                "AND name NOT LIKE 'sqlite_%' " +
                "ORDER BY name;";

            using var reader = command.ExecuteReader();

            while (reader.Read())
                tableNames.Add(reader.GetString(0));
        }

        System.Console.WriteLine(
            $"\nTổng số bảng: {tableNames.Count}");

        int totalRecords = 0;

        // Đọc toàn bộ bản ghi của từng bảng.
        foreach (string tableName in tableNames)
        {
            System.Console.WriteLine("\n----------------------------------------------");
            System.Console.WriteLine($"TABLE: {tableName}");
            System.Console.WriteLine("----------------------------------------------");

            using var command = connection.CreateCommand();

            string safeName = "\"" +
                tableName.Replace("\"", "\"\"") + "\"";

            command.CommandText = $"SELECT * FROM {safeName};";

            using var reader = command.ExecuteReader();

            int rowCount = 0;

            while (reader.Read())
            {
                rowCount++;

                var values = new System.Collections.Generic.List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object value = reader.IsDBNull(i)
                        ? "NULL"
                        : reader.GetValue(i);

                    string displayValue = value is byte[] bytes
                        ? $"BLOB ({bytes.Length} bytes)"
                        : Convert.ToString(
                            value,
                            System.Globalization.CultureInfo.InvariantCulture)
                            ?? "NULL";

                    values.Add(
                        $"{reader.GetName(i)} = {displayValue}");
                }

                System.Console.WriteLine(
                    $"[{rowCount}] {string.Join(" | ", values)}");
            }

            if (rowCount == 0)
                System.Console.WriteLine("(Không có dữ liệu)");

            totalRecords += rowCount;

            System.Console.WriteLine(
                $"Số bản ghi: {rowCount}");
        }

        System.Console.WriteLine("\n==============================================");
        System.Console.WriteLine(" DATABASE READ COMPLETED");
        System.Console.WriteLine($" Tổng số bảng: {tableNames.Count}");
        System.Console.WriteLine($" Tổng số bản ghi: {totalRecords}");
        System.Console.WriteLine("==============================================");
    }
    catch (Exception ex)
    {
        System.Console.WriteLine("\nDATABASE READ FAILED:");
        System.Console.WriteLine(ex);
    }
}

#endif

}
