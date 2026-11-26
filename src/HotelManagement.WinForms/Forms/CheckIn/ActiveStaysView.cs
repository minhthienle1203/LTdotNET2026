using System.Globalization;
using System.Text;
using HotelManagement.Core.DTOs;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.CheckIn;

public sealed class ActiveStaysView : UserControl
{
    private static readonly Color Navy = Color.FromArgb(17, 28, 49);
    private static readonly Color Blue = Color.FromArgb(0, 74, 198);
    private static readonly Color Pale = Color.FromArgb(236, 242, 252);
    private static readonly Color Page = Color.FromArgb(246, 248, 252);
    private static readonly Color Green = Color.FromArgb(22, 128, 61);
    private static readonly Color Red = Color.FromArgb(185, 28, 28);
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    private readonly ActiveStayService _service;
    private ActiveStaySnapshotDto? _data;
    private IReadOnlyList<ActiveStayDto> _visible = [];
    private int? _selectedId;
    private bool _loading;

    private readonly Label _kpiRooms = KpiValue();
    private readonly Label _kpiGuests = KpiValue();
    private readonly Label _kpiServices = KpiValue();
    private readonly Label _kpiDue = KpiValue();
    private readonly Label _kpiPending = KpiValue();
    private readonly Label _subRooms = KpiNote();
    private readonly Label _subGuests = KpiNote();
    private readonly Label _subServices = KpiNote();
    private readonly Label _subDue = KpiNote();
    private readonly Label _subPending = KpiNote();

    private readonly TextBox _txtSearch = new() { Width = 220 };
    private readonly ComboBox _cboFloor = Combo(150);
    private readonly ComboBox _cboType = Combo(150);
    private readonly ComboBox _cboDue = Combo(170);

    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Label _lblStatus = new() { Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleLeft };

