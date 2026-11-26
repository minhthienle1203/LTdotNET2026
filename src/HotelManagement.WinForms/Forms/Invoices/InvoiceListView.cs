using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Invoices;

/// S17 - Danh sách hóa đơn.

public sealed class InvoiceListView : UserControl
{
    private readonly Action<string, object?> navigate;
    private List<InvoiceItem> invoices;

    private readonly InvoiceService _invoiceService;

    // Các thành phần giao diện
    private readonly TextBox SearchBox = new();
    private readonly ComboBox StatusBox = new();
    private readonly DataGridView invoiceGrid = new();
    private readonly TableLayoutPanel ThongKePanel = new();
    private readonly Label invoiceCount = new();
    private readonly Label invoiceDetails = new();

    private readonly Button btnPrintInvoice = new()
    {
        Text = "IN HÓA ĐƠN", AutoSize = true, Width = 75, Height = 40, Font = new Font("Segoe UI", 8, FontStyle.Bold),
        Visible = false
    };

    private InvoiceItem? selectedInvoice;
    private static readonly string[] Stat =
    {
        "TỔNG HÓA ĐƠN",
        "ĐÃ THANH TOÁN",
        "CHƯA THANH TOÁN",
        "ĐÃ IN",
        "CHƯA IN"

    };

