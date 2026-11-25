using HotelManagement.Core.DTOs;
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
        LoadCleaningRoom();
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
        top.Controls.Add(MakeButton("Hoàn tất dọn phòng", CompleteSelected));
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
        _grid.SelectionChanged += (_, _) => ShowSelectedRoom();
        root.Controls.Add(_grid, 0, 1);
        _detail.Dock = DockStyle.Fill;
        _detail.Padding = new Padding(8);
        _detail.Text = "Chọn phòng đang dọn để xem thông tin";
        root.Controls.Add(_detail, 0, 2);
        Controls.Add(root);
    }
    private RoomListDto? GetSelected() => _grid.CurrentRow?.Tag as RoomListDto;
    //xem chi tiết
    private void ShowSelectedRoom()
    {
        var room = GetSelected();
        _detail.Text = room == null
            ? "Bạn chưa chọn phòng đang dọn dẹp"
            : $"Mã phòng:{room.RoomCode}\n Tầng: {room.FloorName} \n Loại phòng: {room.TypeName}\n Trạng thái: Đang dọn \n Ghi chú: {room.Note ?? "Không có"}";
    }

    private void CompleteSelected()
    {
        var room = GetSelected();
        if (MessageBox.Show(this, $"Xác nhận hoàn tất dọn dẹp {room.RoomCode}?", "Xác nhận"
                , MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        try
        {
            _roomService.CompleteCleaning(room.RoomId);
            MessageBox.Show(this, "Đã cập nhật trạng thái phòng", "Thông báo", MessageBoxButtons.OK);
            LoadCleaningRoom();
        }
        catch (Exception e)
        {
            MessageBox.Show(this, e.Message, "Không thể hoàn tất dọn phòng, thử lại sau", MessageBoxButtons.OK);
            LoadCleaningRoom();
        }
    }
    //load toàn bộ phòng đang dọn
    private void LoadCleaningRoom()
    {
        try
        {
            var rooms = _roomService.GetRoomsForCleaning();
            _grid.Rows.Clear();
            foreach (var room in rooms)
            {
                int index = _grid.Rows.Add(room.RoomCode, $"{room.FloorName}",
                    $"{room.TypeName}", room.Note ?? "");
                _grid.Rows[index].Tag = room;
            }

            ShowSelectedRoom();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,ex.Message, "Không tải được danh sách phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private static Button MakeButton(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
        button.Click +=(_,_) => action();
        return button;
    }
}
