using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HotelManagement.WinForms.Forms.Services;

/// S13 - Danh mục dịch vụ.

public sealed class ServiceCatalogView : UserControl
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
    private static readonly string[] Stat =
    {
        "TỔNG DỊCH VỤ",
        "ĐANG CUNG CẤP",
        "NGỪNG CUNG CẤP"
    };
    private ServiceItem? selectedService;
    public ServiceCatalogView(Action<string, object?> navigate)
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
        serviceCount.Dock = DockStyle.Fill;
        serviceCount.TextAlign = ContentAlignment.MiddleLeft;
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
        });
        return panel;
    }

    //  Các ô thoongs kê
    private Control BuildThongKe()
    {
        ThongKePanel.Dock = DockStyle.Fill;
        ThongKePanel.ColumnCount = Stat.Length;
        ThongKePanel.RowCount = 1;
        for (var i = 0; i < Stat.Length; i++)
        {
            ThongKePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / Stat.Length));
            var item = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(2),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            };
            ThongKePanel.Controls.Add(item, i, 0);
        }

        return ThongKePanel;
    }

    private Control BuildFilters()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Tìm Kiếm:", AutoSize = true, Margin = new Padding(0, 8, 5, 0) });
        SearchBox.Width = 300;
        SearchBox.TextChanged += (_, _) => RefreshData();
        panel.Controls.Add(SearchBox);
        panel.Controls.Add(new Label { Text = "Nhóm:", AutoSize = true, Margin = new Padding(75, 8, 5, 0) });
        ServiceBox.Width = 140;
        panel.Controls.Add(ServiceBox);
        panel.Controls.Add(new Label { Text = "Trạng Thái:", AutoSize = true, Margin = new Padding(150, 8, 5, 0) });
        StatusBox.Width = 175;
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
        var detailsPanel = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1, Padding = new Padding(8) };
        detailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        serviceDetails.Dock = DockStyle.Fill;
        serviceDetails.Font = new Font("Segoe UI", 9.5f);
        serviceDetails.Text = "Chọn một dịch vụ từ danh sách";
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
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "ServiceCode", HeaderText = "Mã Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "ServiceName", HeaderText = "Tên Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "CategoryText", HeaderText = "Nhóm Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitName", HeaderText = "Đơn Vị" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "UnitPriceDisplay", HeaderText = "Đơn Giá" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "StatusText", HeaderText = "Trạng Thái" });
    }

    private void RefreshData()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var catText = ServiceBox.SelectedItem?.ToString() ?? "Tất cả nhóm";
        var statText = StatusBox.SelectedItem?.ToString() ?? "Tất cả trạng thái";
        var filtered = services.Where(s =>
            (string.IsNullOrEmpty(keyword) || s.ServiceCode.ToLower().Contains(keyword) ||
             s.ServiceName.ToLower().Contains(keyword)) &&
            (catText == "Tất cả nhóm" || s.CategoryText == catText) &&
            (statText == "Tất cả trạng thái" || s.StatusText == statText)
        ).ToList();
        serviceGrid.DataSource = filtered;
        serviceCount.Text = $"Danh sách dịch vụ (Hiển thị {filtered.Count}/{services.Count})";
        UpdateThongKe();
    }

    private void UpdateThongKe()
    {
        var counts = new[]
        {
            services.Count,
            services.Count(s => s.IsActive),
            services.Count(s => !s.IsActive)
        };
        for (var i = 0; i < Stat.Length && i < ThongKePanel.Controls.Count; i++)
        {
            if (ThongKePanel.Controls[i] is Label lbl)
                lbl.Text = $"{Stat[i]}\n{counts[i]}";
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
            $"Đơn giá hiện hành: {service.UnitPriceDisplay}\n" +
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
        public string UnitPriceDisplay => UnitPrice.ToString("N0") + "đ";
    }
}
