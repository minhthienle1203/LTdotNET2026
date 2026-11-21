using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HotelManagement.WinForms.Forms.Invoices;

/// S17 - Danh sách hóa đơn.

public sealed class InvoiceListView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly List<InvoiceItem> invoices;
    // Các thành phần giao diện
    private readonly TextBox SearchBox = new();
    private readonly ComboBox StatusBox = new();
    private readonly DataGridView invoiceGrid = new();
    private readonly TableLayoutPanel ThongKePanel = new();
    private readonly Label invoiceCount = new();
    private readonly Label invoiceDetails = new();
    private readonly FlowLayoutPanel invoiceActions = new();
    private readonly Button btnPrintInvoice = new() { Text = "In hóa đơn", AutoSize = true, Padding = new Padding(8, 4, 8, 4), Visible = false };
    private InvoiceItem? selectedInvoice;

    public InvoiceListView(Action<string, object?> navigate)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        invoices = LoadInvoices();
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
        layout.Controls.Add(invoiceCount, 0, 3);
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
            Text = "DANH SÁCH HÓA ĐƠN HỆ THỐNG",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Margin = new Padding(0, 5, 16, 0)
        });
        return panel;
    }
    // Các ô thống kê
    private Control BuildThongKe()
    {
        var panel = ThongKePanel;
        panel.Dock = DockStyle.Fill;
        panel.ColumnCount = 3;
        panel.RowCount = 1;
        panel.ColumnStyles.Clear();
        for (var i = 0; i < 3; i++)
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        string[] titles = { "Tổng hóa đơn", "Đã thanh toán", "Chưa thanh toán" };
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
        panel.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(12, 8, 5, 0) });
        StatusBox.Width = 150;
        panel.Controls.Add(StatusBox);
        return panel;
    }
    private Control BuildArea()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Bảng danh mục hóa đơn
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Phần thông tin chi tiết

        // Bảng danh mục hóa đơn
        invoiceGrid.Dock = DockStyle.Fill;
        invoiceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        invoiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        invoiceGrid.ReadOnly = true;
        invoiceGrid.MultiSelect = false;
        invoiceGrid.SelectionChanged += (_, _) => InvoiceSelected();
        panel.Controls.Add(invoiceGrid, 0, 0);

        // Phần thông tin
        var detailsPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
        detailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 75));
        detailsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        invoiceDetails.Dock = DockStyle.Fill;
        invoiceDetails.Font = new Font("Segoe UI", 9.5f);
        invoiceDetails.Text = "Chọn một hóa đơn từ danh sách";
        detailsPanel.Controls.Add(invoiceDetails, 0, 0);
        var actionRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        btnPrintInvoice.Margin = new Padding(0, 4, 0, 0);
        btnPrintInvoice.Click += (_, _) =>
        {
            if (selectedInvoice == null)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn trước");
                return;
            }
            MessageBox.Show($"Đang in hóa đơn {selectedInvoice.InvoiceCode}");
        };
        actionRow.Controls.Add(btnPrintInvoice);
        detailsPanel.Controls.Add(actionRow, 0, 1);

        panel.Controls.Add(detailsPanel, 1, 0);
        return panel;
    }
    private void Filter()
    {
        StatusBox.Items.Add("Tất cả trạng thái");
        StatusBox.Items.AddRange(new object[] { "In", "Chưa in" });
        StatusBox.SelectedIndex = 0;
        StatusBox.SelectedIndexChanged += (_, _) => RefreshData();

        // Các cột hóa đơn: mã hóa đơn, ngày lập, khách hàng, phòng, tổng tiền, trạng thái
        invoiceGrid.AutoGenerateColumns = false;
        invoiceGrid.Columns.Clear();
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "InvoiceCode", HeaderText = "Mã hóa đơn" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IssuedDateText", HeaderText = "Ngày lập" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Khách hàng" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomCode", HeaderText = "Phòng" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Tổng tiền" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PrintStatusText", HeaderText = "Trạng thái" });
    }
    private void RefreshData()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var printFilter = StatusBox.SelectedItem?.ToString() ?? "Tất cả trạng thái";
        var filtered = invoices.Where(inv =>
            (string.IsNullOrEmpty(keyword) ||
             inv.InvoiceCode.ToLower().Contains(keyword) ||
             inv.CustomerName.ToLower().Contains(keyword) ||
             inv.RoomCode.ToLower().Contains(keyword)) &&
            (printFilter == "Tất cả trạng thái" || inv.PrintStatusText == printFilter)
        ).ToList();
        invoiceGrid.DataSource = filtered;
        invoiceCount.Text = $"Danh sách hóa đơn ({filtered.Count}/{invoices.Count} bản ghi)";
        UpdateThongKe();
    }
    private void UpdateThongKe()
    {
        var panel = ThongKePanel;
        var names = new[] { "Tổng hóa đơn", "Đã thanh toán", "Chưa thanh toán" };
        var counts = new[]
        {
            invoices.Count,
            invoices.Count(inv => inv.IsPaid),
            invoices.Count(inv => !inv.IsPaid)
        };
        for (var i = 0; i < counts.Length; i++)
        {
            if (panel.Controls[i] is Label lbl)
            {
                lbl.Text = $"{names[i]}\n{counts[i]}";
            }
        }
    }

    //Hàm kiểm tra lựa chọn
    private void InvoiceSelected()
    {
        if (invoiceGrid.SelectedRows.Count > 0 && invoiceGrid.SelectedRows[0].DataBoundItem is InvoiceItem item)
        {
            SelectInvoice(item);
        }
        else
        {
            SelectInvoice(null);
        }
    }
    private void SelectInvoice(InvoiceItem? invoice)
    {
        selectedInvoice = invoice;
        invoiceActions.Controls.Clear();

        if (invoice == null)
        {
            btnPrintInvoice.Visible = false;
            invoiceDetails.Text = "Chọn một hóa đơn từ danh sách để xem chi tiết.";
            return;
        }
        btnPrintInvoice.Visible = !invoice.IsPrinted;
        invoiceDetails.Text =
            $"[ THÔNG TIN CHI TIẾT ]\n\n" +
            $"Mã hóa đơn: {invoice.InvoiceCode}\n" +
            $"Ngày lập: {invoice.IssuedDateText}\n" +
            $"Khách hàng: {invoice.CustomerName}\n" +
            $"Phòng: {invoice.RoomCode}\n" +
            $"Tổng tiền: {invoice.TotalAmount:N0} đ\n" +
            $"Thanh toán: {invoice.PaymentStatusText}\n" +
            $"Trạng thái in: {invoice.PrintStatusText}";
    }
    private List<InvoiceItem> LoadInvoices() => new()
    {
        new(1, "HD001", new DateTime(2026, 11, 20), "Nguyễn Văn An", "P101", 1250000m, true, true),
        new(2, "HD002", new DateTime(2026, 11, 20), "Trần Thị Bình", "P203", 850000m, false, false),
        new(3, "HD003", new DateTime(2026, 11, 21), "Lê Hoàng Cường", "P305", 2400000m, true, false),
        new(4, "HD004", new DateTime(2026, 11, 21), "Phạm Minh Dung", "P102", 520000m, false, true)
    };
    private static Button CreateButton(string text, Action onClick)
    {
        var button = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
        button.Click += (_, _) => onClick();
        return button;
    }
    private sealed record InvoiceItem(
        int InvoiceId,
        string InvoiceCode,
        DateTime IssuedDate,
        string CustomerName,
        string RoomCode,
        decimal TotalAmount,
        bool IsPaid,
        bool IsPrinted)
    {
        public string IssuedDateText => IssuedDate.ToString("dd/MM/yyyy");
        public string PaymentStatusText => IsPaid ? "Đã thanh toán" : "Chưa thanh toán";
        public string PrintStatusText => IsPrinted ? "In" : "Chưa in";
        public string TotalAmountDisplay => TotalAmount.ToString("N0") + " đ";
    }
}
