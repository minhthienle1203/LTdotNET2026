using System.Drawing;
using System.Windows.Forms;

namespace HotelManagement.WinForms.Forms.Main;

public sealed class MainForm : Form
{
    private static readonly Color HeaderColor = Color.FromArgb(9, 39, 61);
    private static readonly Color SidebarColor = Color.FromArgb(20, 31, 51);
    private static readonly Color SelectedColor = Color.FromArgb(47, 101, 232);
    private static readonly Color PageBackground = Color.FromArgb(245, 247, 250);

    private static readonly Dictionary<string, string> ScreenTitles = new()
    {
        ["S01"] = "Tổng quan và sơ đồ phòng",
        ["S02"] = "Quản lý tầng",
        ["S03"] = "Danh sách loại phòng",
        ["S04"] = "Thêm / cập nhật loại phòng",
        ["S05"] = "Danh sách phòng",
        ["S06"] = "Thêm / cập nhật phòng",
        ["S07"] = "Danh sách khách hàng",
        ["S08"] = "Thêm / cập nhật khách hàng",
        ["S09"] = "Danh sách đặt phòng",
        ["S10"] = "Tạo / cập nhật đặt phòng",
        ["S11"] = "Nhận phòng (Check-in)",
        ["S12"] = "Lưu trú đang diễn ra",
        ["S13"] = "Danh mục dịch vụ",
        ["S14"] = "Thêm / cập nhật dịch vụ",
        ["S15"] = "Ghi nhận dịch vụ phát sinh",
        ["S16"] = "Trả phòng và thanh toán",
        ["S17"] = "Danh sách hóa đơn",
        ["S18"] = "Chi tiết hóa đơn và in mô phỏng",
        ["S19"] = "Dọn phòng",
        ["S20"] = "Báo cáo tháng"

    };

    private static readonly Dictionary<string, string> MenuParents = new()
    {
        ["S04"] = "S03", ["S06"] = "S05", ["S08"] = "S07",
        ["S10"] = "S09", ["S14"] = "S13", ["S15"] = "S12",
        ["S16"] = "S12", ["S18"] = "S17"
    };

    private readonly Panel _pageHost;
    private readonly Label _pageTitle;
    private readonly Label _footerText;
    private readonly Dictionary<string, Button> _menuButtons = new();
    private readonly Dictionary<string, Func<object?, UserControl>> _pages = new();
    private UserControl? _activePage;
    private int _pageHeight;

    public string CurrentScreenCode { get; private set; } = "";
    public string CurrentScreenTitle { get; private set; } = "";

