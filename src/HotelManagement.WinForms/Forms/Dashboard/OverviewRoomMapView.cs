using System.Drawing;
using System.Windows.Forms;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Services;
using HotelManagement.WinForms.Common;

namespace HotelManagement.WinForms.Forms.Dashboard;

/// S01 - Tổng quan và sơ đồ phòng.
public sealed class OverviewRoomMapView : UserControl
{
    private readonly RoomService _roomService;
    private readonly FloorService _floorService;
    private readonly Action<string, object?> _navigate;
    private readonly ComboBox _floorFilter = new();
    private readonly ComboBox _statusFilter = new();
    private readonly FlowLayoutPanel _roomList = new();
    private readonly FlowLayoutPanel _roomActions = new();
    private readonly TableLayoutPanel _statistics = new();
    private readonly Label _roomCount = new();
    private readonly Label _roomDetails = new();
    private readonly Label[] _statisticLabels = new Label[5];
    private List<RoomDashboardDto> _rooms = new();
    private RoomDashboardDto? _selectedRoom;
    private readonly DashboardConText? _context;

    public OverviewRoomMapView(RoomService roomService, FloorService floorService, Action<string, object> navigate,
        object? context = null)
    {
        _roomService = roomService;
        _floorService = floorService;
        _navigate = navigate;
        _context = context as DashboardConText;

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

        LoadDashboard();

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
        panel.Controls.Add(CreateButton("Lam moi", LoadDashboard));
        return panel;
    }

