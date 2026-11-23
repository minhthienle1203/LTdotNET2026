using System.Drawing;
using System.Windows.Forms;
using HotelManagement.Core.Enums;

namespace HotelManagement.WinForms.Forms.Reservations;

public sealed class ReservationListView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly List<ReservationItem> allItems;

    private readonly BindingSource bindingSource = new();
    private readonly DataGridView grid = new();
    private readonly Label[] summaryLabels = new Label[5];

    private readonly TextBox keywordBox = new();
    private readonly DateTimePicker fromPicker = new();
    private readonly DateTimePicker toPicker = new();
    private readonly ComboBox roomTypeBox = new();
    private readonly ComboBox statusBox = new();

    private readonly Label detailLabel = new();
    private readonly Button checkInButton = new();
    private readonly Button editButton = new();
    private readonly Button cancelButton = new();

    public ReservationListView(Action<string, object?> navigate, object? context = null)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        allItems = LoadReservations();

        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        Height = 650;
        MinimumSize = new Size(0, 650);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(BuildHeader(), 0, 0);
        layout.Controls.Add(BuildSummary(), 0, 1);
        layout.Controls.Add(BuildFilters(), 0, 2);
        layout.Controls.Add(BuildMainArea(), 0, 3);
        Controls.Add(layout);

        bindingSource.CurrentChanged += (_, _) => ShowDetail();

        InitializeFilters();
        ApplyFilter(context is int reservationId ? reservationId : null);
    }


    private Control BuildHeader()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label
        {
            Text = "DANH SÁCH ĐẶT PHÒNG",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Margin = new Padding(0, 5, 16, 0)
        });
        panel.Controls.Add(CreateButton("Tạo đặt phòng mới", () => navigate("S10", null)));
        panel.Controls.Add(CreateButton("Làm mới", () => ApplyFilter()));
        return panel;
    }

    private Control BuildSummary()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1 };
        for (var i = 0; i < summaryLabels.Length; i++)
        {
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            summaryLabels[i] = new Label
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                Margin = new Padding(0, 0, 6, 0)
            };
            panel.Controls.Add(summaryLabels[i], i, 0);
        }

        return panel;
    }

    private Control BuildFilters()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };

        keywordBox.Width = 230;
        keywordBox.PlaceholderText = "Mã đặt phòng, tên khách, SĐT";
        keywordBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            ApplyFilter();
        };

        ConfigureDatePicker(fromPicker);
        ConfigureDatePicker(toPicker);

        roomTypeBox.Width = 170;
        roomTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        statusBox.Width = 150;
        statusBox.DropDownStyle = ComboBoxStyle.DropDownList;

        panel.Controls.Add(CreateLabel("Tìm kiếm"));
        panel.Controls.Add(keywordBox);
        panel.Controls.Add(CreateLabel("Ngày đến từ"));
        panel.Controls.Add(fromPicker);
        panel.Controls.Add(CreateLabel("đến"));
        panel.Controls.Add(toPicker);
        panel.Controls.Add(CreateLabel("Loại phòng"));
        panel.Controls.Add(roomTypeBox);
        panel.Controls.Add(CreateLabel("Trạng thái"));
        panel.Controls.Add(statusBox);
        panel.Controls.Add(CreateButton("Lọc dữ liệu", () => ApplyFilter()));
        panel.Controls.Add(CreateButton("Xóa lọc", ClearFilters));
        return panel;
    }

    private Control BuildMainArea()
    {
        var area = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        area.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
        area.Controls.Add(BuildGrid(), 0, 0);
        area.Controls.Add(BuildDetailPanel(), 1, 0);
        return area;
    }

    private Control BuildGrid()
    {
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.RowHeadersVisible = false;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = SystemColors.Window;

        AddColumn("Mã đặt phòng", nameof(ReservationItem.ReservationCode));
        AddColumn("Khách hàng", nameof(ReservationItem.CustomerName));
        AddColumn("Ngày giờ đến", nameof(ReservationItem.ExpectedCheckIn), "dd/MM/yyyy HH:mm");
        AddColumn("Ngày giờ đi", nameof(ReservationItem.ExpectedCheckOut), "dd/MM/yyyy HH:mm");
        AddColumn("Loại phòng", nameof(ReservationItem.RoomTypeName));
        AddColumn("Phòng được gán", nameof(ReservationItem.RoomText));
        AddColumn("Tiền cọc", nameof(ReservationItem.DepositAmount), "N0");
        AddColumn("Trạng thái", nameof(ReservationItem.StatusText));

        grid.DataSource = bindingSource;
        grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0) EditSelected();
        };
        return grid;
    }

    private Control BuildDetailPanel()
    {
        var panel = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(8, 0, 0, 0) };
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        detailLabel.Dock = DockStyle.Fill;
        detailLabel.BorderStyle = BorderStyle.FixedSingle;
        detailLabel.Padding = new Padding(8);

        checkInButton.Text = "Check-in";
        checkInButton.Click += (_, _) => CheckInSelected();
        editButton.Text = "Xem / Sửa";
        editButton.Click += (_, _) => EditSelected();
        cancelButton.Text = "Hủy đặt phòng";
        cancelButton.Click += (_, _) => CancelSelected();

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        foreach (var button in new[] { checkInButton, editButton, cancelButton })
        {
            button.AutoSize = true;
            buttons.Controls.Add(button);
        }

        panel.Controls.Add(detailLabel, 0, 0);
        panel.Controls.Add(buttons, 0, 1);
        return panel;
    }

    private void AddColumn(string header, string property, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property };
        if (format != null) column.DefaultCellStyle.Format = format;
        grid.Columns.Add(column);
    }

    private static void ConfigureDatePicker(DateTimePicker picker)
    {
        picker.Format = DateTimePickerFormat.Short;
        picker.Width = 115;
        picker.ShowCheckBox = true;
        picker.Checked = false;
    }

    private static Label CreateLabel(string text)
        => new() { Text = text, AutoSize = true, Margin = new Padding(10, 8, 4, 0) };

    private static Button CreateButton(string text, Action onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(5) };
        button.Click += (_, _) => onClick();
        return button;
    }



    private List<ReservationItem> LoadReservations()
    {
        return CreateSampleReservations();
    }

    private static List<ReservationItem> CreateSampleReservations() => new()
    {
        new(1, "RSV-202611-001", "Nguyễn Hoàng", "0901000001", new DateTime(2026, 11, 16, 14, 0, 0), new DateTime(2026, 11, 17, 12, 0, 0), "Deluxe", "103", 100000, ReservationStatus.Confirmed, "Đặt trước phòng 103"),
        new(2, "RSV-202611-002", "Trần Quốc Bảo", "0901000002", new DateTime(2026, 11, 18, 14, 0, 0), new DateTime(2026, 11, 19, 12, 0, 0), "Deluxe", "203", 150000, ReservationStatus.Confirmed, "Đặt trước phòng 203"),
        new(3, "RSV-202611-003", "Lê Thu Hà", "0901000003", new DateTime(2026, 11, 20, 14, 0, 0), new DateTime(2026, 11, 21, 12, 0, 0), "Deluxe", "303", 100000, ReservationStatus.Confirmed, "Đặt trước phòng 303"),
        new(4, "RSV-202611-004", "Trần Quốc Bảo", "0901000002", new DateTime(2026, 11, 22, 9, 0, 0), new DateTime(2026, 11, 23, 12, 0, 0), "Standard", null, 0, ReservationStatus.Confirmed, "Chỉ giữ chỗ theo loại phòng"),
        new(5, "RSV-202611-005", "Nguyễn Hoàng", "0901000001", new DateTime(2026, 11, 10, 14, 0, 0), new DateTime(2026, 11, 12, 12, 0, 0), "Standard", "102", 200000, ReservationStatus.CheckedIn, null),
        new(6, "RSV-202611-006", "Lê Thu Hà", "0901000003", new DateTime(2026, 11, 5, 14, 0, 0), new DateTime(2026, 11, 7, 12, 0, 0), "Deluxe", "104", 0, ReservationStatus.Completed, null),
        new(7, "RSV-202611-007", "Trần Quốc Bảo", "0901000002", new DateTime(2026, 11, 8, 14, 0, 0), new DateTime(2026, 11, 9, 12, 0, 0), "Standard", null, 0, ReservationStatus.Cancelled, "Khách hủy")
    };

    private void InitializeFilters()
    {
        roomTypeBox.Items.AddRange(new object[] { "Tất cả loại phòng", "Standard", "Deluxe" });
        roomTypeBox.SelectedIndex = 0;

        statusBox.Items.AddRange(new object[]
        {
            "Tất cả trạng thái", "Đã xác nhận", "Đã nhận phòng", "Hoàn tất", "Đã hủy", "Không đến"
        });
        statusBox.SelectedIndex = 0;
    }
    private void ApplyFilter(int? selectId = null)
    {
        var keepId = selectId ?? Selected?.ReservationId;

        if (fromPicker.Checked && toPicker.Checked && fromPicker.Value.Date > toPicker.Value.Date)
        {
            MessageBox.Show(this, "'Ngày đến từ' không được lớn hơn 'đến'.", "Bộ lọc không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var keyword = keywordBox.Text.Trim();
        var query = allItems.AsEnumerable();

        if (keyword.Length > 0)
            query = query.Where(x =>
                x.ReservationCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.CustomerPhone.Contains(keyword));
        if (fromPicker.Checked)
            query = query.Where(x => x.ExpectedCheckIn.Date >= fromPicker.Value.Date);
        if (toPicker.Checked)
            query = query.Where(x => x.ExpectedCheckIn.Date <= toPicker.Value.Date);
        if (roomTypeBox.SelectedIndex > 0)
            query = query.Where(x => x.RoomTypeName == roomTypeBox.Text);
        if (statusBox.SelectedIndex > 0)
            query = query.Where(x => x.Status == (ReservationStatus)(statusBox.SelectedIndex - 1));

        var list = query.OrderBy(x => x.ExpectedCheckIn).ToList();
        bindingSource.DataSource = list;
        UpdateSummary(list);

        var index = keepId.HasValue ? list.FindIndex(x => x.ReservationId == keepId.Value) : -1;
        if (index >= 0) bindingSource.Position = index;
        ShowDetail();
    }

    private void ClearFilters()
    {
        keywordBox.Clear();
        fromPicker.Checked = false;
        toPicker.Checked = false;
        roomTypeBox.SelectedIndex = 0;
        statusBox.SelectedIndex = 0;
        ApplyFilter();
    }

    private void UpdateSummary(List<ReservationItem> list)
    {
        var confirmed = list.Where(x => x.Status == ReservationStatus.Confirmed).ToList();

        summaryLabels[0].Text = $"Tổng đặt phòng\n{list.Count}";
        summaryLabels[1].Text = $"Đã xác nhận\n{confirmed.Count}";
        summaryLabels[2].Text = $"Đến trong hôm nay\n{confirmed.Count(x => x.ExpectedCheckIn.Date == DateTime.Today)}";
        summaryLabels[3].Text = $"Đã nhận phòng\n{list.Count(x => x.Status == ReservationStatus.CheckedIn)}";
        summaryLabels[4].Text = $"Tiền cọc đang giữ\n{confirmed.Sum(x => x.DepositAmount):N0} đ";
    }

    private ReservationItem? Selected => bindingSource.Current as ReservationItem;

    private void ShowDetail()
    {
        var item = Selected;
        if (item == null)
        {
            detailLabel.Text = "Chọn một đặt phòng để xem chi tiết.";
            checkInButton.Enabled = false;
            editButton.Enabled = false;
            cancelButton.Enabled = false;
            return;
        }

        detailLabel.Text =
            $"Mã đặt phòng: {item.ReservationCode}\n" +
            $"Trạng thái: {item.StatusText}\n\n" +
            $"Khách hàng: {item.CustomerName}\n" +
            $"Điện thoại: {item.CustomerPhone}\n\n" +
            $"Dự kiến đến: {item.ExpectedCheckIn:dd/MM/yyyy HH:mm}\n" +
            $"Dự kiến đi: {item.ExpectedCheckOut:dd/MM/yyyy HH:mm}\n\n" +
            $"Loại phòng: {item.RoomTypeName}\n" +
            $"Phòng được gán: {item.RoomText}\n" +
            $"Tiền cọc: {item.DepositAmount:N0} đ\n\n" +
            $"Ghi chú: {item.Note}";

        var isConfirmed = item.Status == ReservationStatus.Confirmed;
        checkInButton.Enabled = isConfirmed;
        cancelButton.Enabled = isConfirmed;
        editButton.Enabled = true;
    }

    private void EditSelected()
    {
        if (Selected == null) return;
        navigate("S10", Selected.ReservationId);
    }

    private void CheckInSelected()
    {
        if (Selected == null) return;
        navigate("S11", Selected.ReservationId);
    }


}
