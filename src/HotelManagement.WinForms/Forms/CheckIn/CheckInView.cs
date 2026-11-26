using System.Drawing.Printing;
using System.Globalization;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.CheckIn;

public sealed class CheckInView : UserControl
{
    private static readonly Color Navy = Color.FromArgb(17, 28, 49);
    private static readonly Color Blue = Color.FromArgb(0, 74, 198);
    private static readonly Color Pale = Color.FromArgb(236, 242, 252);
    private static readonly Color Page = Color.FromArgb(246, 248, 252);
    private static readonly Color Green = Color.FromArgb(22, 128, 61);
    private static readonly Color Red = Color.FromArgb(185, 28, 28);
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly CheckInService _service;
    private CheckInMode _mode = CheckInMode.Reservation;
    private CheckInReservationDto? _booking;
    private Customer? _customer;
    private CheckInEvaluationDto? _evaluation;
    private string? _receipt;
    private bool _loading;

    public event EventHandler? RoomMapRequested;
    public event EventHandler? CurrentStaysRequested;


    private readonly Button _btnReservationMode = ModeButton();
    private readonly Button _btnWalkInMode = ModeButton();
    private Control _bookingCard = null!;
    private readonly TextBox _txtSearch = new() { Width = 300, PlaceholderText = "Mã đặt phòng, tên, SĐT, số phòng" };
    private readonly ComboBox _cboStatus = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _grid = new();

    private readonly Label _lblBookingState = new()
        { AutoSize = true, Font = Bold(10), Margin = new Padding(3, 8, 12, 3) };

    private readonly Label _lblBookingDetail = Info("");
    private readonly Button _btnCancelBooking = MakeButton("Hủy đặt phòng", Color.White, Red);
    private readonly TextBox _txtName = ReadOnlyBox();
    private readonly TextBox _txtIdentity = ReadOnlyBox();
    private readonly TextBox _txtPhone = ReadOnlyBox();
    private readonly Label _lblCustomerMore = Info("");
    private readonly Button _btnFindCustomer = MakeButton("Tra cứu khách (F3)", Pale, Blue);
    private readonly Button _btnAddCustomer = MakeButton("+ Thêm khách mới", Pale, Blue);
    private readonly TextBox _txtCheckInTime = ReadOnlyBox();
    private readonly NumericUpDown _numGuests = new() { Minimum = 1, Maximum = 20, Value = 1, Dock = DockStyle.Top };