    public MainForm()
    {
        Text = "HotelManagement";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 520);
        WindowState = FormWindowState.Maximized;
        Font = new Font("Segoe UI", 8.5F);
        BackColor = PageBackground;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Margin = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        layout.Controls.Add(BuildHeader(), 0, 0);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = Padding.Empty };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 228));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        main.Controls.Add(BuildSidebar(), 0, 0);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(12, 6, 12, 6), Margin = Padding.Empty };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _pageTitle = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(37, 48, 63), Margin = Padding.Empty };
        _pageHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Margin = Padding.Empty };
        _pageHost.Resize += (_, _) => ResizePage();
        content.Controls.Add(_pageTitle, 0, 0);
        content.Controls.Add(_pageHost, 0, 1);
        main.Controls.Add(content, 1, 0);
        layout.Controls.Add(main, 0, 1);

        _footerText = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(9, 0, 0, 0), ForeColor = Color.FromArgb(95, 105, 120), BackColor = Color.FromArgb(232, 238, 246), Margin = Padding.Empty };
        layout.Controls.Add(_footerText, 0, 2);
        Controls.Add(layout);

        NavigateToScreen("S01");
    }

    public void RegisterPage(string code, Func<object?, UserControl> createPage)
    {
        code = Normalize(code);
        if (!ScreenTitles.ContainsKey(code)) throw new ArgumentException("Mã màn hình không hợp lệ.", nameof(code));
        ArgumentNullException.ThrowIfNull(createPage);
        _pages[code] = createPage;
        if (code == CurrentScreenCode) NavigateToScreen(code);
    }

    public void NavigateToScreen(string code, object? context = null)
    {
        code = Normalize(code);
        if (!ScreenTitles.TryGetValue(code, out var title)) throw new ArgumentException("Mã màn hình không hợp lệ.", nameof(code));
        if ((code is "S15" or "S16" or "S18") && context is null)
        {
            MessageBox.Show(this, $"Cần có dữ liệu liên quan để mở {code}.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_activePage != null)
        {
            _pageHost.Controls.Remove(_activePage);
            _activePage.Dispose();
        }
        _pageHost.AutoScrollPosition = Point.Empty;
        _pageHost.AutoScrollMinSize = Size.Empty;
        _activePage = null;
        _pageHeight = 0;

        if (_pages.TryGetValue(code, out var createPage))
        {
            _activePage = createPage(context);
            _activePage.Dock = DockStyle.None;
            _activePage.AutoSize = false;
            _pageHeight = Math.Max(_activePage.Height, _activePage.MinimumSize.Height);
            _activePage.Location = Point.Empty;
            _activePage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _pageHost.Controls.Add(_activePage);
            ResizePage();
        }

        CurrentScreenCode = code;
        CurrentScreenTitle = title;
        _pageTitle.Text = $"{code} - {title}";
        _footerText.Text = $"Đang mở: {code} - {title}     |     CSDL: Chưa tích hợp";
        Text = $"{code} - {title} | HotelManagement";

        var selected = MenuParents.TryGetValue(code, out var parent) ? parent : code;
        foreach (var item in _menuButtons)
        {
            var active = item.Key == selected;
            var color = active ? SelectedColor : SidebarColor;
            item.Value.BackColor = color;
            item.Value.FlatAppearance.MouseOverBackColor = color;
            item.Value.FlatAppearance.MouseDownBackColor = color;
            item.Value.ForeColor = Color.White;
        }
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Fill, BackColor = HeaderColor, Padding = new Padding(10, 0, 10, 0), Margin = Padding.Empty };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "HOTEL MANAGEMENT  |  HỆ THỐNG QUẢN LÝ KHÁCH SẠN",
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Margin = Padding.Empty
        });
        return header;
    }

    private Control BuildSidebar()
    {
        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, BackColor = SidebarColor, Padding = new Padding(7, 5, 7, 5), Margin = Padding.Empty
        };
        menu.Controls.Add(new Label { Text = "PHÂN HỆ NGHIỆP VỤ", Width = 210, Height = 15, ForeColor = Color.Silver, Font = new Font("Segoe UI", 7, FontStyle.Bold), Margin = new Padding(1, 0, 1, 3) });
        AddGroup(menu, "1. TỔNG QUAN", ("S01", "S01 - Tổng quan và sơ đồ phòng"));
        AddGroup(menu, "2. QUẢN LÝ PHÒNG", ("S02", "S02 - Quản lý tầng"), ("S03", "S03 - Danh sách loại phòng"), ("S05", "S05 - Danh sách phòng"));
        AddGroup(menu, "3. KHÁCH HÀNG", ("S07", "S07 - Danh sách khách hàng"));
        AddGroup(menu, "4. ĐẶT PHÒNG", ("S09", "S09 - Danh sách đặt phòng"));
        AddGroup(menu, "5. LỄ TÂN", ("S11", "S11 - Nhận phòng (Check-in)"), ("S12", "S12 - Lưu trú đang diễn ra"));
        AddGroup(menu, "6. DỊCH VỤ", ("S13", "S13 - Danh mục dịch vụ"));
        AddGroup(menu, "7. HÓA ĐƠN", ("S17", "S17 - Danh sách hóa đơn"));
        AddGroup(menu, "8. DỌN PHÒNG", ("S19", "S19 - Dọn phòng"));
        AddGroup(menu, "9. BÁO CÁO", ("S20", "S20 - Báo cáo tháng"));
        return menu;
    }

    private void AddGroup(FlowLayoutPanel menu, string title, params (string Code, string Text)[] items)
    {
        menu.Controls.Add(new Label { Text = title, Width = 210, Height = 12, ForeColor = Color.Silver, Font = new Font("Segoe UI", 7, FontStyle.Bold), Margin = new Padding(1, 3, 1, 1) });
        foreach (var item in items)
        {
            var button = new Button
            {
                Text = item.Text, Width = 210, Height = 22, Tag = item.Code,
                FlatStyle = FlatStyle.Flat, BackColor = SidebarColor, ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 7.5F),
                Padding = new Padding(6, 0, 3, 0), Margin = new Padding(0, 0, 0, 1),
                Cursor = Cursors.Hand, UseVisualStyleBackColor = false, AutoEllipsis = true
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) => NavigateToScreen(item.Code);
            _menuButtons[item.Code] = button;
            menu.Controls.Add(button);
        }
    }

    private void ResizePage()
    {
        if (_activePage == null || _pageHost.IsDisposed) return;
        var height = Math.Max(_pageHeight, _pageHost.ClientSize.Height);
        _activePage.Size = new Size(_pageHost.ClientSize.Width, height);
        _pageHost.AutoScrollMinSize = new Size(0, height);
    }

    private static string Normalize(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Mã màn hình không được để trống.", nameof(code));
        return code.Trim().ToUpperInvariant();
    }
}
