using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Housekeeping;

public class HousekeepingView : UserControl
{
    private readonly RoomService _roomService;
    private readonly Action<string, object?> _navigate;
    private readonly DataGridView _grid = new();
    private readonly Label _detail = new();

    public HousekeepingView(RoomService roomService, Action<string, object?> navigate)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        BuildLayout();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        top.Controls.Add(new Label {Text = "DỌN PHÒNG", AutoSize = true, Font = new Font("Segoe UI", 13 ,FontStyle.Bold), Margin = new Padding(4)});
        top.Controls.Add(MakeButton("Làm mới", LoadCleaningRoom));
        top.Controls.Add(MakeButton("Xem sơ đồ", () => _navigate("S01", null)));
        root.Controls.Add(top, 0, 0);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.Columns.Add("RoomCode", "Mã phòng");
        _grid.Columns.Add("Floor", "Tầng");
        _grid.Columns.Add("RoomType", "Loại phòng");
        _grid.Columns.Add("Note", "Ghi chú");
        root.Controls.Add(_grid, 0, 1);
        _detail.Dock = DockStyle.Fill;
        _detail.Padding = new Padding(8);
        _detail.Text = "Chọn phòng đang dọn để xem thông tin";
        root.Controls.Add(_detail, 0, 2);
        Controls.Add(root);
    }

    private void LoadCleaningRoom()
    {
        throw new NotImplementedException();
    }

    private static Button MakeButton(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
        button.Click +=(_,_) => action();
        return button;
    }
}
