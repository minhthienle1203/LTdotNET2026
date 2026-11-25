using System.Globalization;

namespace HotelManagement.WinForms.Forms.Reports;

public class MonthlyReportView : UserControl
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
    private const int CalendarCellSize = 32;
    private readonly DateTimePicker _monthFilter = new();
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

    public MonthlyReportView(Action<string, object?> navigate)
    {
        _navigate = navigate;
        Dock = DockStyle.Fill;
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
        host.Controls.Add(panel);
        return host;

    }

    private void LoadReport()
    {
        _totalRevenue.Text = "12.500.000 đ";
        _totalRevenueDetails.Text = "Doanh thu trong tháng";

        _roomRevenue.Text = "10.000.000 đ";
        _roomRevenueDetails.Text = "Doanh thu phòng";

        _serviceRevenue.Text = "2.500.000 đ";
        _serviceRevenueDetails.Text = "Doanh thu dịch vụ";

        _occupancyRate.Text = "65,50%";
        _occupancyDetails.Text = "Công suất phòng";
    }

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