    private readonly DateTimePicker _dtCheckOut = new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Dock = DockStyle.Top
    };

    private readonly TextBox _txtNote = new() { Multiline = true, Height = 55, Dock = DockStyle.Top };
    private readonly Label _lblHeldRoom = Info("");
    private readonly ComboBox _cboRoom = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
    private readonly Label _lblRates = Info("");
    private readonly ComboBox _cboRoomType = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly Label _lblSummary = Info("");
    private readonly Label _lblChecklistTitle = new() { AutoSize = true, Font = Bold(10) };

    private readonly FlowLayoutPanel _checklist = new()
    {
        FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true
    };

    private readonly Label _lblResult = new() { AutoSize = true, MaximumSize = new Size(320, 0), Font = Bold(9.5f) };
    private readonly Button _btnConfirm = MakeButton("XÁC NHẬN NHẬN PHÒNG  [F10]", Color.Black, Color.White);
    private readonly Button _btnReset = MakeButton("Hủy bỏ (Esc)", Color.White, Navy);
    private readonly Button _btnPrint = MakeButton("In phiếu nhận phòng", Color.White, Navy);

    private sealed record RoomOption(CheckInRoomDto Room)
    {
        public override string ToString() => $"P.{Room.RoomCode} – {Room.FloorName} – {Room.TypeName} ({Room.Status})";
    }

    private sealed record TypeOption(int? Id, string Name)
    {
        public override string ToString() => Name;
    }

    private sealed record StatusOption(string Name, ReservationStatus? Value)
    {
        public override string ToString() => Name;
    }

    public CheckInView(CheckInService service)
    {
        _service = service;
        Dock = DockStyle.Fill;
        BackColor = Page;
        Font = new Font("Segoe UI", 9F);
        BuildLayout();
        WireEvents();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode) Guard(ResetForm);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.F2:
                Guard(() => SetMode(CheckInMode.Reservation));
                _txtSearch.Focus();
                return true;
            case Keys.F3:
                FindCustomer();
                return true;
            case Keys.F4:
                _cboRoom.Focus();
                if (_cboRoom.Enabled) _cboRoom.DroppedDown = true;
                return true;
            case Keys.F5:
                Guard(RefreshData);
                return true;
            case Keys.F10:
                Confirm();
                return true;
            case Keys.Escape when !_cboRoom.DroppedDown && !_cboStatus.DroppedDown:
                AskReset();
                return true;
            default: return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _errors.Dispose();
        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = Color.White, Padding = new Padding(12, 8, 12, 6)
        };
        header.Controls.Add(new Label
        {
            Text = "5. LỄ TÂN  ›  S11 - NHẬN PHÒNG (CHECK-IN)", AutoSize = true, ForeColor = Blue, Font = Bold(8.5f)
        });
        header.Controls.Add(
            new Label { Text = "Tiếp Nhận && Bàn Giao Phòng Lưu Trú", AutoSize = true, Font = Bold(13) });

        var btnMap = MakeButton("Sơ đồ phòng (S01)", Pale, Navy);
        var btnStays = MakeButton("Lưu trú hiện tại (S12)", Pale, Navy);
        var btnRefresh = MakeButton("Làm mới (F5)", Blue, Color.White);
        btnMap.Click += (_, _) => RoomMapRequested?.Invoke(this, EventArgs.Empty);
        btnStays.Click += (_, _) => CurrentStaysRequested?.Invoke(this, EventArgs.Empty);
        btnRefresh.Click += (_, _) => Guard(RefreshData);
        header.Controls.Add(Flow(
            Info("F10: Hoàn tất  |  F2: Chọn đặt phòng  |  F3: Tra cứu khách  |  F4: Đổi phòng  |  Esc: Hủy bỏ"),
            btnMap, btnStays, btnRefresh));

        var modes = new TableLayoutPanel
            { Dock = DockStyle.Top, Height = 66, ColumnCount = 2, Padding = new Padding(10, 4, 10, 4) };
        modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        modes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        modes.Controls.Add(_btnReservationMode, 0, 0);
        modes.Controls.Add(_btnWalkInMode, 1, 0);
        var left = Column();
        _bookingCard = Card("①  CHỌN ĐẶT PHÒNG  [F2]", BuildBookingSection());
        left.Controls.Add(_bookingCard);
        left.Controls.Add(Card("②  XÁC THỰC KHÁCH HÀNG && GIẤY TỜ",
            Flow(_btnFindCustomer, _btnAddCustomer),
            Fields(("Họ và tên khách đại diện", _txtName), ("Loại && số giấy tờ", _txtIdentity),
                ("Số điện thoại", _txtPhone)),
            _lblCustomerMore));
        left.Controls.Add(Card("③  THÔNG TIN LƯU TRÚ THỰC TẾ",
            Fields(("Thời điểm check-in (giờ hệ thống)", _txtCheckInTime), ("Số khách thực tế", _numGuests),
                ("Dự kiến trả phòng", _dtCheckOut)),
            Fields(("Ghi chú phục vụ && yêu cầu đặc biệt", _txtNote))));
        left.Controls.Add(Card("④  CHỈ ĐỊNH PHÒNG THỰC TẾ  [F4]",
            _lblHeldRoom,
            Fields(("Loại phòng (khách vãng lai)", _cboRoomType), ("Phòng bàn giao", _cboRoom)),
            _lblRates));
        _btnConfirm.AutoSize = false;
        _btnConfirm.Size = new Size(330, 48);
        _btnConfirm.Font = Bold(10.5f);
        var right = Column();
        right.Controls.Add(Card("TÓM TẮT PHIẾU NHẬN PHÒNG", _lblSummary));
        right.Controls.Add(Card("BỘ KIỂM TRA ĐIỀU KIỆN CHECK-IN", _lblChecklistTitle, _checklist));
        right.Controls.Add(Card("THỰC HIỆN", _lblResult, _btnConfirm, Flow(_btnReset, _btnPrint)));

        var body = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(6, 0, 6, 6) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(Scrollable(left), 0, 0);
        body.Controls.Add(Scrollable(right), 1, 0);
        Controls.Add(body);
        Controls.Add(modes);
        Controls.Add(header);
    }

    private Control[] BuildBookingSection()
    {
        _cboStatus.Items.Add(new StatusOption("Chờ nhận phòng", ReservationStatus.Confirmed));
        _cboStatus.Items.Add(new StatusOption("Đã nhận phòng", ReservationStatus.CheckedIn));
        _cboStatus.Items.Add(new StatusOption("Hoàn tất", ReservationStatus.Completed));
        _cboStatus.Items.Add(new StatusOption("Đã hủy", ReservationStatus.Cancelled));
        _cboStatus.Items.Add(new StatusOption("Khách không đến", ReservationStatus.NoShow));
        _cboStatus.Items.Add(new StatusOption("Tất cả trạng thái", null));
        _cboStatus.SelectedIndex = 0;

        _grid.Height = 125;
        _grid.Dock = DockStyle.Top;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White;
        foreach (var title in new[] { "Mã đặt", "Khách", "Phòng", "Dự kiến đến", "Dự kiến đi", "Trạng thái" })
            _grid.Columns.Add(title, title);

        return [Flow(_txtSearch, _cboStatus), _grid, Flow(_lblBookingState, _btnCancelBooking), _lblBookingDetail];
    }

    private void WireEvents()
    {
        _btnReservationMode.Click += (_, _) => Guard(() => SetMode(CheckInMode.Reservation));
        _btnWalkInMode.Click += (_, _) => Guard(() => SetMode(CheckInMode.WalkIn));
        _txtSearch.TextChanged += (_, _) =>
        {
            if (!_loading) Guard(() => LoadBookings(_booking?.ReservationId));
        };
        _cboStatus.SelectedIndexChanged += (_, _) =>
        {
            if (!_loading) Guard(() => LoadBookings(null));
        };
        _grid.SelectionChanged += (_, _) =>
        {
            if (!_loading) Guard(() => SelectBooking(_grid.CurrentRow?.Tag as CheckInReservationDto));
        };
        _numGuests.ValueChanged += (_, _) =>
        {
            if (!_loading) Guard(UpdateState);
        };
        _dtCheckOut.ValueChanged += (_, _) =>
        {
            if (!_loading)
                Guard(() =>
                {
                    LoadRooms(SelectedRoom?.RoomId);
                    UpdateState();
                });
        };
        _cboRoom.SelectedIndexChanged += (_, _) =>
        {
            if (!_loading) Guard(UpdateState);
        };
        _cboRoomType.SelectedIndexChanged += (_, _) =>
        {
            if (!_loading)
                Guard(() =>
                {
                    LoadRooms(SelectedRoom?.RoomId);
                    UpdateState();
                });
        };
        _btnFindCustomer.Click += (_, _) => FindCustomer();
        _btnAddCustomer.Click += (_, _) => AddCustomer();
        _btnCancelBooking.Click += (_, _) => CancelBooking();
        _btnConfirm.Click += (_, _) => Confirm();
        _btnReset.Click += (_, _) => AskReset();
        _btnPrint.Click += (_, _) => Guard(PrintReceipt);
    }

    private void SetMode(CheckInMode mode)
    {
        _mode = mode;
        ResetForm();
    }

    private void ResetForm()
    {
        _receipt = null;
        _booking = null;
        _customer = null;
        Silently(() =>
        {
            _txtSearch.Clear();
            _cboStatus.SelectedIndex = 0;
            _txtNote.Clear();
            _numGuests.Value = 1;
            _dtCheckOut.Value = _service.Now.Date.AddDays(1).AddHours(12);
            _cboRoomType.Items.Clear();
        });
        _bookingCard.Visible = _mode == CheckInMode.Reservation;
        StyleModes();
        if (_mode == CheckInMode.Reservation) LoadBookings(null);
        else ShowSelection();
    }

    private void RefreshData()
    {
        if (_receipt is not null)
        {
            ResetForm();
            return;
        }

        StyleModes();
        if (_mode == CheckInMode.Reservation)
        {
            LoadBookings(_booking?.ReservationId);
            return;
        }

        if (_customer is not null) _customer = _service.GetCustomer(_customer.CustomerId);
        var roomId = SelectedRoom?.RoomId;
        ShowCustomer();
        LoadRooms(roomId);
        UpdateState();
    }

    private void LoadBookings(int? keepId)
    {
        var status = ((StatusOption)_cboStatus.SelectedItem!).Value;
        var rows = _service.GetReservations(status, _txtSearch.Text);
        var now = _service.Now;
        CheckInReservationDto? selected = null;

        Silently(() =>
        {
            _grid.Rows.Clear();
            foreach (var r in rows)
            {
                var overdue = r.Status == ReservationStatus.Confirmed && r.ExpectedCheckOut <= now;
                var index = _grid.Rows.Add(r.ReservationCode, r.CustomerName, r.RoomCode ?? "Chưa gán",
                    r.ExpectedCheckIn.ToString("dd/MM HH:mm"), r.ExpectedCheckOut.ToString("dd/MM HH:mm"),
                    CheckInService.StatusText(r.Status) + (overdue ? " (quá hạn)" : ""));
                var row = _grid.Rows[index];
                row.Tag = r;
                row.Cells[5].Style.ForeColor = overdue ? Red : StatusColor(r.Status);
            }

            var target = _grid.Rows.Cast<DataGridViewRow>()
                             .FirstOrDefault(x => ((CheckInReservationDto)x.Tag!).ReservationId == keepId)
                         ?? (_grid.Rows.Count > 0 ? _grid.Rows[0] : null);
            _grid.ClearSelection();
            if (target is not null)
            {
                _grid.CurrentCell = target.Cells[0];
                target.Selected = true;
                selected = (CheckInReservationDto)target.Tag!;
            }
        });
        if (selected is not null && selected.ReservationId == _booking?.ReservationId && _receipt is null)
        {
            var roomId = SelectedRoom?.RoomId;
            _booking = selected;
            _customer = _service.GetCustomer(selected.CustomerId);
            ShowCustomer();
            LoadRooms(roomId);
            UpdateState();
        }
        else SelectBooking(selected);
    }

    private void SelectBooking(CheckInReservationDto? booking)
    {
        if (_mode != CheckInMode.Reservation) return;
        _receipt = null;
        _booking = booking;
        _customer = booking is null ? null : _service.GetCustomer(booking.CustomerId);
        Silently(() =>
        {
            _numGuests.Value = 1;
            _txtNote.Text = booking?.Note?.ReplaceLineEndings(Environment.NewLine) ?? "";
            _dtCheckOut.Value = booking?.ExpectedCheckOut ?? _service.Now.Date.AddDays(1).AddHours(12);
        });
        ShowSelection();
    }

    private void ShowSelection()
    {
        ShowCustomer();
        LoadRooms(_booking?.RoomId);
        UpdateState();
    }

    private void ShowCustomer()
    {
        _txtName.Text = _customer?.FullName ?? "";
        _txtIdentity.Text = _customer is null ? "" : $"{_customer.IdentityType}  {_customer.IdentityNumber}";
        _txtPhone.Text = _customer?.Phone ?? "";
        _lblCustomerMore.Text = _customer is null
            ? (_mode == CheckInMode.WalkIn ? "Hãy tra cứu khách trước, chỉ thêm mới khi chưa có hồ sơ." : "")
            : $"Hồ sơ #{_customer.CustomerId}   •   Email: {_customer.Email ?? "—"}   •   Địa chỉ: {_customer.Address ?? "—"}";
    }

    private void LoadRooms(int? selectId)
    {
        var rooms = new List<CheckInRoomDto>();
        if (_mode == CheckInMode.WalkIn || _booking is { Status: ReservationStatus.Confirmed })
            rooms.AddRange(_service.GetAssignableRooms(_booking?.ReservationId, _dtCheckOut.Value));

        if (_mode == CheckInMode.WalkIn) rooms = FilterByType(rooms, ref selectId);
        else Silently(() => { _cboRoomType.Items.Clear(); });
        if (selectId is int id && rooms.All(r => r.RoomId != id) && _service.GetRoom(id) is { } held)
            rooms.Insert(0, held);

        Silently(() =>
        {
            _cboRoom.Items.Clear();
            foreach (var room in rooms) _cboRoom.Items.Add(new RoomOption(room));
            _cboRoom.SelectedItem = _cboRoom.Items.Cast<RoomOption>().FirstOrDefault(o => o.Room.RoomId == selectId);
        });
    }

    private List<CheckInRoomDto> FilterByType(List<CheckInRoomDto> rooms, ref int? selectId)
    {
        var current = (_cboRoomType.SelectedItem as TypeOption)?.Id;
        var types = rooms.GroupBy(r => r.RoomTypeId)
            .Select(g => new TypeOption(g.Key, g.First().TypeName))
            .OrderBy(t => t.Name).ToList();
        Silently(() =>
        {
            _cboRoomType.Items.Clear();
            _cboRoomType.Items.Add(new TypeOption(null, "Tất cả loại phòng"));
            foreach (var type in types) _cboRoomType.Items.Add(type);
            _cboRoomType.SelectedItem = _cboRoomType.Items.Cast<TypeOption>().FirstOrDefault(t => t.Id == current)
                                        ?? _cboRoomType.Items[0];
        });
        var typeId = ((TypeOption)_cboRoomType.SelectedItem!).Id;
        if (typeId is null) return rooms;
        var filtered = rooms.Where(r => r.RoomTypeId == typeId).ToList();
        var wanted = selectId;
        if (wanted is not null && filtered.All(r => r.RoomId != wanted)) selectId = null;
        return filtered;
    }

    private CheckInRoomDto? SelectedRoom => (_cboRoom.SelectedItem as RoomOption)?.Room;

    private CheckInDraftDto BuildDraft() => new(
        _mode, _booking?.ReservationId, _customer?.CustomerId, SelectedRoom?.RoomId,
        (int)_numGuests.Value, TrimToMinute(_dtCheckOut.Value), _txtNote.Text);

    private void UpdateState()
    {
        if (_receipt is not null) return;

        var isBookingMode = _mode == CheckInMode.Reservation;
        var pending = _booking is { Status: ReservationStatus.Confirmed, StayId: null };
        var editable = !isBookingMode || pending;

        _btnFindCustomer.Enabled = _btnAddCustomer.Enabled = !isBookingMode;
        _cboRoomType.Enabled = !isBookingMode;
        _numGuests.Enabled = _dtCheckOut.Enabled = _cboRoom.Enabled = editable;
        _txtNote.ReadOnly = !editable;
        _btnCancelBooking.Visible = isBookingMode && pending;
        _btnPrint.Enabled = false;
        _txtCheckInTime.Text = editable ? _service.Now.ToString("dd/MM/yyyy HH:mm") : "—";

        ShowBookingState();

        var room = SelectedRoom;
        _lblHeldRoom.Text = _booking is null
            ? "Chỉ liệt kê phòng giao được trong khoảng thời gian đã chọn."
            : _booking.RoomCode is null
                ? $"Đặt phòng chỉ giữ theo loại {_booking.TypeName}. Hãy chọn một phòng."
                : $"Phòng đang giữ: P.{_booking.RoomCode}. Chỉ đổi được sang phòng cùng loại {_booking.TypeName}.";
        _lblRates.Text = room is null
            ? ""
            : $"Giá chụp khi check-in:  {Money(room.DailyRate)}/ngày  •  {Money(room.HourlyRate)}/giờ  •  Tối đa {room.MaxOccupancy} khách";

        _lblSummary.Text = string.Join(Environment.NewLine,
            $"Phòng nhận:  {(room is null ? "—" : $"P.{room.RoomCode} ({room.FloorName})")}",
            $"Hạng phòng:  {room?.TypeName ?? _booking?.TypeName ?? "—"}",
            $"Khách đại diện:  {_customer?.FullName ?? "—"}",
            $"Số khách:  {(editable ? _numGuests.Value.ToString() : "—")}",
            $"Tiền cọc:  {Money(_booking?.DepositAmount ?? 0)}");

        _evaluation = editable ? _service.Evaluate(BuildDraft()) : null;
        ShowChecklist();
        ShowFieldErrors();

        _btnConfirm.Enabled = _evaluation?.CanConfirm == true;
        _btnConfirm.BackColor = _btnConfirm.Enabled ? Color.Black : Color.Silver;
        (_lblResult.Text, _lblResult.ForeColor) = _evaluation switch
        {
            null => ("Không thể check-in ở trạng thái này.", Color.DimGray),
            { CanConfirm: true } => ("Đủ điều kiện. Có thể xác nhận nhận phòng.", Green),
            _ => ("Chưa đủ điều kiện, xem bảng kiểm tra phía trên.", Red)
        };
    }

    private void ShowFieldErrors()
    {
        _errors.Clear();
        if (_evaluation is null) return;
        var fields = new Control[] { _txtName, _grid, _cboRoom, _numGuests, _dtCheckOut };
        for (var i = 0; i < fields.Length && i < _evaluation.Items.Count; i++)
        {
            var item = _evaluation.Items[i];
            if (item.Passed) continue;
            _errors.SetIconAlignment(fields[i], ErrorIconAlignment.MiddleRight);
            _errors.SetIconPadding(fields[i], i == 1 ? -20 : -36);
            _errors.SetError(fields[i], item.Detail);
        }
    }

    private void ShowBookingState()
    {
        if (_booking is null)
        {
            _lblBookingState.Text = "Chưa có đặt phòng được chọn";
            _lblBookingState.ForeColor = Color.DimGray;
            _lblBookingDetail.Text = "";
            return;
        }

        _lblBookingState.Text =
            $"● {_booking.ReservationCode} – {CheckInService.StatusText(_booking.Status).ToUpper(Vi)}";
        _lblBookingState.ForeColor = StatusColor(_booking.Status);
        var hint = _booking.Status switch
        {
            ReservationStatus.Confirmed => "Có thể nhận phòng (nếu đạt đủ điều kiện) hoặc hủy.",
            ReservationStatus.CheckedIn => $"Khách đã nhận phòng (lượt lưu trú #{_booking.StayId}). Theo dõi tại S12.",
            ReservationStatus.Completed => "Đã trả phòng và hoàn tất. Chỉ xem.",
            ReservationStatus.Cancelled => "Đặt phòng đã hủy. Chỉ xem.",
            _ => "Khách không đến. Chỉ xem."
        };
        _lblBookingDetail.Text =
            $"Loại: {_booking.TypeName}   •   Phòng giữ: {_booking.RoomCode ?? "Chưa gán"}   •   Cọc: {Money(_booking.DepositAmount)}\n" +
            $"Lịch: {_booking.ExpectedCheckIn:dd/MM/yyyy HH:mm} → {_booking.ExpectedCheckOut:dd/MM/yyyy HH:mm}   •   Tạo: {_booking.CreatedAt:dd/MM/yyyy}\n{hint}";
    }

    private void ShowChecklist()
    {
        foreach (var old in _checklist.Controls.Cast<Control>().ToList()) old.Dispose();
        _lblChecklistTitle.Text =
            _evaluation is null ? "—" : $"{_evaluation.PassedCount}/{_evaluation.Items.Count} đạt";
        if (_evaluation is null) return;

        var number = 1;
        foreach (var item in _evaluation.Items)
        {
            _checklist.Controls.Add(new Label
            {
                AutoSize = true, MaximumSize = new Size(325, 0), Margin = new Padding(0, 4, 0, 4),
                ForeColor = item.Passed ? Green : Red,
                Text = $"{(item.Passed ? "✔" : "✖")}  {number++}. {item.Title.Replace("&", "&&")}\n      {item.Detail}"
            });
        }
    }

    private void StyleModes()
    {
        _btnReservationMode.Text =
            $"CÓ ĐẶT TRƯỚC (RESERVATION CHECK-IN)\n{_service.CountDueToday()} đơn đến hạn hôm nay";
        _btnWalkInMode.Text = "KHÁCH VÃNG LAI (WALK-IN CHECK-IN)\nNhận phòng trực tiếp, không cần đặt trước";
        foreach (var (button, active) in new[]
                 {
                     (_btnReservationMode, _mode == CheckInMode.Reservation),
                     (_btnWalkInMode, _mode == CheckInMode.WalkIn)
                 })
        {
            button.BackColor = active ? Navy : Pale;
            button.ForeColor = active ? Color.White : Navy;
        }
    }

    private void FindCustomer()
    {
        if (_mode != CheckInMode.WalkIn || _receipt is not null) return;
        if (Dialogs.PickCustomer(this, _service) is not { } customer) return;
        Guard(() =>
        {
            _customer = customer;
            ShowCustomer();
            UpdateState();
        });
    }

    private void AddCustomer()
    {
        if (_mode != CheckInMode.WalkIn || _receipt is not null) return;
        if (Dialogs.AddCustomer(this, _service) is not { } customer) return;
        Guard(() =>
        {
            _customer = customer;
            ShowCustomer();
            UpdateState();
        });
    }

    private void Confirm()
    {
        if (_receipt is not null) return;
        Guard(() =>
        {
            UpdateState();
            if (_evaluation is not { CanConfirm: true }) return;

            var draft = BuildDraft();
            var room = SelectedRoom!;
            var customerName = _customer!.FullName;
            var answer = MessageBox.Show(this,
                $"Xác nhận nhận phòng P.{room.RoomCode} cho khách {customerName}?\n" +
                $"Số khách: {draft.GuestCount}\nTrả phòng dự kiến: {draft.ExpectedCheckOut:dd/MM/yyyy HH:mm}",
                "Xác nhận check-in", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return;

            var deposit = _booking?.DepositAmount ?? 0;
            var code = _booking?.ReservationCode ?? "Khách vãng lai";
            var result = draft.Mode == CheckInMode.Reservation
                ? _service.CheckInReservation(draft)
                : _service.CheckInWalkIn(draft);

            var receipt = string.Join(Environment.NewLine,
                "PHIẾU NHẬN PHÒNG", "",
                $"Lượt lưu trú: #{result.StayId}", $"Đặt phòng: {code}",
                $"Khách: {customerName}", $"Phòng: {result.RoomCode}", $"Số khách: {draft.GuestCount}",
                $"Nhận phòng: {result.ActualCheckIn:dd/MM/yyyy HH:mm}",
                $"Trả dự kiến: {draft.ExpectedCheckOut:dd/MM/yyyy HH:mm}",
                $"Tiền cọc: {Money(deposit)}");

            ShowCompleted(receipt, result);
            MessageBox.Show(this, $"Đã nhận phòng {result.RoomCode}. Lượt lưu trú #{result.StayId}.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }, reloadOnBusinessError: true);
    }
}
