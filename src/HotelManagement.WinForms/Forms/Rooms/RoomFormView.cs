using HotelManagement.Core.Enums;
using HotelManagement.WinForms.Common;

using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Rooms;

public sealed class RoomFormView : UserControl
{
    private readonly RoomService _roomService;
    private readonly FloorService _floorService;
    private readonly Action<string, object?> _navigate;
    private readonly TextBox _roomCode = new();
    private readonly ComboBox _floor = new();
    private readonly ComboBox _roomType = new();
    private readonly TextBox _note = new();
    private readonly CheckBox _isActive = new() {Text = "Đang hoạt động", Checked = true};
    private readonly Label _status = new() {AutoSize = true};
    private readonly RoomFormContext? _context;
    private Button? _saveButton;

    public RoomFormView(
        RoomService roomService,
        FloorService floorService,
        Action<string, object?> navigate,
        object? context = null
    )
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _floorService = floorService ?? throw new ArgumentNullException(nameof(floorService));
        _navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        _context = context as RoomFormContext ?? new RoomFormContext();
        Dock = DockStyle.Fill;
        Padding = new Padding(18);
        BuildLayout();
        try
        {
            LoadOptions();
            LoadRoom();
        }
        catch (Exception ex)
        {
            _saveButton!.Enabled = false;
            MessageBox.Show(this, ex.Message, "Khong the mo form chinh sua");
        }
}

    private void LoadOptions()
    {
        throw new NotImplementedException();
    }

    private void LoadRoom()
    {
        var room = _roomService.GetRoomById(_context.RoomId.Value) ??
                   throw new InvalidOperationException("Không tìm thấy phòng đã chọn");
        _roomCode.Text = room.RoomCode;
        _note.Text = room.Note ?? string.Empty;
        _isActive.Checked = room.IsActive;
        _status.Text = GetStatusText(room.Status);
        SelectFloorById(room.RoomId);
        SelectRoomTypeById(room.RoomTypeId);
    }

    private void SelectRoomTypeById(int roomRoomTypeId)
    {
        throw new NotImplementedException();
    }

    private void SelectFloorById(int roomRoomId)
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
        _floor.Width = 320;
        _roomType.Width = 320;
        _note.Multiline = true;
        _note.Height = 65;
        _status.Text = "Trạng thái mặc định khi đăng kí phòng mới - Hiện chưa có thông tin";
        AddField(root, 1, "Mã Phòng(*)", _roomCode);
        AddField(root, 2, "Tầng (*)", _floor);
        AddField(root, 3, "Loại phòng (*)", _roomType);
        AddField(root, 4 , "Ghi chú", _note);
        AddField(root, 5, "Trạng thái phòng", _status);
        AddField(root, 6 , "Trạng thái hoạt động", _isActive);
        var buttons = new FlowLayoutPanel{ Dock = DockStyle.Top, AutoSize =true};
        _saveButton = MakeButton("Lưu", SaveRoom);
        buttons.Controls.Add(_saveButton);
        buttons.Controls.Add(MakeButton("Hủy", ReturnToList));
        root.RowCount = 8;
        root.Controls.Add(buttons ,0 , 7);
        root.SetColumnSpan(buttons, 2);
        Controls.Add(root);
    }

    private static Button MakeButton(string text, Action action)
    {
       var button = new Button {Text = text, AutoSize = true, Margin = new Padding(4)};
       button.Click += (_, _) => action();
       return button;
    }

    private void SaveRoom()
    {

    }

    private void ReturnToList()
    {
        _navigate("S05", _context.ReturnContext ?? new RoomListContext());
    }
    private static string GetStatusText(RoomStatus roomStatus) => roomStatus switch
    {
        RoomStatus.Available => "Trống",
        RoomStatus.Reserved => "Đã đặt trước",
        RoomStatus.Occupied => "Đang có khách",
        RoomStatus.Cleaning => "Đang dọn",
        _ => "Không thể các định"
    };
    private static void AddField(TableLayoutPanel panel, int row, string label, Control control)
    {
        panel.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0,8,6,4)
        }, 0 ,row);

        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        control.Margin = new Padding(4);
        panel.Controls.Add(control, 1, row);
    }
}
