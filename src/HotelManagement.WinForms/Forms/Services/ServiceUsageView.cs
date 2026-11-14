using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HotelManagement.WinForms.Forms.Services;

/// S15 - Ghi nhan dịch vụ.

public sealed class ServiceUsageView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly List<ServiceItem> services;
    // Các thành phần giao diện
    private readonly TextBox SearchBox = new();
    private readonly ComboBox ServiceBox = new();
    private readonly ComboBox StatusBox = new();
    private readonly DataGridView serviceGrid = new();
    private readonly TableLayoutPanel ThongKePanel = new();
    private readonly Label serviceCount = new();
    private readonly Label serviceDetails = new();
    private readonly FlowLayoutPanel serviceActions = new();
    private ServiceItem? selectedService;

    public ServiceUsageView(Action<string, object?> navigate)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        services = LoadServices();
        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        AutoScroll = true;
        MinimumSize = new Size(0, 650);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(BuildHeader(), 0, 0);
        layout.Controls.Add(BuildThongKe(), 0, 1);
        layout.Controls.Add(BuildFilters(), 0, 2);
        layout.Controls.Add(serviceCount, 0, 3);
        layout.Controls.Add(BuildArea(), 0, 4);
        Controls.Add(layout);
        Filter();
        RefreshData();
    }
    private Control BuildHeader()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label
        {
            Text = "DANH MỤC DỊCH VỤ HỆ THỐNG",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Margin = new Padding(0, 5, 16, 0)
        });
        return panel;
    }
    //  Các ô thoongs kê
    private Control BuildThongKe()
    {
        var panel = ThongKePanel;
        panel.Dock = DockStyle.Fill;
        panel.ColumnCount = 3;
        panel.RowCount = 1;
        panel.ColumnStyles.Clear();
        for (var i = 0; i < 3; i++)
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        string[] titles = { "Tổng dịch vụ", "Đang cung cấp", "Ngừng cung cấp" };
        panel.Controls.Clear();
        for (var i = 0; i < titles.Length; i++)
        {
            var item = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Text = titles[i] + "\n0",
                Margin = new Padding(2),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Color.White
            };
            panel.Controls.Add(item, i, 0);
        }
        return panel;
    }
    private Control BuildFilters()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Margin = new Padding(0, 8, 5, 0) });
        SearchBox.Width = 160;
        SearchBox.TextChanged += (_, _) => RefreshData();
        panel.Controls.Add(SearchBox);
        panel.Controls.Add(new Label { Text = "Nhóm:", AutoSize = true, Margin = new Padding(12, 8, 5, 0) });
        ServiceBox.Width = 150;
        panel.Controls.Add(ServiceBox);
        panel.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(12, 8, 5, 0) });
        StatusBox.Width = 150;
        panel.Controls.Add(StatusBox);
        return panel;
    }
    private Control BuildArea()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Bảng danh mục dịch vụ
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Phần thông tin chi tiết
        // Bảng danh mục
        serviceGrid.Dock = DockStyle.Fill;
        serviceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        serviceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        serviceGrid.ReadOnly = true;
        serviceGrid.MultiSelect = false;
        serviceGrid.SelectionChanged += (_, _) => ServiceSelected();
        panel.Controls.Add(serviceGrid, 0, 0);
        // Phần thông tin
        var detailsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1, Padding = new Padding(8) };
        detailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        serviceDetails.Dock = DockStyle.Fill;
        serviceDetails.Font = new Font("Segoe UI", 9.5f);
        serviceDetails.Text = "Chọn một dịch vụ từ danhprivate Control BuildArea()\n{\n    var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };\n    panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Bảng danh mục dịch vụ (70%)\n    panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Phần thông tin chi tiết (30%)\n\n    // Cấu hình DataGridView bên trái\n    serviceGrid.Dock = DockStyle.Fill;\n    serviceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;\n    serviceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;\n    serviceGrid.ReadOnly = true;\n    serviceGrid.MultiSelect = false;\n    serviceGrid.AllowUserToAddRows = false;\n    serviceGrid.RowHeadersVisible = false;\n    serviceGrid.SelectionChanged += (_, _) => ServiceSelected();\n    panel.Controls.Add(serviceGrid, 0, 0);\n\n    // Phần thông tin bên phải (Thu gọn lại, cho text chiếm trọn 100% không gian)\n    var detailsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1, Padding = new Padding(8) };\n    detailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));\n\n    serviceDetails.Dock = DockStyle.Fill;\n    serviceDetails.Font = new Font(\"Segoe UI\", 9.5f);\n    serviceDetails.Text = \"Chọn một dịch vụ từ danh sách để xem chi tiết.\";\n    detailsPanel.Controls.Add(serviceDetails, 0, 0);\n\n    panel.Controls.Add(detailsPanel, 1, 0);\n    return panel;\n} sách để xem chi tiết.";
        detailsPanel.Controls.Add(serviceDetails, 0, 0);
        panel.Controls.Add(detailsPanel, 1, 0);
        return panel;
    }
    private void Filter()
    {
        ServiceBox.Items.Add("Tất cả nhóm");
        ServiceBox.Items.AddRange(new object[] { "Đồ uống", "Giặt ủi", "Thuê xe", "Khác" });
        ServiceBox.SelectedIndex = 0;
        StatusBox.Items.Add("Tất cả trạng thái");
        StatusBox.Items.AddRange(new object[] { "Đang cung cấp", "Ngừng cung cấp" });
        StatusBox.SelectedIndex = 0;
        ServiceBox.SelectedIndexChanged += (_, _) => RefreshData();
        StatusBox.SelectedIndexChanged += (_, _) => RefreshData();
        // Các cột
        serviceGrid.AutoGenerateColumns = false;
        serviceGrid.Columns.Clear();
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiceCode", HeaderText = "Mã DV" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiceName", HeaderText = "Tên dịch vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryText", HeaderText = "Nhóm" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitName", HeaderText = "ĐVT" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Đơn giá" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StatusText", HeaderText = "Trạng thái" });
    }
    private void RefreshData()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var catText = ServiceBox.SelectedItem?.ToString() ?? "Tất cả nhóm";
        var statText = StatusBox.SelectedItem?.ToString() ?? "Tất cả trạng thái";
        var filtered = services.Where(s =>
            (string.IsNullOrEmpty(keyword) || s.ServiceCode.ToLower().Contains(keyword) || s.ServiceName.ToLower().Contains(keyword)) &&
            (catText == "Tất cả nhóm" || s.CategoryText == catText) &&
            (statText == "Tất cả trạng thái" || s.StatusText == statText)
        ).ToList();
        serviceGrid.DataSource = filtered;
        serviceCount.Text = $"Danh sách dịch vụ ({filtered.Count}/{services.Count} bản ghi)";
        UpdateThongKe();
    }
    private void UpdateThongKe()
    {
        var panel = ThongKePanel;
        var names = new[] { "Tổng dịch vụ", "Đang cung cấp", "Ngừng cung cấp" };
        var counts = new[]
        {
            services.Count,
            services.Count(s => s.IsActive),
            services.Count(s => !s.IsActive)
        };

        for (var i = 0; i < counts.Length; i++)
        {
            if (panel.Controls[i] is Label lbl)
            {
                lbl.Text = $"{names[i]}\n{counts[i]}";
            }
        }
    }

    private void ServiceSelected()
    {
        if (serviceGrid.SelectedRows.Count > 0 && serviceGrid.SelectedRows[0].DataBoundItem is ServiceItem item)
        {
            SelectService(item);
        }
        else
        {
            SelectService(null);
        }
    }
    private void SelectService(ServiceItem? service)
    {
        selectedService = service;
        serviceActions.Controls.Clear();

        if (service == null)
        {
            serviceDetails.Text = "Chọn một dịch vụ từ danh sách để xem chi tiết.";
            return;
        }

        serviceDetails.Text =
            $"[ THÔNG TIN CHI TIẾT ]\n\n" +
            $"Mã dịch vụ: {service.ServiceCode}\n" +
            $"Tên dịch vụ: {service.ServiceName}\n" +
            $"Nhóm dịch vụ: {service.CategoryText}\n" +
            $"Đơn vị tính: {service.UnitName}\n" +
            $"Đơn giá hiện hành: {service.UnitPrice:N0} đ\n" +
            $"Trạng thái: {service.StatusText}\n" +
            $"Mô tả: {service.Description ?? "(Không có mô tả)"}";
    }
    private List<ServiceItem> LoadServices() => new()
    {
        new(1, "NC01", "Nước khoáng Lavie 500ml", "Đồ uống", "Chai", 10000, "Nước suối đóng chai", true),
        new(2, "NC02", "Bia Heineken 330ml", "Đồ uống", "Lon", 25000, "Đồ uống có cồn", true),
        new(3, "GU01", "Giặt ủi cao cấp", "Giặt ủi", "Chiếc", 100000, "Dịch vụ giặt nhanh", true),
        new(4, "TH01", "Thuê xe Vinfast", "Thuê xe", "Ngày", 890000, "Xe điện đời mới", false)
    };
    private static Button CreateButton(string text, Action onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
        button.Click += (_, _) => onClick();
        return button;
    }
    private sealed record ServiceItem(
        int ServiceId,
        string ServiceCode,
        string ServiceName,
        string CategoryText,
        string UnitName,
        decimal UnitPrice,
        string? Description,
        bool IsActive)
    {
        public string StatusText => IsActive ? "Đang cung cấp" : "Ngừng cung cấp";
        public string UnitPriceDisplay => UnitPrice.ToString("N0") + " đ";
    }
}
