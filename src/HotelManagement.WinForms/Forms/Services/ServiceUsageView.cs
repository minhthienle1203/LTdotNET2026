using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Services;

/// S14 - Ghi nhan dịch vụ.

public sealed class ServiceUsageView : UserControl
{
    private readonly Action<string, object?> navigate;
    private List<ServiceItem> services;
    private readonly HotelServiceService ServiceCatalog;
    private readonly StayServiceService _stayServiceService;
    private int _currentStayId;
    private string? _currentRoomCode;

    // Các thành phần giao diện
    private readonly TextBox SearchBox = new();
    private readonly ComboBox ServiceBox = new();
    private readonly ComboBox StatusBox = new();
    private readonly DataGridView serviceGrid = new();
    private readonly DataGridView usedServiceGrid = new();
    private readonly TableLayoutPanel ThongKePanel = new();
    private readonly Label serviceCount = new();
    private readonly Label serviceDetails = new();
    private readonly Label targetStayLabel = new()
    {
        Text = "Chưa chọn lượt lưu trú (Stay)",
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        Margin = new Padding(16, 6, 0, 0)
    };
    private readonly NumericUpDown quantityInput = new() { Minimum = 1, Maximum = 999, Value = 1, Width = 65,DecimalPlaces = 0 };
    private readonly Button btnRecord = new() { Text = "THÊM", Width = 75, Height = 40, Font = new Font("Segoe UI", 8, FontStyle.Bold)};
    private readonly Button btnRemove = new() { Text = "XÓA", Width = 75, Height = 40, Font = new Font("Segoe UI", 8, FontStyle.Bold), Enabled = false }; // [THÊM] Nút xóa dịch vụ đã dùng
    private readonly Label totalAmountLabel = new()
    {
        Text = "Tổng tiền: 0đ",
        AutoSize = true,
        Margin = new Padding(10, 6, 4, 0)
    };
    private static readonly string[] Stat =
    {
        "TỔNG DỊCH VỤ",
        "ĐANG CUNG CẤP",
        "NGỪNG CUNG CẤP"
    };
    private ServiceItem? selectedService;

    public ServiceUsageView(Action<string, object?> navigate, HotelServiceService serviceServiceCatalog, StayServiceService stayServiceService)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        ServiceCatalog = serviceServiceCatalog ?? throw new ArgumentNullException(nameof(serviceServiceCatalog));
        _stayServiceService = stayServiceService ?? throw new ArgumentNullException(nameof(stayServiceService));
        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        AutoScroll = true;
        MinimumSize = new Size(0, 650);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
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
        serviceCount.Dock = DockStyle.Fill;
        serviceCount.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(serviceCount, 0, 3);
        layout.Controls.Add(BuildArea(), 0, 4);
        Controls.Add(layout);
        ConfigureUsedServiceGrid(); // Cấu hình cột và sự kiện cho usedServiceGrid
        LoadServices();
        Filter();
        RefreshData();
        ReloadUsedServices(); // Tải danh sách đã dùng
    }
    public void SetStayContext(int stayId, string? roomCode = null)
    {
        _currentStayId = stayId;
        _currentRoomCode = string.IsNullOrWhiteSpace(roomCode) ? null : roomCode.Trim();
        if (_currentStayId > 0)
        {
            targetStayLabel.Text = _currentRoomCode != null
                ? $"Đang ghi cho: Phòng {_currentRoomCode} (Stay #{_currentStayId})"
                : $"Đang ghi cho: Stay #{_currentStayId}";
            btnRecord.Enabled = true;
        }
        else
        {
            targetStayLabel.Text = "Chưa chọn lượt lưu trú (Stay)";
            btnRecord.Enabled = false;
            btnRemove.Enabled = false;
        }
        ReloadUsedServices();
    }

    private void LoadServices()
    {
        IReadOnlyList<HotelService> entities = ServiceCatalog.GetAll(includeInactive: true);
        services = entities.Select(s => new ServiceUsageView.ServiceItem(
            s.ServiceId,
            s.ServiceCode,
            s.ServiceName,
            ToCategoryText(s.Category),
            s.UnitName,
            s.UnitPrice,
            s.Description,
            s.IsActive
        )).ToList();
    }
