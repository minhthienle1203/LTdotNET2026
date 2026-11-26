using System.Drawing;
using System.Windows.Forms;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Reservations;
public sealed class ReservationListView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly ReservationService reservationService;

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
    public ReservationListView(Action<string, object?> navigate, ReservationService reservationService, object? context = null)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        this.reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));

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
        LoadData(context is int reservationId ? reservationId : null);
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
        panel.Controls.Add(CreateButton("Làm mới", () => LoadData()));
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
            LoadData();
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
        panel.Controls.Add(CreateButton("Lọc dữ liệu", () => LoadData()));
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

        AddColumn("Mã đặt phòng", nameof(ReservationListItemDto.ReservationCode));
        AddColumn("Khách hàng", nameof(ReservationListItemDto.CustomerName));
        AddColumn("Ngày giờ đến", nameof(ReservationListItemDto.ExpectedCheckIn), "dd/MM/yyyy HH:mm");
        AddColumn("Ngày giờ đi", nameof(ReservationListItemDto.ExpectedCheckOut), "dd/MM/yyyy HH:mm");
        AddColumn("Loại phòng", nameof(ReservationListItemDto.RoomTypeName));
        AddColumn("Phòng được gán", nameof(ReservationListItemDto.RoomText));
        AddColumn("Tiền cọc", nameof(ReservationListItemDto.DepositAmount), "N0");
        AddColumn("Trạng thái", nameof(ReservationListItemDto.StatusText));

        grid.DataSource = bindingSource;
        grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0) EditSelected();
        };
        return grid;
    }

    private Control BuildDetailPanel()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(8, 0, 0, 0) };
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
    private void InitializeFilters()
    {
        statusBox.Items.AddRange(new object[]
        {
            "Tất cả trạng thái", "Đã xác nhận", "Đã nhận phòng", "Hoàn tất", "Đã hủy", "Không đến"
        });
        statusBox.SelectedIndex = 0;

        try
        {
            var roomTypes = new List<RoomTypeOptionDto> { new() { RoomTypeId = 0, TypeName = "Tất cả loại phòng" } };
            roomTypes.AddRange(reservationService.GetRoomTypeOptions(false));

            roomTypeBox.DisplayMember = nameof(RoomTypeOptionDto.TypeName);
            roomTypeBox.ValueMember = nameof(RoomTypeOptionDto.RoomTypeId);
            roomTypeBox.DataSource = roomTypes;
        }
        catch (Exception ex)
        {
            ShowError("Không tải được danh sách loại phòng.", ex);
        }
    }

    private ReservationFilter BuildFilter() => new()
    {
        Keyword = string.IsNullOrWhiteSpace(keywordBox.Text) ? null : keywordBox.Text.Trim(),
        FromDate = fromPicker.Checked ? fromPicker.Value.Date : null,
        ToDate = toPicker.Checked ? toPicker.Value.Date : null,
        RoomTypeId = roomTypeBox.SelectedValue is int typeId && typeId > 0 ? typeId : null,
        Status = statusBox.SelectedIndex > 0 ? (ReservationStatus)(statusBox.SelectedIndex - 1) : null
    };
    private void LoadData(int? selectId = null)
    {
        var keepId = selectId ?? Selected?.ReservationId;

        try
        {
            var list = reservationService.SearchReservations(BuildFilter());
            bindingSource.DataSource = list;
            UpdateSummary(list);

            var index = keepId.HasValue ? list.FindIndex(x => x.ReservationId == keepId.Value) : -1;
            if (index >= 0) bindingSource.Position = index;
            ShowDetail();
        }
        catch (BusinessRuleException ex)
        {
            MessageBox.Show(this, ex.Message, "Bộ lọc không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            ShowError("Không tải được danh sách đặt phòng.", ex);
        }
    }

    private void ClearFilters()
    {
        keywordBox.Clear();
        fromPicker.Checked = false;
        toPicker.Checked = false;
        roomTypeBox.SelectedIndex = 0;
        statusBox.SelectedIndex = 0;
        LoadData();
    }

    private void UpdateSummary(List<ReservationListItemDto> list)
    {
        var confirmed = list.Where(x => x.Status == ReservationStatus.Confirmed).ToList();

        summaryLabels[0].Text = $"Tổng đặt phòng\n{list.Count}";
        summaryLabels[1].Text = $"Đã xác nhận\n{confirmed.Count}";
        summaryLabels[2].Text = $"Đến trong hôm nay\n{confirmed.Count(x => x.ExpectedCheckIn.Date == DateTime.Today)}";
        summaryLabels[3].Text = $"Đã nhận phòng\n{list.Count(x => x.Status == ReservationStatus.CheckedIn)}";
        summaryLabels[4].Text = $"Tiền cọc đang giữ\n{confirmed.Sum(x => x.DepositAmount):N0} đ";
    }

    private ReservationListItemDto? Selected => bindingSource.Current as ReservationListItemDto;

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

        var hours = item.TotalHours;
        var days = item.TotalDays;
        var byHour = item.RoomAmountByHour;
        var byDay = item.RoomAmountByDay;

        detailLabel.Text =
            $"Mã đặt phòng: {item.ReservationCode}\n" +
            $"Trạng thái: {item.StatusText}\n\n" +
            $"Khách hàng: {item.CustomerName}\n" +
            $"Điện thoại: {item.CustomerPhone}\n\n" +
            $"Dự kiến đến: {item.ExpectedCheckIn:dd/MM/yyyy HH:mm}\n" +
            $"Dự kiến đi: {item.ExpectedCheckOut:dd/MM/yyyy HH:mm}\n\n" +
            $"Loại phòng: {item.RoomTypeName}\n" +
            $"Phòng được gán: {item.RoomText}\n" +
            $"Thời gian: {hours} giờ ({days} ngày)\n" +
            $"Tổng tiền phòng nếu tính theo giờ: {byHour:N0} đ\n" +
            $"Tổng tiền phòng nếu tính theo ngày: {byDay:N0} đ\n" +
            $"Tiền cọc: {item.DepositAmount:N0} đ\n" +
            $"Còn lại (tính theo ngày): {Math.Max(0, byDay - item.DepositAmount):N0} đ\n\n" +
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

    private void CancelSelected()
    {
        var item = Selected;
        if (item == null) return;

        var answer = MessageBox.Show(this,
            $"Hủy đặt phòng {item.ReservationCode} của khách {item.CustomerName}?",
            "Xác nhận hủy", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        try
        {
            reservationService.CancelReservation(item.ReservationId);
            LoadData(item.ReservationId);
        }
        catch (BusinessRuleException ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể hủy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            ShowError("Hủy đặt phòng thất bại.", ex);
        }
    }

    private void ShowError(string message, Exception ex)
    {
        MessageBox.Show(this, $"{message}\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
