using HotelManagement.WinForms.Common;

using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Rooms;

public sealed class RoomFormView : UserControl
{
    private readonly RoomService _roomService;
    private readonly FloorService _floorService;
    //roomtype
    private readonly Action<string, object?> _navigate;
    private readonly TextBox _roomCode = new();
    private readonly ComboBox _floor = new();
    private readonly ComboBox _roomType = new();
    private readonly TextBox _note = new();
    private readonly CheckBox _isActive = new() {Text = "Đang hoạt động"};
    private readonly Label _status = new();
    private readonly RoomFormContext _context;

    public RoomFormView(
        RoomService roomService,
        FloorService floorService,
        Action<string, object?> navigate
     //   object? context
    )
    {
        _roomService = roomService;
        _floorService = floorService;
        _navigate = navigate;
     //   _context = context as RoomFormContext ?? new RoomFormContext();
        Dock = DockStyle.Fill;
        Padding = new Padding(18);
        BuildLayout();
        LoadOptions();
        LoadRoom();
    }

    private void LoadOptions()
    {
        throw new NotImplementedException();
    }

    private void LoadRoom()
    {
        throw new NotImplementedException();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
            { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 7, Width = 760 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var title = new Label
        {
            Text = _context.RoomId.HasValue ? "CẬP NHẬT PHÒNG" : "THÊM PHÒNG",
            Font = new Font("Segoe UI", 13, FontStyle.Bold), AutoSize = true
        };
        root.Controls.Add(title, 0 ,0);
        root.SetColumnSpan(title, 2);
        _roomCode.MaxLength = 30;
        _floor.DropDownStyle = ComboBoxStyle.DropDownList;
        _roomType.DropDownStyle = ComboBoxStyle.DropDownList;
        _note.Multiline = true;
        _note.Height = 65;
        AddField(root, 1, "Mã Phòng(*)", _roomCode);
        AddField(root, 2, "Tầng (*)", _floor);
        AddField(root, 3, "Loại phòng (*)", _roomType);
        AddField(root, 4 , "Ghi chú", _note);
        AddField(root, 5, "Trạng thái phòng", _status);
        AddField(root, 6 , "Trạng thái hoạt động", _isActive);
        var buttons = new FlowLayoutPanel{ Dock = DockStyle.Top, AutoSize =true};
        buttons.Controls.Add(MakeButton("Lưu", SaveRoom ));
        buttons.Controls.Add(MakeButton("Hủy", ReturnToDashboard));
        root.RowCount = 8;
        root.Controls.Add(buttons ,0 , 7);
        root.SetColumnSpan(buttons, 2);
        Controls.Add(root);
    }

    private Control? MakeButton(string lưu, object saveRoom)
    {
        throw new NotImplementedException();
    }

    private void SaveRoom()
    {

    }

    private void ReturnToDashboard()
    {
        _navigate("S01", _context.ReturnContext);
    }
    private static void AddField(TableLayoutPanel panel, int row, string label, Control control)
    {
        throw new NotImplementedException();
    }
}
