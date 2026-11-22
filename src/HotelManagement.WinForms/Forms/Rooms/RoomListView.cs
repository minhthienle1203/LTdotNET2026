
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Services;
using HotelManagement.WinForms.Common;

namespace HotelManagement.WinForms.Forms.Rooms;

public sealed class RoomListView : UserControl
{
    private readonly RoomService _roomService;
    private readonly FloorService _floorService;
    private readonly Action<string, object>? _navigate;
    private readonly RoomListContext _context;
    private readonly TextBox _search = new();
    private readonly DataGridView _grid = new();

    public RoomListView(
        RoomService roomService,
        FloorService floorService,
        Action<string, object>? navigate,
        object? context = null)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _floorService = floorService ?? throw new ArgumentNullException(nameof(floorService));
        _navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        _context = context as RoomListContext ?? (context is int roomTypeId
            ? new RoomListContext(RoomTypeId: roomTypeId)
            : new RoomListContext());
        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        BuidldLayout();
        LoadRooms();
    }

    private void BuidldLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, WrapContents = true
        };
        top.Controls.Add(new Label{Text = "DANH SASCH PHONG", AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold), Margin = new Padding(4)});
        top.Controls.Add(new Label { Text = "Mã Phòng", AutoSize = true, Margin = new Padding(4) });
        _search.Width = 120;
        top.Controls.Add(_search);
        top.Controls.Add(MakeButton("Tìm", LoadRooms));
        top.Controls.Add(MakeButton("Thêm phòng", AddRoom));
        top.Controls.Add(MakeButton("Sửa", EditRoom));
        top.Controls.Add(MakeButton("Xem sơ đồ", ShowDashBoard));
        root.Controls.Add(top, 0 , 0);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.Columns.Add("Room Code", "Mã Phòng");
        _grid.Columns.Add("Floor", "Tầng");
        _grid.Columns.Add("RoomType", "Loại phòng");
        _grid.Columns.Add("Status", "Trạng thái");
        _grid.Columns.Add("Note", "Ghi chú");
        _grid.Columns.Add("IsActive", "Hoạt động");
        root.Controls.Add(_grid, 0 ,1);
        Controls.Add(root);
        _search.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) LoadRooms();
        };
    }

    private void ShowDashBoard()
    {
        if (_navigate != null) _navigate("S01", new DashboardConText());
    }

    private void EditRoom()
    {
        throw new NotImplementedException();
    }

    private void AddRoom()
    {
        _navigate("S06", new RoomFormContext());
    }

    private void LoadRooms()
    {
        try
        {
            var rooms = _roomService.GetRooms(_search.Text);
            _grid.Rows.Clear();
            foreach (var room in rooms)
            {
                int index = _grid.Rows.Add(room.RoomCode, $"{room.FloorNumber} - {room.FloorName}",
                                                          $"{room.TypeCode} - {room.TypeName}",
                    GetStatusText(room.Status), room.Note ?? string.Empty, room.IsActive ? "Đang hoạt động" : "Ngừng hoạt động");
                _grid.Rows[index].Tag = room;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể tải danh sách phòng",  MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string GetStatusText(RoomStatus roomStatus) => roomStatus switch
    {
        RoomStatus.Available => "Trống",
        RoomStatus.Reserved => "Đã đặt trước",
        RoomStatus.Occupied => "Đang có khách",
        RoomStatus.Cleaning => "Đang dọn",
        _ => "Không thể các định"
    };

    private static Button MakeButton(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
        button.Click += (_, _) => action();
        return button;
    }
}