    private Control BuildStatistics()
    {
        _statistics.Dock = DockStyle.Fill;
        _statistics.ColumnCount = 5;
        _statistics.RowCount = 1;
        for (var i = 0; i < 5; i++)
        {
            _statistics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            _statisticLabels[i] = new Label()
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "0",
                Margin = new Padding(2)
            };
            _statistics.Controls.Add(_statisticLabels[i], i, 0);
        }
        return _statistics;
    }

    private Control BuildFilters()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Tầng", AutoSize = true, Margin = new Padding(0, 8, 5, 0) });
        _floorFilter.Width = 135;
        panel.Controls.Add(_floorFilter);
        panel.Controls.Add(new Label { Text = "Trạng thái", AutoSize = true, Margin = new Padding(12, 8, 5, 0) });
        _statusFilter.Width = 150;
        panel.Controls.Add(_statusFilter);
        panel.Controls.Add(CreateButton("Quản lý tầng", () => _navigate("S02", null)));
        panel.Controls.Add(CreateButton("Loại phòng", () => _navigate("S03", null)));
        panel.Controls.Add(CreateButton("Danh sách phòng", () => _navigate("S05", null)));
        _statusFilter.SelectedIndexChanged += (_, _) => RefreshDashboard();
        return panel;
    }

    private static Control BuildLegend()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Chi tiết:", AutoSize = true, Margin = new Padding(0, 5, 8, 0) });
        AddLegendItem(panel, "Trống", Color.Green);
        AddLegendItem(panel, "Đang ở", Color.Red);
        AddLegendItem(panel, "Đang dọn", Color.Yellow);
        AddLegendItem(panel, "Đặt trước", Color.DarkOrange);
        return panel;
    }

    private Control BuildRoomArea()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _roomCount.Text = "Danh sách phòng";
        _roomCount.Dock = DockStyle.Fill;
        panel.Controls.Add(_roomCount, 0, 0);
        panel.Controls.Add(new Label
        {
            Text = "Thông tin phòng",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 1, 0);

        _roomList.Dock = DockStyle.Fill;
        _roomList.AutoScroll = true;
        _roomList.WrapContents = true;
        panel.Controls.Add(_roomList, 0, 1);

        var details = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        _roomDetails.Dock = DockStyle.Fill;
        _roomDetails.Text = "Chọn một phòng để xem thông tin.";
        details.Controls.Add(_roomDetails, 0, 0);
        details.Controls.Add(new Label { Text = "Thao tác", Dock = DockStyle.Fill }, 0, 1);
        _roomActions.Dock = DockStyle.Fill;
        _roomActions.FlowDirection = FlowDirection.TopDown;
        _roomActions.WrapContents = false;
        details.Controls.Add(_roomActions, 0, 2);
        panel.Controls.Add(details, 1, 1);
        return panel;
    }

    private void LoadDashboard()
    {
        try
        {
            _rooms = _roomService.GetRoomsByFloor().ToList();
            _selectedRoom = null;
            SelectRoom(null);
            LoadFilters();
            RefreshDashboard();
            SelectRoom(_rooms.FirstOrDefault(room => room.RoomId == _context?.RoomId));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi tải dữ liệu", ex.Message);
        }
    }

    private void FloorFilterChange(object? sender, EventArgs e) => RefreshDashboard();



    private void LoadFilters()
    {
        var selectedFloorId = _context?.FloorId;
        _floorFilter.SelectedIndexChanged -= FloorFilterChange;
        _floorFilter.Items.Clear();
        _floorFilter.Items.Add(new FloorFilterItem(null, "Tất cả tầng"));
        foreach (Floor floor in _floorService.GetFloors())
        {
            _floorFilter.Items.Add(new FloorFilterItem(floor.FloorId, $"Tầng{floor.FloorNumber} = {floor.FloorName}"));
        }

        _floorFilter.DisplayMember = nameof(FloorFilterItem.Text);
        _floorFilter.SelectedIndex = 0;
        if (selectedFloorId.HasValue)
        {
            for (var i = 0; i < _floorFilter.Items.Count; i++)
            {
                if (_floorFilter.Items[i] is FloorFilterItem item && item.FloorId == selectedFloorId)
                {
                    _floorFilter.SelectedIndex = i;
                    break;
                }
            }
        }

        _statusFilter.Items.Clear();
        _statusFilter.Items.Add(new StatusFilterItem(null, "Tất cả trạng thái"));
        _statusFilter.Items.Add(new StatusFilterItem(RoomStatus.Available, "Trống"));
        _statusFilter.Items.Add(new StatusFilterItem(RoomStatus.Occupied, "Đang có khách"));
        _statusFilter.Items.Add(new StatusFilterItem(RoomStatus.Cleaning, "Đang dọn dẹp"));
        _statusFilter.Items.Add(new StatusFilterItem(RoomStatus.Reserved, "Đã đặc trước"));
        _statusFilter.DisplayMember = nameof(StatusFilterItem.Text);
        _statusFilter.SelectedIndex = 0;
        _floorFilter.SelectedIndexChanged += FloorFilterChange;
    }

    private void RefreshDashboard()
    {
        if ((_floorFilter.SelectedItem is not FloorFilterItem floorItem) ||
        (_statusFilter.SelectedItem is not StatusFilterItem statusItem))
        return;
        var filteredRooms = _rooms.Where(room =>
            (!floorItem.FloorId.HasValue || room.FloorId == floorItem.FloorId.Value) &&
            (!statusItem.Status.HasValue || room.Status == statusItem.Status.Value));
        var roomDashboardDtos = filteredRooms as RoomDashboardDto[] ?? filteredRooms.ToArray();
        _roomCount.Text = $"Danh sách phòng ({roomDashboardDtos.Count()})";
        UpdateStatistics();
        _roomList.Controls.Clear();

        foreach (var room in roomDashboardDtos)
        {
            var button = new Button
            {
                Text = $"{room.RoomCode}\n{room.TypeName} - {room.HourlyRate:N0} đ/giờ \n{GetStatusText(room.Status)}",
                Size = new Size(165, 88),
                BackColor = GetStatusColor(room.Status),
                UseVisualStyleBackColor = false,
                Margin = new Padding(4)
            };
            button.Click += (_, _) => SelectRoom(room);
            _roomList.Controls.Add(button);
        }

        if (_selectedRoom != null && !roomDashboardDtos.Any(room => room.RoomId == _selectedRoom.RoomId))
            SelectRoom(null);
    }

    private void UpdateStatistics()
    {
        var labels = new[]
        {
            $"Tổng phòng \n {_rooms.Count}",
            $"Phòng trống \n {_rooms.Count(room => room.Status == RoomStatus.Available)}",
            $"Phòng có khách \n {_rooms.Count(room => room.Status == RoomStatus.Occupied)}",
            $"Đang dọn \n {_rooms.Count(room => room.Status == RoomStatus.Cleaning)}",
            $"Đã đặt trước \n {_rooms.Count(room => room.Status == RoomStatus.Reserved)}"
        };
        for (var i = 0; i < labels.Length; i++)
            _statisticLabels[i].Text = labels[i];
    }

    private void SelectRoom(RoomDashboardDto? room)
    {
        _selectedRoom = room;
        _roomActions.Controls.Clear();
        if (room == null)
        {
            _roomDetails.Text = "Chọn một phòng để xem thông tin.";
            return;
        }

        var relatedRecord = room.ReservationId.HasValue
            ? $"ReservationId: {room.ReservationId}"
            : room.StayId.HasValue
                ? $"StayId: {room.StayId}"
                : "";
        _roomDetails.Text = $"Mã phòng: {room.RoomCode}\n" +
                            $"Tầng: {room.FloorNumber} - {room.FloorName}\n" +
                            $"Loại phòng: {room.TypeName}\n" +
                            $"Gía theo giờ: {room.HourlyRate:N0}Đ\n" +
                            $"Gía theo ngày: {room.DailyRate:N0}Đ\n" +
                            $"Trạng thái: {GetStatusText(room.Status)}\n" +
                            $"Trạng thái lưu trú: {GetStayStatusText(room.StayStatus)}\n" +
                            $"Ghi chú: {room.Note ?? "Không có ghi chú"} \n {relatedRecord}";

        switch (room.Status)
        {
            case RoomStatus.Available:
                AddAction("Nhận phòng", "S11", room.RoomId);
                AddAction("Đặt phòng", "S10", room.RoomTypeId);
                break;
            case RoomStatus.Reserved:
                if (room.ReservationId.HasValue)
                {
                    AddAction("Xem đặt phòng", "S09", room.ReservationId.Value);
                    AddAction("Check-in", "S11", room.RoomId);
                }

                break;
            case RoomStatus.Occupied:
                if (room.StayId.HasValue)
                {
                    AddAction("Xem lưu trú", "S12", room.StayId.Value);
                    AddAction("Ghi dịch vụ", "S15", room.StayId.Value);
                    AddAction("Trả phòng", "S16", room.StayId.Value);
                }

                break;
            case RoomStatus.Cleaning:
                AddAction("Mở dọn phòng", "S19", room.RoomId);
                break;
        }
    }

    private void AddAction(string text, string screenCode, int id)
    {
        _roomActions.Controls.Add(CreateButton(text, () => _navigate(screenCode, id)));
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(4) };
        button.Click += (_, _) => action();
        return button;
    }

    private static void AddLegendItem(FlowLayoutPanel panel, string text, Color color)
    {
        panel.Controls.Add(new Panel { BackColor = color, Size =  new Size(14,14), Margin =new Padding(8, 6,3,0)});
        panel.Controls.Add(new Label{ Text = text, AutoSize = true, Margin = new Padding(0 , 5, 4,0)});

    }

    private static string GetStayStatusText(StayStatus? status) => status switch
    {
        StayStatus.InProgress => "Đang ở",
        StayStatus.CheckoutPending => "Đang chờ hoàn tất trả phòng",
        StayStatus.Completed => "Đã hoàn tất", _ => "Đang không có khách ở"
    };

    private static string GetStatusText(RoomStatus status) => status switch
    {
        RoomStatus.Available => "Trống",
        RoomStatus.Occupied => "Đang có khách",
        RoomStatus.Cleaning => "Đang dọn",
        RoomStatus.Reserved => "Đã đặt trước",
        _ => "Không thể xác định"
    };

    private static Color GetStatusColor(RoomStatus status) => status switch
    {
        RoomStatus.Available => Color.Green,
        RoomStatus.Occupied => Color.Red,
        RoomStatus.Cleaning => Color.Yellow,
        RoomStatus.Reserved => Color.DarkOrange,
        _ => SystemColors.Control
    };

    private sealed record FloorFilterItem(int? FloorId, string Text);
    private sealed record StatusFilterItem(RoomStatus? Status, string Text);
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

        Console.WriteLine(
            $"Database: {connection.DataSource}");

        Console.WriteLine(
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

        Console.WriteLine(
            $"\nTổng số bảng: {tableNames.Count}");

        int totalRecords = 0;

        // Đọc toàn bộ bản ghi của từng bảng.
        foreach (string tableName in tableNames)
        {
            Console.WriteLine("\n----------------------------------------------");
            Console.WriteLine($"TABLE: {tableName}");
            Console.WriteLine("----------------------------------------------");

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

                Console.WriteLine(
                    $"[{rowCount}] {string.Join(" | ", values)}");
            }

            if (rowCount == 0)
                Console.WriteLine("(Không có dữ liệu)");

            totalRecords += rowCount;

            Console.WriteLine(
                $"Số bản ghi: {rowCount}");
        }

        Console.WriteLine("\n==============================================");
        Console.WriteLine(" DATABASE READ COMPLETED");
        Console.WriteLine($" Tổng số bảng: {tableNames.Count}");
        Console.WriteLine($" Tổng số bản ghi: {totalRecords}");
        Console.WriteLine("==============================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine("\nDATABASE READ FAILED:");
        Console.WriteLine(ex);
    }
}

#endif

}


