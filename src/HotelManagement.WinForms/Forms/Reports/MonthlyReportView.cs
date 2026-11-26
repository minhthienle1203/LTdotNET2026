using System.Globalization;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Reports;

public class MonthlyReportView : UserControl
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
    private const int CalendarCellSize = 32;
    private readonly DateTimePicker _monthFilter = new();
    private readonly ReportService _reportService;
    private readonly Action<string, object?> _navigate;
    //cac loai doanh thu
    private readonly Label _totalRevenue = new();
    private readonly Label _totalRevenueDetails = new();
    private readonly Label _roomRevenue = new();
    private readonly Label _roomRevenueDetails = new();
    private readonly Label _serviceRevenue = new();
    private readonly Label _serviceRevenueDetails = new();
    private readonly Label _occupancyRate = new();
    private readonly Label _occupancyDetails = new();
    private readonly Label _occupiedHours = new();
    private readonly Label _activeRoomCount = new();
    private readonly Label _daysInMonth = new();
    private readonly Label _reportStatus = new();
    private readonly Panel _calendarPanel = new();

    public MonthlyReportView(ReportService reportService, Action<string, object?> navigate)
    {
        _reportService = reportService;
        _navigate = navigate;
        Dock = DockStyle.Fill;
        AutoSize = true;
        BuildLayout();
        LoadReport();
    }
    private void BuildLayout()
    {
        var root = CreateGrid(1, 5);
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildFilter(), 0, 1);
        root.Controls.Add(BuildStatistics(), 0, 2);
        root.Controls.Add(BuildOccupancyPanel(), 0,3);
        _reportStatus.Dock = DockStyle.Fill;
        _reportStatus.TextAlign = ContentAlignment.MiddleLeft;
        _reportStatus.AutoEllipsis = true;
        root.Controls.Add(_reportStatus, 0, 4);
        Controls.Add(root);
    }
    private Control BuildHeader()
    {
        var header = CreateGrid(1, 2);
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new Label
        {
            Text = "BÁO CÁO DOANH THU THEO THÁNG", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        var description = new Label
        {
            Text = "Theo dõi doanh thu và tình hình khai thác các phòng theo tháng", Dock = DockStyle.Fill
        };
        header.Controls.Add(title, 0,0);
        header.Controls.Add(description, 0,1);
        return header;
    }
    private Control BuildFilter()
    {
        var panel = new Panel{Dock = DockStyle.Fill, Margin = new Padding(2)};
        var row = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var monthLabel = new Label
        {
            Text = "Kỳ báo cáo:", AutoSize = true, Margin = new Padding(3, 7, 3, 3)
        };
        _monthFilter.Format = DateTimePickerFormat.Custom;
        _monthFilter.CustomFormat = "MM/yyyy";
        _monthFilter.ShowUpDown = true;
        _monthFilter.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _monthFilter.Width = 112;
        row.Controls.Add(monthLabel);
        row.Controls.Add(_monthFilter);
        row.Controls.Add(MakeButton("Xem báo cáo", LoadReport));
        panel.Controls.Add(row);
        return panel;
    }
    private Control BuildStatistics()
    {
        var cards = CreateGrid(4, 1);
        for(var i = 0; i < 4; i++)
        {cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));}

        cards.Controls.Add(BuildStatisticsCrad("TỔNG DOANH THU", _totalRevenue, _totalRevenueDetails), 0, 0);
        cards.Controls.Add(BuildStatisticsCrad("DOANH THU PHÒNG", _roomRevenue, _roomRevenueDetails),1,0);
        cards.Controls.Add(BuildStatisticsCrad("DOANH THU DỊCH VỤ", _serviceRevenue, _serviceRevenueDetails),2, 0);
        cards.Controls.Add(BuildStatisticsCrad("CÔNG SUẤT PHÒNG", _occupancyRate, _occupancyDetails), 3, 0);
        return cards;
    }

    private Control BuildStatisticsCrad(string title, Label value, Label details)
    {
        var card = new GroupBox {Text = title, Dock = DockStyle.Fill, Margin = new Padding(4)};
        var content = CreateGrid(1, 2);
        value.Dock = DockStyle.Fill;
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.Font = new Font(value.Font, FontStyle.Bold);
        value.Text = "--";
        details.Dock = DockStyle.Fill;
        details.TextAlign = ContentAlignment.MiddleLeft;
        details.Font = new Font(details.Font, FontStyle.Bold);
        details.Text = "Chưa có dữ liệu";
        content.Controls.Add(value,0,0);
        content.Controls.Add(details, 0,1 );
        card.Controls.Add(content);
        return card;

    }

    private Control BuildOccupancyPanel()
    {
        var host = new Panel { Dock = DockStyle.Fill };
        var panel = new GroupBox {Text = "CÔNG SUẤT KHAI THÁC", Size = new Size(390, 360)};
        var content = CreateGrid(1, 4);
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _occupiedHours.Dock = DockStyle.Fill;
        _occupiedHours.TextAlign = ContentAlignment.MiddleLeft;
        _occupiedHours.Font = new Font(_occupiedHours.Font, FontStyle.Bold);
        _occupiedHours.Text = "-- / -- giờ";
        content.Controls.Add(_occupiedHours, 0,0);
        var details = CreateGrid(2, 1);
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        _activeRoomCount.Dock = DockStyle.Fill;
        _activeRoomCount.Font = new Font(_daysInMonth.Font, FontStyle.Regular);
        _daysInMonth.Dock = DockStyle.Fill;
        details.Controls.Add(_activeRoomCount, 0, 0);
        details.Controls.Add(_daysInMonth,1,0);
        content.Controls.Add(details, 0, 1);

        var calendarTitle = new Label
        {
            Text = "Lịch các  ngày trong tháng", Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9, FontStyle.Regular), TextAlign = ContentAlignment.MiddleLeft
        };
        content.Controls.Add(calendarTitle, 0, 2);
        //panel lịch có hoạt động của khách
        _calendarPanel.Dock = DockStyle.Fill;
        content.Controls.Add(_calendarPanel, 0, 3);
        panel.Controls.Add(content);
        host.Controls.Add(panel);
        CenterControl(panel, host);
        host.Resize += (_,_) => CenterControl(panel, host);
        return host;
    }
    private void BuildCalendar(int year, int month, IEnumerable<int> occopiedDays)
    {
        _calendarPanel.Controls.Clear();
        var occupied = new HashSet<int>(occopiedDays ?? Array.Empty<int>());
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var firstOffset = ((int)new DateTime(year, month, 1).DayOfWeek + 6) % 7;
        var calendar = new TableLayoutPanel
            { ColumnCount = 7, RowCount = 7, Size = new Size(CalendarCellSize * 7, 22 + CalendarCellSize * 6) };

        for (var column = 0; column < 7; column++)
        {
            calendar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CalendarCellSize));
        }
        calendar.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        for (int row = 0; row < 6; row++)
        {
            calendar.RowStyles.Add(new RowStyle(SizeType.Absolute, CalendarCellSize));
        }

        string[] weekdays = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        for (var columm = 0; columm < 7; columm++)
        {
            var weekdayLabel = new Label{ Text = weekdays[columm], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, AutoSize = true};
            calendar.Controls.Add(weekdayLabel, columm, 0);
        }

        for (var slot = 0; slot < 42; slot++)
        {
            var day = slot - firstOffset + 1;
            var validDay = day >= 1 && day <= daysInMonth;
            var isOccupied = validDay && occupied.Contains(day);
            var dayLabel = new Label
            {
                Text = validDay ? day.ToString(VietnameseCulture) : string.Empty,
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = isOccupied ? SystemColors.Highlight : SystemColors.Window,
                ForeColor = isOccupied ? SystemColors.HighlightText : SystemColors.ControlText,
            };
            calendar.Controls.Add(dayLabel, slot % 7, slot/7 +1);
        }

        var calendarArea = new Panel { Dock = DockStyle.Fill };
        calendarArea.Controls.Add(calendar);
        CenterControl(calendar, calendarArea);
        calendarArea.Resize += (_,_) => CenterControl(calendar, calendarArea);
        var body = CreateGrid(1, 2);
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));
        body.Controls.Add(calendarArea, 0, 0);
        body.Controls.Add(new Label
        {
            Text = "Ô được tô sáng: ngày có khách lưu trú", Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 6, FontStyle.Italic), AutoSize = true
        });
        _calendarPanel.Controls.Add(body);
    }
    private void LoadReport()
    {
        try
        {
            var report = _reportService.GetMonthlyReport(_monthFilter.Value.Year, _monthFilter.Value.Month);
            var total = report.TotalRevenue;
            _totalRevenue.Text = FormatCurrencyVnd(total);
            _totalRevenueDetails.Text = $"Kỳ {report.Month:D2}/{report.Year}";
            //tiền phòng, tiền dịch vụ và tỉ lệ doanh thu từng loại
            _roomRevenue.Text = FormatCurrencyVnd(report.RoomRevenue);
            _serviceRevenue.Text = FormatCurrencyVnd(report.ServiceRevenue);
            _roomRevenueDetails.Text = total > 0m
                ? $"Chiếm {(report.RoomRevenue / total * 100m).ToString("N1", VietnameseCulture)}% tổng thu"
                : "Chưa có doanh thu";
            _serviceRevenueDetails.Text = total > 0m
                ? $"Chiếm {(report.ServiceRevenue / total * 100m).ToString("N1", VietnameseCulture)}% tổng thu"
                : "Chưa có doanh thu";
            //tỉ lệ lắp đầy
            _occupancyRate.Text = report.OccupancyRate.ToString("N2", VietnameseCulture) + "%";
            _occupancyDetails.Text =
                $"{report.ActiveRoomCount.ToString("N0", VietnameseCulture)} phòng đang kinh doanh";
            _occupiedHours.Text =
                $"{report.OccupiedRoomHours.ToString("N2", VietnameseCulture)} / {report.TotalRoomHours.ToString("N2", VietnameseCulture)} giờ có khách";
            _activeRoomCount.Text = $"{report.ActiveRoomCount.ToString("N0", VietnameseCulture)} phòng đang kinh doanh";
            BuildCalendar(report.Year, report.Month, report.OccupiedDays);
            _reportStatus.Text = $"Kỳ {report.Month:D2} / {report.Year} Doanh thu tính theo hóa đơn đã thanh toán";
        }
        catch (Exception ex)
        {
            _reportStatus.Text = "Không tải được dữ liệu";
            MessageBox.Show(this, ex.Message, "Không tải được dữ liệu báo cáor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string FormatCurrencyVnd(decimal amount) => amount.ToString("N0", VietnameseCulture);

    private static TableLayoutPanel CreateGrid(int columns, int rows) => new()
    {
        Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rows
    };

    private static void CenterControl(Control control, Control parent)
    {
        control.Location = new Point(Math.Max(0, (parent.ClientSize.Width - control.Width) / 2),
                        Math.Max(0, (parent.ClientSize.Height - control.Height) / 2));
    }
    private static Button MakeButton(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
        button.Click += (_,_) => action();
        return button;
    }

}