private static string ToCategoryText(ServiceCategory category) => category switch
{
    ServiceCategory.FoodDrink => "Đồ uống",
    ServiceCategory.Laundry => "Giặt ủi",
    ServiceCategory.Transport => "Thuê xe",
    _ => "Khác"
};

    private Control BuildHeader()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label
        {
            Text = "DANH MỤC DỊCH VỤ HỆ THỐNG",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
        });
        panel.Controls.Add(targetStayLabel);
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
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); // Bảng danh mục dịch vụ
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); // Phần thông tin chi tiết
        var leftPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        // Bảng danh mục
        serviceGrid.Dock = DockStyle.Fill;
        serviceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        serviceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        serviceGrid.ReadOnly = true;
        serviceGrid.MultiSelect = false;
        serviceGrid.SelectionChanged += (_, _) => ServiceSelected();
        leftPanel.Controls.Add(serviceGrid, 0, 0);
        var usedHeaderRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            Margin = new Padding(0)
        };
        usedHeaderRow.Controls.Add(new Label
        {
            Text = "DỊCH VỤ ĐÃ GHI NHẬN CHO LƯỢT LƯU TRÚ",
            AutoSize = true,
            Margin = new Padding(0, 5, 10, 0),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        });
        btnRemove.Width = 65;
        btnRemove.Height = 22;
        btnRemove.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        btnRemove.Margin = new Padding(0, 1, 0, 0);
        usedHeaderRow.Controls.Add(btnRemove);
        leftPanel.Controls.Add(usedHeaderRow, 0, 1);
        usedServiceGrid.Dock = DockStyle.Fill;
        leftPanel.Controls.Add(usedServiceGrid, 0, 2); // Đặt usedServiceGrid vào ô (0, 2) lefPanel
        panel.Controls.Add(leftPanel, 0, 0); // Đưa leftPanel vào ô (0, 0) của panel chính
        // Phần thông tin
        var detailsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
        detailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        detailsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        serviceDetails.Dock = DockStyle.Fill;
        serviceDetails.Font = new Font("Segoe UI", 9.5f);
        serviceDetails.Text = "Chọn một dịch vụ từ danh sách";
        detailsPanel.Controls.Add(serviceDetails, 0, 0);
        var actionRow = new FlowLayoutPanel { Dock = DockStyle.Fill,WrapContents = true, AutoScroll = true };
        actionRow.Controls.Add(new Label { Text = "Số Lượng:", AutoSize = true, Margin = new Padding(0, 6, 4, 0) });
        quantityInput.ValueChanged += (_, _) => UpdateTotalAmount();
        actionRow.Controls.Add(quantityInput);
        actionRow.Controls.Add(totalAmountLabel);
        btnRecord.Margin = new Padding(8, 0, 0, 0);
        //Xử lý sự kiện button
        btnRecord.Click += (_, _) =>
        {
            if (_currentStayId <= 0)
            {
                MessageBox.Show("Vui lòng chọn một phòng đang có khách trước khi ghi dịch vụ.");
                return;
            }
            if (selectedService == null)
            {
                MessageBox.Show("Vui lòng chọn một dịch vụ.");
                return;
            }

            if (!selectedService.IsActive)
            {
                MessageBox.Show("Dịch vụ này đang ngừng cung cấp, không thể ghi nhận.");
                return;
            }
            try
            {
                int qty = (int)quantityInput.Value;
                _stayServiceService.AddServiceToStay(
                    stayId: _currentStayId,
                    serviceId: selectedService.ServiceId,
                    quantity: qty,
                    unitPrice: selectedService.UnitPrice,
                    usedAt: DateTime.Now,
                    note: _currentRoomCode != null ? $"Phòng {_currentRoomCode}" : null
                );
                var total = selectedService.UnitPrice * qty;
                MessageBox.Show($"Đã ghi nhận {qty}x {selectedService.ServiceName} ({total:N0}đ) cho Stay #{_currentStayId}.");
                quantityInput.Value = 1;
                ReloadUsedServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu dịch vụ: {ex.Message}");
            }
        };
        actionRow.Controls.Add(btnRecord);
        btnRemove.Margin = new Padding(6, 0, 0, 0);
        btnRemove.Click += (_, _) => // Xử lý sự kiện xóa dịch vụ đã ghi nhận
        {
            if (_currentStayId <= 0)
            {
                MessageBox.Show("Vui lòng chọn lượt lưu trú trước khi xóa dịch vụ.");
                return;
            }
            if (usedServiceGrid.CurrentRow?.DataBoundItem is not UsedServiceRow selectedRow)
            {
                MessageBox.Show("Vui lòng chọn dịch vụ đã ghi nhận cần xóa.");
                return;
            }
            var confirm = MessageBox.Show(
                $"Xóa dịch vụ \"{selectedRow.ServiceName}\" (SL: {selectedRow.Quantity:0.###}, Thành tiền: {selectedRow.Amount:N0}đ)?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;
            try
            {
                _stayServiceService.RemoveServiceFromStay(selectedRow.StayServiceId);
                ReloadUsedServices();
                MessageBox.Show("Đã xóa dịch vụ.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa dịch vụ: {ex.Message}");
            }
        };
        detailsPanel.Controls.Add(actionRow, 0, 1);
        panel.Controls.Add(detailsPanel, 1, 0);
        return panel;
    }

    private void ConfigureUsedServiceGrid() // Làm bảng dịch vụ đã dùng
    {
        usedServiceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        usedServiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        usedServiceGrid.ReadOnly = true;
        usedServiceGrid.MultiSelect = false;
        usedServiceGrid.AutoGenerateColumns = false;
        usedServiceGrid.ColumnHeadersDefaultCellStyle.Font = new Font(usedServiceGrid.Font, FontStyle.Bold);
        usedServiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiceName", HeaderText = "Dịch Vụ", FillWeight = 130 });
        usedServiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "QuantityDisplay", HeaderText = "SL", FillWeight = 45 });
        usedServiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPriceDisplay", HeaderText = "Đơn Giá", FillWeight = 85 });
        usedServiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AmountDisplay", HeaderText = "Thành Tiền", FillWeight = 95 });
        usedServiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UsedAtDisplay", HeaderText = "Thời Gian", FillWeight = 110 });
        usedServiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Note", HeaderText = "Ghi Chú", FillWeight = 90 });
    }
    private void ReloadUsedServices() // Tải lại danh sách dịch vụ đã dùng của _currentStayId
    {
        if (_currentStayId <= 0)
        {
            usedServiceGrid.DataSource = null;
            btnRemove.Enabled = false;
            return;
        }
        var serviceLookup = services?.ToDictionary(s => s.ServiceId, s => s.ServiceName) ?? new Dictionary<int, string>();
        IReadOnlyList<StayService> usedItems = _stayServiceService.GetByStayId(_currentStayId);
        var rows = usedItems.Select(item => new UsedServiceRow(
            item.StayServiceId,
            serviceLookup.TryGetValue(item.ServiceId, out var name) ? name : $"Dịch vụ #{item.ServiceId}",
            item.Quantity,
            item.UnitPrice,
            item.Amount,
            item.UsedAt,
            item.Note
        )).ToList();
        usedServiceGrid.DataSource = rows;
        btnRemove.Enabled = rows.Count > 0;
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
        serviceGrid.ColumnHeadersDefaultCellStyle.Font =
            new Font(serviceGrid.Font, FontStyle.Bold);
        serviceGrid.AutoGenerateColumns = false;
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiceCode", HeaderText = "Mã Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ServiceName", HeaderText = "Tên Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryText", HeaderText = "Nhóm Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitName", HeaderText = "Đơn Vị" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPriceDisplay", HeaderText = "Đơn Giá" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StatusText", HeaderText = "Trạng Thái" });
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
        quantityInput.Value = 1;
        if (service == null)
        {
            serviceDetails.Text = "Chọn một dịch vụ từ danh sách để xem chi tiết.";
            UpdateTotalAmount();
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
        UpdateTotalAmount();
    }
    private void UpdateTotalAmount()
    {
        if (selectedService == null)
        {
            totalAmountLabel.Text = "Tổng tiền: 0đ";
            return;
        }

        var total = selectedService.UnitPrice * quantityInput.Value;
        totalAmountLabel.Text = $"Tổng tiền: {total:N0}đ";
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

    private sealed record UsedServiceRow( // Dòng dữ liệu hiển thị cho bảng dịch vụ đã dùng
        int StayServiceId,
        string ServiceName,
        decimal Quantity,
        decimal UnitPrice,
        decimal Amount,
        DateTime UsedAt,
        string? Note)
    {
        public string QuantityDisplay => Quantity.ToString("0");
        public string UnitPriceDisplay => UnitPrice.ToString("N0") + "đ";
        public string AmountDisplay => Amount.ToString("N0") + "đ";
        public string UsedAtDisplay => UsedAt.ToString("dd/MM/yyyy HH:mm");
    }
}