    public InvoiceListView(Action<string, object?> navigate, InvoiceService invoiceService)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
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
        invoiceCount.Dock = DockStyle.Fill;
        invoiceCount.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(invoiceCount, 0, 3);
        layout.Controls.Add(BuildArea(), 0, 4);
        Controls.Add(layout);
        LoadInvoices();
        Filter();
        RefreshData();
    }

    private void LoadInvoices()
    {
        IReadOnlyList<Invoice> entities = _invoiceService.GetAll();
        invoices = entities.Select(inv => new InvoiceItem(
            InvoiceId: inv.InvoiceId,
            InvoiceCode: inv.InvoiceCode,
            StayId: inv.StayId,
            IssuedDate: inv.CreatedAt,
            CustomerName: $"Stay #{inv.StayId}",
            RoomCode: $"#{inv.StayId}",
            TotalAmount: inv.AmountDue,
            IsPaid: inv.Status == InvoiceStatus.Paid,
            IsPrinted: inv.PrintedAt.HasValue,
            ChargeBasic: inv.ChargeBasis == ChargeBasis.Day ? "Theo ngày" : "Theo giờ",
            ChargedUnits: inv.ChargedUnits,
            AppliedRate: inv.AppliedRate,
            RoomCharge: inv.RoomCharge,
            ServiceCharge: inv.ServiceCharge,
            SubTotal: inv.SubTotal,
            DepositDeducted: inv.DepositDeducted,
            AmountDue: inv.AmountDue,
            RefundDue: inv.RefundDue,
            PrintedAt: inv.PrintedAt
        )).ToList();
    }

    private Control BuildHeader()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        panel.Controls.Add(new Label
        {
            Text = "DANH SÁCH HÓA ĐƠN HỆ THỐNG",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
        });
        return panel;
    }

    // Các ô thống kê
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
        SearchBox.Width = 160;
        SearchBox.TextChanged += (_, _) => RefreshData();
        panel.Controls.Add(SearchBox);
        panel.Controls.Add(new Label { Text = "Trạng Thái:", AutoSize = true, Margin = new Padding(12, 8, 5, 0) });
        StatusBox.Width = 175;
        panel.Controls.Add(StatusBox);
        return panel;
    }

    private Control BuildArea()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65)); // Bảng danh mục hóa đơn
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35)); // Phần thông tin chi tiết

        // Bảng danh mục hóa đơn
        invoiceGrid.Dock = DockStyle.Fill;
        invoiceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        invoiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        invoiceGrid.ReadOnly = true;
        invoiceGrid.MultiSelect = false;
        invoiceGrid.SelectionChanged += (_, _) => InvoiceSelected();
        panel.Controls.Add(invoiceGrid, 0, 0);

        // Phần thông tin
        var detailsPanel = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
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
            _invoiceService.MarkAsPrinted(selectedInvoice.InvoiceId, DateTime.Now);
            LoadInvoices();
            RefreshData();
            SelectInvoice(invoices.FirstOrDefault(x => x.InvoiceId == selectedInvoice.InvoiceId));
            MessageBox.Show($"Đã in hóa đơn {selectedInvoice.InvoiceCode}");
        };
        actionRow.Controls.Add(btnPrintInvoice);
        detailsPanel.Controls.Add(actionRow, 0, 1);
        panel.Controls.Add(detailsPanel, 1, 0);
        return panel;
    }

    private void Filter()
    {
        StatusBox.Items.Add("Tất cả trạng thái");
        StatusBox.Items.AddRange(new object[] { "In", "Chưa in", "Chưa thanh toán", "Đã thanh toán" });
        StatusBox.SelectedIndex = 0;
        StatusBox.SelectedIndexChanged += (_, _) => RefreshData();

        // Các cột hóa đơn: mã hóa đơn, ngày lập, khách hàng, phòng, tổng tiền, trạng thái
        invoiceGrid.ColumnHeadersDefaultCellStyle.Font =
            new Font(invoiceGrid.Font, FontStyle.Bold);
        invoiceGrid.AutoGenerateColumns = false;
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "InvoiceCode", HeaderText = "Mã Hóa Đơn" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "IssuedDateText", HeaderText = "Ngày Lập" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "CustomerName", HeaderText = "Lượt Lưu Trú" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RoomCode", HeaderText = "Phòng" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "TotalAmountDisplay", HeaderText = "Tổng Tiền" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "PrintStatusText", HeaderText = "Trạng Thái In" });
        invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "PaymentStatusText", HeaderText = "Thanh Toán" });
    }

    private void RefreshData()
    {
        var keyword = SearchBox.Text.Trim().ToLower();
        var printFilter = StatusBox.SelectedItem?.ToString() ?? "Tất cả trạng thái";
        var paidFilter = StatusBox.SelectedItem?.ToString() ?? "Tất cả trạng thái";
        var filtered = invoices.Where(inv =>
            (string.IsNullOrEmpty(keyword) ||
             inv.InvoiceCode.ToLower().Contains(keyword) ||
             inv.CustomerName.ToLower().Contains(keyword) ||
             inv.RoomCode.ToLower().Contains(keyword)) &&
            (printFilter == "Tất cả trạng thái" || inv.PrintStatusText == printFilter ||
             inv.PaymentStatusText == paidFilter)
        ).ToList();
        invoiceGrid.DataSource = filtered;
        invoiceCount.Text = $"Danh sách hóa đơn đã in({filtered.Count}/{invoices.Count})";
        UpdateThongKe();
    }

    private void UpdateThongKe()
    {
        var counts = new[]
        {
            invoices.Count,
            invoices.Count(s => s.IsPaid),
            invoices.Count(s => !s.IsPaid),
            invoices.Count(s => s.IsPrinted),
            invoices.Count(s => !s.IsPrinted),


        };
        for (var i = 0; i < Stat.Length && i < ThongKePanel.Controls.Count; i++)
        {
            if (ThongKePanel.Controls[i] is Label lbl)
                lbl.Text = $"{Stat[i]}\n{counts[i]}";
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
            $"Lượt lưu trú (StayId): #{invoice.StayId}\n" +
            $"Ngày lập: {invoice.IssuedDateText}\n" +
            $"Thanh toán: {invoice.PaymentStatusText}\n" +
            $"Trạng thái in: {invoice.PrintStatusText}\n" +
            $"Thời điểm in: {(invoice.PrintedAt.HasValue ? invoice.PrintedAt.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa in")}\n\n" +
            $"[ CHI TIẾT TIỀN ]\n" +
            $"Tiền phòng: {invoice.RoomCharge:N0} đ\n" +
            $"Tiền dịch vụ: {invoice.ServiceCharge:N0} đ\n" +
            $"Tạm tính: {invoice.SubTotal:N0} đ\n" +
            $"Tiền cọc trừ: {invoice.DepositDeducted:N0} đ\n" +
            $"Khách cần trả: {invoice.AmountDue:N0} đ\n" +
            $"Tiền hoàn lại: {invoice.RefundDue:N0} đ";
    }


    private sealed record InvoiceItem(
        int InvoiceId,
        string InvoiceCode,
        int StayId,
        DateTime IssuedDate,
        string CustomerName,
        string RoomCode,
        decimal TotalAmount,
        bool IsPaid,
        bool IsPrinted,
        string ChargeBasic,
        int ChargedUnits,
        decimal AppliedRate,
        decimal RoomCharge,
        decimal ServiceCharge,
        decimal SubTotal,
        decimal DepositDeducted,
        decimal AmountDue,
        decimal RefundDue,
        DateTime? PrintedAt)
    {
        public string IssuedDateText => IssuedDate.ToString("dd/MM/yyyy");
        public string PaymentStatusText => IsPaid ? "Đã thanh toán" : "Chưa thanh toán";
        public string PrintStatusText => IsPrinted ? "In" : "Chưa in";
        public string TotalAmountDisplay => TotalAmount.ToString("N0") + "đ";
    }
}

