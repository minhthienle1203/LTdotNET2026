using System.Drawing;
using System.Windows.Forms;
using HotelManagement.Core.Enums;

namespace HotelManagement.WinForms.Forms.Reservations;

public sealed class ReservationFormView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly int? reservationId;
    private bool loading;

    private readonly ErrorProvider errorProvider = new();

    private readonly ComboBox customerBox = new();
    private readonly Label customerDetailLabel = new();
    private readonly DateTimePicker checkInPicker = new();
    private readonly DateTimePicker checkOutPicker = new();
    private readonly Label durationLabel = new();
    private readonly ComboBox roomTypeBox = new();
    private readonly Label availabilityLabel = new();
    private readonly ComboBox roomBox = new();
    private readonly NumericUpDown depositBox = new();
    private readonly TextBox noteBox = new();
    private readonly Label titleLabel = new();
    private readonly Label summaryLabel = new();

    public ReservationFormView(Action<string, object?> navigate, object? context = null)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        reservationId = context is int id ? id : null;

        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        Height = 780;
        MinimumSize = new Size(0, 780);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.Controls.Add(BuildHeader(), 0, 0);
        layout.Controls.Add(BuildBody(), 0, 1);
        layout.Controls.Add(BuildButtons(), 0, 2);
        Controls.Add(layout);

        errorProvider.ContainerControl = this;

        customerBox.SelectedIndexChanged += (_, _) => UpdateAll();
        checkInPicker.ValueChanged += (_, _) => RefreshRooms();
        checkOutPicker.ValueChanged += (_, _) => RefreshRooms();
        roomBox.SelectedIndexChanged += (_, _) => UpdateAll();
        depositBox.ValueChanged += (_, _) => UpdateAll();
        roomTypeBox.SelectedIndexChanged += (_, _) =>
        {
            if (loading) return;
            FillRooms();
            availabilityLabel.Text = "Kiểm tra.";
            UpdateAll();
        };

        LoadForm();
    }


    private Control BuildHeader()
    {
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Ariel", 15, FontStyle.Bold);
        titleLabel.Margin = new Padding(0, 5, 0, 0);

        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(titleLabel);
        return panel;
    }

    private Control BuildBody()
    {
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        body.Controls.Add(BuildGroups(), 0, 0);
        body.Controls.Add(BuildSummary(), 1, 0);
        return body;
    }

    private Control BuildGroups()
    {
        var groups = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
        groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var height in new[] { 120, 95, 100, 85, 70 })
            groups.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        groups.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        groups.Controls.Add(BuildCustomerGroup(), 0, 0);
        groups.Controls.Add(BuildTimeGroup(), 0, 1);
        groups.Controls.Add(BuildRoomTypeGroup(), 0, 2);
        groups.Controls.Add(BuildRoomGroup(), 0, 3);
        groups.Controls.Add(BuildDepositGroup(), 0, 4);
        groups.Controls.Add(BuildNoteGroup(), 0, 5);
        return groups;
    }


    private Control BuildCustomerGroup()
    {
        var group = CreateGroup("1. Thông tin khách hàng", out var flow);

        customerBox.Width = 300;
        customerBox.DropDownStyle = ComboBoxStyle.DropDownList;
        customerBox.DisplayMember = nameof(CustomerOption.Text);

        var findButton = CreateButton("Tìm khách", () => navigate("S07", null));
        var newButton = CreateButton("Tạo khách mới", () => navigate("S08", null));
        flow.Controls.Add(CreateLabel("Khách hàng *"));
        flow.Controls.Add(customerBox);
        flow.Controls.Add(findButton);
        flow.Controls.Add(newButton);
        flow.SetFlowBreak(newButton, true);

        customerDetailLabel.AutoSize = true;
        customerDetailLabel.Margin = new Padding(10, 8, 0, 0);
        flow.Controls.Add(customerDetailLabel);
        return group;
    }

    private Control BuildTimeGroup()
    {
        var group = CreateGroup("2. Thời gian lưu trú dự kiến", out var flow);

        ConfigureDateTimePicker(checkInPicker);
        ConfigureDateTimePicker(checkOutPicker);

        flow.Controls.Add(CreateLabel("Nhận phòng *"));
        flow.Controls.Add(checkInPicker);
        flow.Controls.Add(CreateLabel("Trả phòng *"));
        flow.Controls.Add(checkOutPicker);
        flow.SetFlowBreak(checkOutPicker, true);

        durationLabel.AutoSize = true;
        durationLabel.Margin = new Padding(10, 8, 0, 0);
        flow.Controls.Add(durationLabel);
        return group;
    }

    private Control BuildRoomTypeGroup()
    {
        var group = CreateGroup("3. Loại phòng và kiểm tra khả dụng", out var flow);

        roomTypeBox.Width = 250;
        roomTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        roomTypeBox.DisplayMember = nameof(RoomTypeOption.Text);

        var checkButton = CreateButton("Kiểm tra khả dụng", CheckAvailability);
        flow.Controls.Add(CreateLabel("Loại phòng *"));
        flow.Controls.Add(roomTypeBox);
        flow.Controls.Add(checkButton);
        flow.SetFlowBreak(checkButton, true);

        availabilityLabel.AutoSize = true;
        availabilityLabel.Margin = new Padding(10, 8, 0, 0);
        flow.Controls.Add(availabilityLabel);
        return group;
    }

    private Control BuildRoomGroup()
    {
        var group = CreateGroup("4. Chỉ định phòng cụ thể (không bắt buộc)", out var flow);

        roomBox.Width = 250;
        roomBox.DropDownStyle = ComboBoxStyle.DropDownList;
        roomBox.DisplayMember = nameof(RoomOption.Text);

        flow.Controls.Add(CreateLabel("Phòng"));
        flow.Controls.Add(roomBox);
        flow.SetFlowBreak(roomBox, true);
        flow.Controls.Add(new Label
        {
            Text = ".Chọn phòng = Đã đặt.",
            AutoSize = true,
            Margin = new Padding(10, 6, 0, 0)
        });
        return group;
    }
    private Control BuildDepositGroup()
    {
        var group = CreateGroup("5. Tiền đặt cọc", out var flow);

        depositBox.Width = 150;
        depositBox.Minimum = 0;
        depositBox.Maximum = 1000000000;
        depositBox.Increment = 50000;
        depositBox.ThousandsSeparator = true;

        flow.Controls.Add(CreateLabel("Tiền cọc đã nhận (VND)"));
        flow.Controls.Add(depositBox);
        return group;
    }

    private Control BuildNoteGroup()
    {
        var group = new GroupBox { Text = "6. Ghi chú đặt phòng", Dock = DockStyle.Fill };
        noteBox.Dock = DockStyle.Fill;
        noteBox.Multiline = true;
        noteBox.ScrollBars = ScrollBars.Vertical;
        group.Controls.Add(noteBox);
        return group;
    }

    private Control BuildSummary()
    {
        var group = new GroupBox { Text = "Tóm tắt phiếu đặt phòng", Dock = DockStyle.Fill, Margin = new Padding(8, 3, 0, 3) };
        summaryLabel.Dock = DockStyle.Fill;
        summaryLabel.Padding = new Padding(4);
        group.Controls.Add(summaryLabel);
        return group;
    }

    private Control BuildButtons()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        panel.Controls.Add(CreateButton("Lưu đặt phòng", SaveReservation));
        panel.Controls.Add(CreateButton("Hủy", () => navigate("S09", null)));
        panel.Controls.Add(CreateButton("Làm lại", LoadForm));
        return panel;
    }

    private static GroupBox CreateGroup(string title, out FlowLayoutPanel flow)
    {
        flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill };
        group.Controls.Add(flow);
        return group;
    }

    private static void ConfigureDateTimePicker(DateTimePicker picker)
    {
        picker.Format = DateTimePickerFormat.Custom;
        picker.CustomFormat = "dd/MM/yyyy HH:mm";
        picker.Width = 150;
    }

    private static Label CreateLabel(string text)
        => new() { Text = text, AutoSize = true, Margin = new Padding(10, 8, 4, 0) };

    private static Button CreateButton(string text, Action onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(5) };
        button.Click += (_, _) => onClick();
        return button;
    }

    private void LoadForm()
    {
        loading = true;
        errorProvider.Clear();

        customerBox.Items.Clear();
        customerBox.Items.AddRange(Customers);
        roomTypeBox.Items.Clear();
        roomTypeBox.Items.AddRange(RoomTypes);

        var sample = SampleReservations.FirstOrDefault(x => x.Id == reservationId);
        if (sample == null)
        {
            titleLabel.Text = "TẠO ĐẶT PHÒNG MỚI";
            customerBox.SelectedIndex = -1;
            roomTypeBox.SelectedIndex = 0;
            checkInPicker.Value = DateTime.Today.AddHours(14);
            checkOutPicker.Value = DateTime.Today.AddDays(1).AddHours(12);
            FillRooms();
            depositBox.Value = 0;
            noteBox.Clear();
            availabilityLabel.Text = "Kiểm tra khả dụng.";
        }
        else
        {
            titleLabel.Text = $"SỬA ĐẶT PHÒNG {sample.Code}";
            customerBox.SelectedIndex = sample.CustomerIndex;
            roomTypeBox.SelectedIndex = sample.TypeIndex;
            checkInPicker.Value = sample.CheckIn;
            checkOutPicker.Value = sample.CheckOut;
            FillRooms(sample.RoomCode);
            depositBox.Value = sample.Deposit;
            noteBox.Text = sample.Note;
            availabilityLabel.Text = "Kiểm tra khả dụng.";
        }

        loading = false;
        UpdateAll();
    }

    private IEnumerable<RoomInfo> FreeRooms(RoomTypeOption type)
    {
        var from = checkInPicker.Value;
        var to = checkOutPicker.Value;
        return type.Rooms.Where(room =>
            !(room.Status == RoomStatus.Occupied && from <= DateTime.Now) &&
            !SampleReservations.Any(x =>
                x.Id != reservationId && x.RoomCode == room.Code && x.CheckIn < to && x.CheckOut > from));
    }

    private void FillRooms(string? keepCode = null)
    {
        roomBox.Items.Clear();
        roomBox.Items.Add(new RoomOption(null, null));
        if (roomTypeBox.SelectedItem is RoomTypeOption type)
        {
            foreach (var room in FreeRooms(type))
                roomBox.Items.Add(new RoomOption(room.Code, room.Status));
        }

        var index = roomBox.Items.Cast<RoomOption>().ToList().FindIndex(x => x.Code == keepCode);
        roomBox.SelectedIndex = keepCode != null && index >= 0 ? index : 0;
    }



}