    private readonly Label _lblRoom = new() { AutoSize = true, Font = Bold(15), ForeColor = Red };
    private readonly Label _lblStay = Info();
    private readonly ListView _lstServices = new()
    {
        View = View.Details, FullRowSelect = true, Width = 350, Height = 130,
        HeaderStyle = ColumnHeaderStyle.Nonclickable
    };
    private readonly RadioButton _rdoDay = new() { Text = "Theo ngày", Checked = true, AutoSize = true };
    private readonly RadioButton _rdoHour = new() { Text = "Theo giờ", AutoSize = true };
    private readonly Label _lblMoney = Info();
    private readonly Label _lblDue = new() { AutoSize = true, Font = Bold(11), ForeColor = Blue, MaximumSize = new Size(350, 0) };
    private readonly Button _btnService = BigButton("Ghi dịch vụ phát sinh (S15)  [F6]", Navy);
    private readonly Button _btnCheckout = BigButton("Trả phòng && thanh toán (S16)  [F8]", Blue);

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30_000 };
    public event EventHandler? CheckInRequested;
    public event Action<int?>? RoomMapRequested;
    public event Action<int>? AddServiceRequested;
    public event Action<int>? CheckoutRequested;

    private sealed record Option<T>(string Name, T Value)
    {
        public override string ToString() => Name;
    }

    public ActiveStaysView(ActiveStayService service)
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
        if (DesignMode) return;
        RefreshData();
        _timer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.F1: RoomMapRequested?.Invoke(Selected?.RoomId); return true;
            case Keys.F5: RefreshData(); return true;
            case Keys.F6: OpenForSelected(AddServiceRequested, "S15"); return true;
            case Keys.F8: OpenForSelected(CheckoutRequested, "S16"); return true;
            case Keys.F11: CheckInRequested?.Invoke(this, EventArgs.Empty); return true;
            default: return base.ProcessCmdKey(ref msg, keyData);
        }
    }
    public void RefreshData() => Reload(showError: true);
    public void SelectStay(int stayId)
    {
        _selectedId = stayId;
        Silently(() =>
        {
            _txtSearch.Clear();
            if (_cboFloor.Items.Count > 0) _cboFloor.SelectedIndex = 0;
            if (_cboType.Items.Count > 0) _cboType.SelectedIndex = 0;
            _cboDue.SelectedIndex = 0;
        });
        Reload(showError: true);
    }
    private void BuildLayout()
    {
        var btnRefresh = SmallButton("Làm mới [F5]", Color.White, Navy);
        var btnCheckIn = SmallButton("+ Nhận phòng mới [F11]", Blue, Color.White);
        var btnMap = SmallButton("Sơ đồ phòng (S01) [F1]", Color.White, Navy);
        var btnExport = SmallButton("Xuất CSV (mở bằng Excel)", Color.White, Navy);
        btnRefresh.Click += (_, _) => RefreshData();
        btnCheckIn.Click += (_, _) => CheckInRequested?.Invoke(this, EventArgs.Empty);
        btnMap.Click += (_, _) => RoomMapRequested?.Invoke(Selected?.RoomId);
        btnExport.Click += (_, _) => Export();

        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = Color.White, Padding = new Padding(12, 8, 12, 6)
        };
        header.Controls.Add(new Label { Text = "Chỉ hiển thị lượt lưu trú InProgress. F6: ghi dịch vụ, F8: trả phòng.", AutoSize = true });
        header.Controls.Add(Flow(btnRefresh, btnCheckIn, btnMap, btnExport));
        var kpis = new TableLayoutPanel { Dock = DockStyle.Top, Height = 98, ColumnCount = 5, RowCount = 1, Padding = new Padding(6, 4, 6, 0) };
        var cards = new[]
        {
            Kpi("PHÒNG ĐANG Ở", _kpiRooms, _subRooms), Kpi("KHÁCH LƯU TRÚ", _kpiGuests, _subGuests),
            Kpi("DỊCH VỤ TẠM TÍNH", _kpiServices, _subServices), Kpi("DỰ KIẾN TRẢ HÔM NAY", _kpiDue, _subDue),
            Kpi("CHỜ IN HÓA ĐƠN", _kpiPending, _subPending)
        };
        for (var i = 0; i < cards.Length; i++)
        {
            kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            kpis.Controls.Add(cards[i], i, 0);
        }
        _cboDue.Items.AddRange(new object[]
        {
            new Option<DueFilter>("Tất cả ngày trả", DueFilter.All),
            new Option<DueFilter>("Trả hôm nay", DueFilter.Today),
            new Option<DueFilter>("Đã quá hạn", DueFilter.Overdue),
            new Option<DueFilter>("Từ ngày mai", DueFilter.Later)
        });
        _cboDue.SelectedIndex = 0;
        var btnClear = SmallButton("Xóa lọc", Color.White, Navy);
        btnClear.Click += (_, _) =>
        {
            Silently(() =>
            {
                _txtSearch.Clear();
                if (_cboFloor.Items.Count > 0) _cboFloor.SelectedIndex = 0;
                if (_cboType.Items.Count > 0) _cboType.SelectedIndex = 0;
                _cboDue.SelectedIndex = 0;
            });
            ApplyFilter();
        };
        var filters = Flow(Caption("Tìm nhanh"), _txtSearch, Caption("Tầng"), _cboFloor,
            Caption("Hạng phòng"), _cboType, Caption("Dự kiến trả"), _cboDue, btnClear);
        filters.BackColor = Color.White;
        filters.Padding = new Padding(8, 6, 8, 6);

        BuildGrid();
        var gridPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Color.White };
        gridPanel.Controls.Add(_grid);
        gridPanel.Controls.Add(_lblStatus);
        _lstServices.Columns.Add("Dịch vụ", 160);
        _lstServices.Columns.Add("SL", 70);
        _lstServices.Columns.Add("Thành tiền", 100, HorizontalAlignment.Right);

        var detail = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false,
            AutoScroll = true, BackColor = Color.White, Padding = new Padding(10)
        };
        detail.Controls.AddRange(new Control[]
        {
            new Label { Text = "CHI TIẾT LƯU TRÚ ĐANG CHỌN", AutoSize = true, Font = Bold(10) },
            _lblRoom, _lblStay,
            new Label { Text = "Dịch vụ đã dùng trong đợt ở", AutoSize = true, Font = Bold(9), ForeColor = Blue },
            _lstServices,
            new Label { Text = "Tạm tính đến hiện tại", AutoSize = true, Font = Bold(9) },
            Flow(_rdoDay, _rdoHour), _lblMoney, _lblDue, _btnService, _btnCheckout
        });
        foreach (var panel in detail.Controls.OfType<FlowLayoutPanel>())
        {
            panel.Dock = DockStyle.None;
            panel.MinimumSize = new Size(350, 0);
        }

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(6) };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(gridPanel, 0, 0);
        body.Controls.Add(detail, 1, 0);
        Controls.Add(body);
        Controls.Add(filters);
        Controls.Add(kpis);
        Controls.Add(header);
    }

    private void BuildGrid()
    {
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.RowTemplate.Height = 42;
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grid.DefaultCellStyle.SelectionBackColor = Pale;
        _grid.DefaultCellStyle.SelectionForeColor = Navy;

        foreach (var (title, weight) in new[]
                 {
                     ("Phòng", 55f), ("Hạng", 80f), ("Khách đại diện", 175f), ("Giờ check-in", 85f),
                     ("Đã ở", 70f), ("Dự kiến trả", 100f), ("Khách", 45f), ("Dịch vụ", 80f)
                 })
        {
            var index = _grid.Columns.Add(title, title);
            _grid.Columns[index].FillWeight = weight;
            _grid.Columns[index].SortMode = DataGridViewColumnSortMode.NotSortable;
        }
    }

    private void WireEvents()
    {
        _txtSearch.TextChanged += (_, _) => { if (!_loading) ApplyFilter(); };
        foreach (var combo in new[] { _cboFloor, _cboType, _cboDue })
            combo.SelectedIndexChanged += (_, _) => { if (!_loading) ApplyFilter(); };
        _grid.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            _selectedId = (_grid.CurrentRow?.Tag as ActiveStayDto)?.StayId;
            ShowDetail();
        };
        _rdoDay.CheckedChanged += (_, _) => ShowDetail();
        _btnService.Click += (_, _) => OpenForSelected(AddServiceRequested, "S15");
        _btnCheckout.Click += (_, _) => OpenForSelected(CheckoutRequested, "S16");
        _timer.Tick += (_, _) => { if (Visible) Reload(showError: false); };
    }
    private void Reload(bool showError)
    {
        try
        {
            _data = _service.Load();
            FillLookup(_cboFloor, "Tất cả tầng", _data.Floors);
            FillLookup(_cboType, "Tất cả hạng phòng", _data.RoomTypes);
            ShowSummary();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _lblStatus.ForeColor = Red;
            _lblStatus.Text = "Không nạp được dữ liệu, đang hiển thị dữ liệu cũ. Bấm F5 để thử lại.";
            if (showError)
                MessageBox.Show(this, "Lỗi đọc CSDL:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FillLookup(ComboBox combo, string allText, IReadOnlyList<LookupItemDto> items)
    {
        if (combo.DroppedDown) return;
        var current = (combo.SelectedItem as Option<int?>)?.Value;
        Silently(() =>
        {
            combo.Items.Clear();
            combo.Items.Add(new Option<int?>(allText, null));
            foreach (var item in items) combo.Items.Add(new Option<int?>(item.Name, item.Id));
            combo.SelectedItem = combo.Items.Cast<Option<int?>>().FirstOrDefault(o => o.Value == current)
                                 ?? combo.Items[0];
        });
    }

    private void ShowSummary()
    {
        var s = ActiveStayService.Summarize(_data!, _data!.LoadedAt);
        _kpiRooms.Text = $"{s.OccupiedRooms} / {s.ActiveRoomCount}";
        _subRooms.Text = $"Công suất {s.OccupancyPercent.ToString("0.#", Vi)}%";
        _kpiGuests.Text = s.Guests.ToString();
        _subGuests.Text = "người đang ở";
        _kpiServices.Text = Money(s.ServiceTotal);
        _subServices.Text = $"{s.ServiceLines} dòng dịch vụ";
        _kpiDue.Text = $"{s.DueToday} lượt";
        _subDue.Text = $"{s.Overdue} lượt đã quá hạn";
        _subDue.ForeColor = s.Overdue > 0 ? Red : Color.DimGray;
        _kpiPending.Text = $"{s.CheckoutPending} lượt";
        _subPending.Text = "CheckoutPending";
    }

    private void ApplyFilter()
    {
        if (_data is null) return;
        var now = _data.LoadedAt;
        var filter = new ActiveStayFilter(
            _txtSearch.Text,
            (_cboFloor.SelectedItem as Option<int?>)?.Value,
            (_cboType.SelectedItem as Option<int?>)?.Value,
            ((Option<DueFilter>)_cboDue.SelectedItem!).Value);
        _visible = ActiveStayService.Filter(_data, filter, now);

        var scroll = _grid.Rows.Count > 0 ? _grid.FirstDisplayedScrollingRowIndex : -1;
        Silently(() =>
        {
            _grid.Rows.Clear();
            foreach (var stay in _visible)
            {
                var overdue = ActiveStayService.IsOverdue(stay, now);
                var index = _grid.Rows.Add(
                    "P." + stay.RoomCode, stay.TypeName,
                    $"{stay.CustomerName}\nKH-{stay.CustomerId:0000} • {stay.Phone}",
                    stay.ActualCheckIn.ToString("dd/MM HH:mm"),
                    ActiveStayService.DurationText(stay.ActualCheckIn, now),
                    (overdue ? "⚠ Quá hạn\n" : "") + stay.ExpectedCheckOut.ToString("dd/MM HH:mm"),
                    stay.GuestCount.ToString(),
                    Money(stay.ServiceTotal));
                var row = _grid.Rows[index];
                row.Tag = stay;
                row.Cells[0].Style.ForeColor = row.Cells[0].Style.SelectionForeColor = Red;
                if (overdue) row.Cells[5].Style.ForeColor = row.Cells[5].Style.SelectionForeColor = Red;
            }

            var target = _grid.Rows.Cast<DataGridViewRow>()
                             .FirstOrDefault(r => ((ActiveStayDto)r.Tag!).StayId == _selectedId)
                         ?? (_grid.Rows.Count > 0 ? _grid.Rows[0] : null);
            _grid.ClearSelection();
            if (target is not null)
            {
                _grid.CurrentCell = target.Cells[0];
                target.Selected = true;
                if (scroll >= 0 && scroll < _grid.Rows.Count) _grid.FirstDisplayedScrollingRowIndex = scroll;
            }
            _selectedId = (target?.Tag as ActiveStayDto)?.StayId;
        });

        _lblStatus.ForeColor = Color.DimGray;
        _lblStatus.Text = _data.Stays.Count == 0
            ? $"Không có khách đang lưu trú.   •   Cập nhật {now:HH:mm:ss}"
            : $"Hiển thị {_visible.Count}/{_data.Stays.Count} lượt   •   Cập nhật {now:HH:mm:ss}, tự làm mới mỗi 30 giây";
        ShowDetail();
    }


}
