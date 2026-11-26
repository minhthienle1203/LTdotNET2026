using System.Drawing;
using System.Windows.Forms;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Reservations;


public sealed class ReservationFormView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly ReservationService reservationService;
    private readonly int? requestedId;

    private Reservation? editing;
    private bool loading;

    public decimal RoomTotal { get; private set; }
    public decimal RoomRemaining { get; private set; }

    private readonly ErrorProvider errorProvider = new();

    private readonly ComboBox customerBox = new();
    private readonly Label customerDetailLabel = new();
    private readonly DateTimePicker checkInPicker = new();
    private readonly DateTimePicker checkOutPicker = new();
    private readonly Label durationLabel = new();
    private readonly ComboBox roomTypeBox = new();
    private readonly Label availabilityLabel = new();
    private readonly ComboBox roomBox = new();
    private readonly RadioButton byDayRadio = new();
    private readonly RadioButton byHourRadio = new();
    private readonly NumericUpDown depositBox = new();
    private readonly TextBox noteBox = new();
    private readonly Label titleLabel = new();
    private readonly Label summaryLabel = new();
    private readonly Button saveButton = new();


    public ReservationFormView(Action<string, object?> navigate, ReservationService reservationService, object? context = null)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        this.reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        requestedId = context is int id ? id : null;

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
        byDayRadio.CheckedChanged += (_, _) => UpdateAll();
        byHourRadio.CheckedChanged += (_, _) => UpdateAll();
        roomTypeBox.SelectedIndexChanged += (_, _) =>
        {
            if (loading) return;
            Run(() =>
            {
                FillRooms(null);
                availabilityLabel.Text = "Bấm 'Kiểm tra khả dụng' để xem số phòng trống.";
                UpdateAll();
            });
        };

        Run(LoadForm);
    }

    private Control BuildHeader()
    {
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Segoe UI", 15, FontStyle.Bold);
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
        var groups = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7 };
        groups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        foreach (var height in new[] { 120, 95, 100, 85, 60, 70 })
            groups.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        groups.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        groups.Controls.Add(BuildCustomerGroup(), 0, 0);
        groups.Controls.Add(BuildTimeGroup(), 0, 1);
        groups.Controls.Add(BuildRoomTypeGroup(), 0, 2);
        groups.Controls.Add(BuildRoomGroup(), 0, 3);
        groups.Controls.Add(BuildPricingGroup(), 0, 4);
        groups.Controls.Add(BuildDepositGroup(), 0, 5);
        groups.Controls.Add(BuildNoteGroup(), 0, 6);
        return groups;
    }

    private Control BuildCustomerGroup()
    {
        var group = CreateGroup("1. Thông tin khách hàng", out var flow);

        customerBox.Width = 300;
        customerBox.DropDownStyle = ComboBoxStyle.DropDownList;
        customerBox.DisplayMember = nameof(CustomerOptionDto.Text);

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

        roomTypeBox.Width = 400;
        roomTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        roomTypeBox.DisplayMember = nameof(RoomTypeOptionDto.Text);

        var checkButton = CreateButton("Kiểm tra khả dụng", () => Run(CheckAvailability));
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
        roomBox.DisplayMember = nameof(RoomOptionDto.Text);

        flow.Controls.Add(CreateLabel("Phòng"));
        flow.Controls.Add(roomBox);
        flow.SetFlowBreak(roomBox, true);
        flow.Controls.Add(new Label
        {
            Text = "Không chọn = chỉ giữ chỗ theo loại phòng. Chọn phòng = phòng chuyển sang 'Đã đặt' (Reserved).",
            AutoSize = true,
            Margin = new Padding(10, 6, 0, 0)
        });
        return group;
    }

    private Control BuildPricingGroup()
    {
        var group = CreateGroup("5. Cách tính tiền phòng", out var flow);

        byDayRadio.Text = "Theo ngày";
        byDayRadio.AutoSize = true;
        byDayRadio.Checked = true;
        byHourRadio.Text = "Theo giờ";
        byHourRadio.AutoSize = true;

        flow.Controls.Add(byDayRadio);
        flow.Controls.Add(byHourRadio);
        return group;
    }

    private Control BuildDepositGroup()
    {
        var group = CreateGroup("6. Tiền đặt cọc", out var flow);

        depositBox.Width = 150;
        depositBox.Minimum = 0;
        depositBox.Maximum = 1000000000;
        depositBox.Increment = 50000;
        depositBox.DecimalPlaces = 0;
        depositBox.ThousandsSeparator = true;

        flow.Controls.Add(CreateLabel("Tiền cọc đã nhận (VND)"));
        flow.Controls.Add(depositBox);
        return group;
    }

    private Control BuildNoteGroup()
    {
        var group = new GroupBox { Text = "7. Ghi chú đặt phòng", Dock = DockStyle.Fill };
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
        saveButton.Text = "Lưu đặt phòng";
        saveButton.AutoSize = true;
        saveButton.Margin = new Padding(5);
        saveButton.Click += (_, _) => Run(SaveReservation);

        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        panel.Controls.Add(saveButton);
        panel.Controls.Add(CreateButton("Hủy", () => navigate("S09", requestedId)));
        panel.Controls.Add(CreateButton("Làm lại", () => Run(LoadForm)));
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
        try
        {
            errorProvider.Clear();

            customerBox.Items.Clear();
            customerBox.Items.AddRange(reservationService.GetActiveCustomers().ToArray());
            roomTypeBox.Items.Clear();
            roomTypeBox.Items.AddRange(reservationService.GetRoomTypeOptions(true).ToArray());

            editing = requestedId.HasValue ? reservationService.GetReservation(requestedId.Value) : null;
            if (requestedId.HasValue && editing == null)
                MessageBox.Show(this, "Không tìm thấy đặt phòng cần sửa. Form chuyển sang chế độ tạo mới.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (editing == null)
            {
                titleLabel.Text = "TẠO ĐẶT PHÒNG MỚI";
                customerBox.SelectedIndex = -1;
                roomTypeBox.SelectedIndex = roomTypeBox.Items.Count > 0 ? 0 : -1;
                checkInPicker.Value = DateTime.Today.AddHours(14);
                checkOutPicker.Value = DateTime.Today.AddDays(1).AddHours(12);
                depositBox.Value = 0;
                noteBox.Clear();
                FillRooms(null);
            }
            else
            {
                titleLabel.Text = $"{(editing.Status == ReservationStatus.Confirmed ? "SỬA" : "XEM")} ĐẶT PHÒNG {editing.ReservationCode}";
                SelectItem<CustomerOptionDto>(customerBox, x => x.CustomerId == editing.CustomerId);
                SelectItem<RoomTypeOptionDto>(roomTypeBox, x => x.RoomTypeId == editing.RoomTypeId);
                checkInPicker.Value = editing.ExpectedCheckIn;
                checkOutPicker.Value = editing.ExpectedCheckOut;
                depositBox.Value = editing.DepositAmount;
                noteBox.Text = editing.Note ?? string.Empty;
                FillRooms(editing.RoomId);
            }

            availabilityLabel.Text = "Bấm 'Kiểm tra khả dụng' để xem số phòng trống.";
            var editable = editing == null || editing.Status == ReservationStatus.Confirmed;
            foreach (var control in new Control[] { customerBox, checkInPicker, checkOutPicker, roomTypeBox, roomBox, depositBox, noteBox, saveButton })
                control.Enabled = editable;
        }
        finally
        {
            loading = false;
        }

        UpdateAll();
    }

    private static void SelectItem<T>(ComboBox box, Func<T, bool> match)
    {
        box.SelectedIndex = -1;
        for (var i = 0; i < box.Items.Count; i++)
        {
            if (box.Items[i] is T item && match(item))
            {
                box.SelectedIndex = i;
                return;
            }
        }
    }

    private List<RoomOptionDto> LoadFreeRooms()
    {
        if (roomTypeBox.SelectedItem is not RoomTypeOptionDto type)
            return new List<RoomOptionDto>();

        return reservationService.GetFreeRooms(type.RoomTypeId, TrimSeconds(checkInPicker.Value),
            TrimSeconds(checkOutPicker.Value), editing?.ReservationId);
    }
    private void FillRooms(int? keepRoomId)
    {
        roomBox.Items.Clear();
        roomBox.Items.Add(new RoomOptionDto());
        roomBox.Items.AddRange(LoadFreeRooms().ToArray());

        if (keepRoomId.HasValue)
            SelectItem<RoomOptionDto>(roomBox, x => x.RoomId == keepRoomId);
        if (roomBox.SelectedIndex < 0)
            roomBox.SelectedIndex = 0;
    }
    private void RefreshRooms()
    {
        if (loading) return;
        Run(() =>
        {
            FillRooms((roomBox.SelectedItem as RoomOptionDto)?.RoomId);
            UpdateAll();
        });
    }

    private void CheckAvailability()
    {
        if (roomTypeBox.SelectedItem is not RoomTypeOptionDto type)
        {
            availabilityLabel.Text = "Chưa chọn loại phòng.";
            return;
        }

        var result = reservationService.CheckAvailability(type.RoomTypeId, TrimSeconds(checkInPicker.Value),
            TrimSeconds(checkOutPicker.Value), editing?.ReservationId);

        availabilityLabel.Text = result.IsAvailable
            ? $"Còn {result.Available}/{result.TotalRooms} phòng trống trong khoảng thời gian đã chọn."
            : "Đã hết phòng trong khoảng thời gian đã chọn.";
    }


    private void UpdateAll()
    {
        if (loading) return;

        var customer = customerBox.SelectedItem as CustomerOptionDto;
        var type = roomTypeBox.SelectedItem as RoomTypeOptionDto;
        var room = roomBox.SelectedItem as RoomOptionDto;
        var checkIn = checkInPicker.Value;
        var checkOut = checkOutPicker.Value;
        var hours = RoomPriceCalculator.CountHours(checkIn, checkOut);
        var days = RoomPriceCalculator.CountDays(checkIn, checkOut);
        var byHour = type == null ? 0 : RoomPriceCalculator.ByHour(checkIn, checkOut, type.HourlyRate);
        var byDay = type == null ? 0 : RoomPriceCalculator.ByDay(checkIn, checkOut, type.DailyRate);
        var total = byHourRadio.Checked ? byHour : byDay;
        var remain = Math.Max(0, total - depositBox.Value);
        RoomTotal = total;
        RoomRemaining = remain;

        customerDetailLabel.Text = customer == null
            ? "Chưa chọn khách hàng."
            : $"{customer.IdentityText}   SĐT: {customer.Phone}   Email: {customer.Email ?? "(không có)"}";

        durationLabel.Text = hours > 0
            ? $"Thời lượng dự kiến: {hours} giờ (tương đương {days} ngày)"
            : "Thời gian không hợp lệ: giờ trả phải sau giờ nhận.";

        summaryLabel.Text =
            $"Khách đại diện: {customer?.FullName ?? "(chưa chọn)"}\n\n" +
            $"Nhận phòng: {checkInPicker.Value:dd/MM/yyyy HH:mm}\n" +
            $"Trả phòng: {checkOutPicker.Value:dd/MM/yyyy HH:mm}\n" +
            $"Thời gian: {hours} giờ ({days} ngày)\n\n" +
            $"Loại phòng: {type?.TypeName ?? "(chưa chọn)"}\n" +
            $"Phòng cụ thể: {(room?.RoomCode == null ? "(không gán)" : room.Text)}\n\n" +
            $"Theo giờ: {hours} giờ = {byHour:N0} đ\n" +
            $"Theo ngày: {days} ngày = {byDay:N0} đ\n" +
            $"Cách tính đã chọn: {(byHourRadio.Checked ? "Theo giờ" : "Theo ngày")}\n\n" +
            $"TỔNG TIỀN PHÒNG DỰ KIẾN: {total:N0} đ\n" +
            $"Tiền cọc đã nhận: {depositBox.Value:N0} đ\n" +
            $"Còn lại phải thu: {remain:N0} đ\n\n" +
            "Trạng thái khi lưu: Đã xác nhận (Confirmed)";
    }

    private bool ValidateForm()
    {
        errorProvider.Clear();

        var valid = true;
        valid &= Check(customerBox.SelectedItem is CustomerOptionDto, customerBox, "Vui lòng chọn khách hàng.");
        valid &= Check(checkOutPicker.Value > checkInPicker.Value, checkOutPicker, "Giờ trả phòng phải sau giờ nhận phòng.");
        valid &= Check(roomTypeBox.SelectedItem is RoomTypeOptionDto, roomTypeBox, "Vui lòng chọn loại phòng.");
        return valid;
    }

    private bool Check(bool condition, Control control, string message)
    {
        if (!condition) errorProvider.SetError(control, message);
        return condition;
    }

    private void SaveReservation()
    {
        if (!ValidateForm()) return;

        var customer = (CustomerOptionDto)customerBox.SelectedItem!;
        var type = (RoomTypeOptionDto)roomTypeBox.SelectedItem!;
        var room = roomBox.SelectedItem as RoomOptionDto;

        var reservation = new Reservation
        {
            ReservationId = editing?.ReservationId ?? 0,
            CustomerId = customer.CustomerId,
            RoomTypeId = type.RoomTypeId,
            RoomId = room?.RoomId,
            ExpectedCheckIn = TrimSeconds(checkInPicker.Value),
            ExpectedCheckOut = TrimSeconds(checkOutPicker.Value),
            DepositAmount = depositBox.Value,
            Note = noteBox.Text
        };

        int savedId;
        if (editing == null)
        {
            savedId = reservationService.CreateReservation(reservation);
        }
        else
        {
            reservationService.UpdateReservation(reservation);
            savedId = reservation.ReservationId;
        }

        MessageBox.Show(this, "Đã lưu đặt phòng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        navigate("S09", savedId);
    }

    private static DateTime TrimSeconds(DateTime value)
        => new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (BusinessRuleException ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Lỗi hệ thống:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
