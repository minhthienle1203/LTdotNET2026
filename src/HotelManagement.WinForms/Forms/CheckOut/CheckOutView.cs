using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;
using HotelManagement.Core.Services;

namespace HotelManagement.WinForms.Forms.Stays;

/// S16 - Trả phòng và thanh toán.
public sealed class CheckOutView : UserControl
{
    private readonly Action<string, object?> navigate;
    private readonly CheckoutService checkoutService;
    private readonly int stayId;
    private Stay? currentStay;
    private Room? currentRoom;
    private string customerName = "---";
    private decimal depositAmount;
    private decimal serviceCharge;
    private decimal roomCharge;
    private int? createdInvoiceId;

    private readonly Label lblStayInfo = new()
    {
        AutoSize = true,
        Text = "Phòng: --- | Khách: --- | Cọc: 0 đ\nCheck-in: --- | Thời gian ở: 0 giờ"
    };

    private readonly DateTimePicker dtpCheckOut = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd/MM/yyyy HH:mm",
        Width = 215
    };

    private readonly RadioButton rdoByDay = new()
    {
        Text = "Tính theo ngày",
        Checked = true,
        AutoSize = true,
        Margin = new Padding(0, 4, 14, 0)
    };

    private readonly RadioButton rdoByHour = new()
    {
        Text = "Tính theo giờ",
        AutoSize = true,
        Margin = new Padding(0, 4, 14, 0)
    };

    private readonly Label lblRoomTotal = new()
    {
        Text = "Tiền buồng: 0 đ",
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        Margin = new Padding(8, 5, 0, 0)
    };

    private readonly DataGridView serviceGrid = new();

    private readonly Label lblServiceTotal = new()
    {
        Text = "Tổng tiền dịch vụ: 0 đ",
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        Dock = DockStyle.Bottom
    };

    private readonly Label lblSummary = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f),
        Text = "Tiền buồng: 0 đ\nTiền dịch vụ: 0 đ\nTạm tính: 0 đ\nTrừ cọc: 0 đ"
    };

    private readonly Label lblAmountDue = new()
    {
        Text = "THỰC THU: 0 đ",
        AutoSize = true,
        Font = new Font("Segoe UI", 12, FontStyle.Bold),
        ForeColor = Color.OrangeRed,
        Margin = new Padding(0, 10, 0, 0)
    };

    private readonly ComboBox cboPayMethod = new()
    {
        Width = 170
    };

    private readonly TextBox txtNote = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true
    };

    private readonly Button btnPay = new()
    {
        Text = "Thanh toán",
        AutoSize = true,
        Padding = new Padding(8, 5, 8, 5),
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
    };

    private readonly Button btnPrint = new()
    {
        Text = "In hóa đơn",
        AutoSize = true,
        Padding = new Padding(16, 5, 8, 5),
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
    };

    public CheckOutView(Action<string, object?> navigate, CheckoutService checkoutService, int stayId)
    {
        this.navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        this.checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        this.stayId = stayId;
        Dock = DockStyle.Fill;
        Padding = new Padding(10);
        AutoScroll = true;
//Phân tiêu đề và nội dung
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(new Label
        {
            Text = "S16 - TRẢ PHÒNG VÀ THANH TOÁN",
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold)
        }, 0, 0);
        root.Controls.Add(BuildBody(), 0, 1);
        Controls.Add(root);

        InitGrid();
        InitEvents();
        LoadCheckOutData();
        LoadUsedServices();
        RecalculateSummary();
    }

    private Control BuildBody()
    {
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        body.Controls.Add(BuildLeft(), 0, 0);
        body.Controls.Add(BuildRight(), 1, 0);
        return body;
    }

    private Control BuildLeft()
    {
        var left = new TableLayoutPanel
            { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0, 0, 8, 0) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        //Thông tin lưu trú
        var Stay1 = new GroupBox
            { Text = "Thông tin lượt lưu trú", Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 8) };
        var Stay2 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        Stay2.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        Stay2.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var pOut = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        pOut.Controls.Add(new Label { Text = "Check-out:", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        pOut.Controls.Add(dtpCheckOut);
        Stay2.Controls.Add(lblStayInfo, 0, 0);
        Stay2.Controls.Add(pOut, 0, 1);
        Stay1.Controls.Add(Stay2);

        //Tính tiền buồng
        var Rate1 = new GroupBox
            { Text = "Phương thức tính tiền buồng", Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 6) };
        var Rate2 = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        Rate2.Controls.AddRange(new Control[] { rdoByDay, rdoByHour, lblRoomTotal });
        Rate1.Controls.Add(Rate2);

        // Danh mục dịch vụ
        var dv = new GroupBox
            { Text = "Dịch vụ đã sử dụng", Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 8) };
        serviceGrid.Dock = DockStyle.Fill;
        serviceGrid.AllowUserToAddRows = false;
        serviceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        serviceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        serviceGrid.ReadOnly = true;
        serviceGrid.MultiSelect = false;
        serviceGrid.AutoGenerateColumns = false;
        serviceGrid.RowHeadersVisible = false;
        dv.Controls.Add(serviceGrid);
        dv.Controls.Add(lblServiceTotal);

        left.Controls.Add(Stay1, 0, 0);
        left.Controls.Add(Rate1, 0, 1);
        left.Controls.Add(dv, 0, 2);
        return left;
    }

    private Control BuildRight()
    {
        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
        //Chi tiết thanh toán
        var Sum1 = new GroupBox
            { Text = "Chi tiết thanh toán", Dock = DockStyle.Fill, Padding = new Padding(10, 18, 10, 8) };
        var Sum2 = new FlowLayoutPanel
            { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        Sum2.Controls.AddRange(new Control[] { lblSummary, lblAmountDue });
        Sum1.Controls.Add(Sum2);

        //Thanh toán
        var Pay1 = new GroupBox
            { Text = "Phương thức thanh toán", Dock = DockStyle.Fill, Padding = new Padding(8, 18, 8, 8) };
        var Pay2 = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        Pay2.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        Pay2.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        Pay2.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Pay2.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        cboPayMethod.Items.AddRange(new object[] { "Tiền mặt", "Thanh toán mô phỏng" });
        cboPayMethod.SelectedIndex = 0;
        var lblNote = new Label
        {
            Text = "Phần ghi chú:",
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0)
        };
        var pBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        btnPrint.Margin = new Padding(6, 0, 0, 0);
        pBtn.Controls.AddRange(new Control[] { btnPay, btnPrint });
        Pay2.Controls.Add(cboPayMethod, 0, 0);
        Pay2.Controls.Add(lblNote, 0, 1);
        Pay2.Controls.Add(txtNote, 0, 2);
        Pay2.Controls.Add(pBtn, 0, 3);
        Pay1.Controls.Add(Pay2);
        right.Controls.Add(Sum1, 0, 0);
        right.Controls.Add(Pay1, 0, 1);
        return right;
    }

//Danh sachs dịch vụ
    private void InitGrid()
    {
        serviceGrid.Columns.Clear();
        serviceGrid.ColumnHeadersDefaultCellStyle.Font = new Font(serviceGrid.Font, FontStyle.Bold);
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colService", HeaderText = "Dịch Vụ" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty", HeaderText = "Số Lượng" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPrice", HeaderText = "Đơn Giá" });
        serviceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAmount", HeaderText = "Thành Tiền" });
    }

//Xử lý khi chọn
    private void InitEvents()
    {
        rdoByDay.CheckedChanged += (_, _) => RecalculateSummary();
        rdoByHour.CheckedChanged += (_, _) => RecalculateSummary();
        dtpCheckOut.ValueChanged += (_, _) => RecalculateSummary();
        btnPay.Click += (_, _) => HandlePay();
        btnPrint.Click += (_, _) => HandlePrint();
    }

    private void HandlePrint()
    {
        bool isPaid = createdInvoiceId.HasValue || checkoutService.IsPaid(stayId);
        MessageBox.Show(isPaid
            ? "Đang in hóa đơn (Trạng thái: Đã thanh toán)."
            : "Đang in hóa đơn (Trạng thái: Chưa thanh toán).");

        navigate("S17", new { StayId = stayId, InvoiceId = createdInvoiceId, IsPaid = isPaid });
    }

    private void HandlePay()
    {
        if (currentStay == null) return;

        try
        {
            var basis = rdoByHour.Checked ? ChargeBasis.Hour : ChargeBasis.Day;
            var method = cboPayMethod.SelectedIndex == 1
                ? PaymentMethod.Simulated
                : PaymentMethod.Cash;
            createdInvoiceId = checkoutService.PayAndCheckOut(
                stayId: currentStay.StayId,
                checkOutAt: dtpCheckOut.Value,
                chargeBasis: basis,
                serviceCharge: serviceCharge,
                paymentMethod: method,
                note: txtNote.Text);
            currentStay = checkoutService.GetStay(stayId);
            currentRoom = checkoutService.GetRoom(currentStay.RoomId);
            btnPay.Enabled = false;
            UpdateStayInfoLabel();
            MessageBox.Show($"Đã xác nhận thanh toán. Phòng {currentRoom.RoomCode} chuyển sang trạng thái Dọn dẹp.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi");
        }
    }

    // Các hàm tải dữ liệu
    private void LoadCheckOutData()
    {
        currentStay = checkoutService.GetStay(stayId);
        currentRoom = checkoutService.GetRoom(currentStay.RoomId);
        customerName = checkoutService.GetCustomerName(currentStay.CustomerId);
        depositAmount = checkoutService.GetDepositAmount(currentStay.ReservationId);
        bool paid = currentStay.Status == StayStatus.Completed || checkoutService.IsPaid(stayId);
        btnPay.Enabled = !paid;
        UpdateStayInfoLabel();
    }

    private void LoadUsedServices()
    {
        serviceGrid.Rows.Clear();
        var items = checkoutService.GetUsedServices(stayId);
        foreach (var item in items)
        {
            string serviceName = checkoutService.GetServiceName(item.ServiceId);
            serviceGrid.Rows.Add(
                serviceName,
                item.Quantity,
                $"{item.UnitPrice:N0}đ",
                $"{item.Amount:N0}đ");
        }

        serviceCharge = checkoutService.CalculateServiceTotal(items);
        lblServiceTotal.Text = $"Tổng tiền dịch vụ: {serviceCharge:N0}đ";
    }

    private void RecalculateSummary()
    {
        if (currentStay == null) return;
        var basis = rdoByHour.Checked ? ChargeBasis.Hour : ChargeBasis.Day;
        var span = dtpCheckOut.Value - currentStay.ActualCheckIn;
        try
        {
            (_, _, roomCharge) = checkoutService.CalculateRoomCharge(currentStay, dtpCheckOut.Value, basis);
        }
        catch
        {
            roomCharge = 0m;
        }

        var (subTotal, depositDeducted, amountDue, refundDue) =
                checkoutService.CalculateTotals(roomCharge, serviceCharge, depositAmount);
            lblRoomTotal.Text = $"Tiền buồng: {roomCharge:N0} đ";
            string refundLine = refundDue > 0m ? $"\nHoàn cọc: {refundDue:N0}đ" : string.Empty;
            lblSummary.Text =
                $"Tiền buồng: {roomCharge:N0}đ\n" +
                $"Tiền dịch vụ: {serviceCharge:N0}đ\n" +
                $"Tạm tính: {subTotal:N0} \n" +
                $"Trừ cọc: {depositDeducted:N0}đ" +
                refundLine;
            lblAmountDue.Text = $"THỰC THU: {amountDue:N0}đ";
    }

    private void UpdateStayInfoLabel(TimeSpan? span = null)
    {
        if (currentStay == null || currentRoom == null) return;
        var elapsed = span ?? (dtpCheckOut.Value - currentStay.ActualCheckIn);
        double hours = Math.Max(0, elapsed.TotalHours);
        string statusText = (currentStay.Status == StayStatus.Completed || checkoutService.IsPaid(stayId))
            ? " | Đã thanh toán"
            : string.Empty;
        lblStayInfo.Text =
            $"Phòng: {currentRoom.RoomCode} | Khách: {customerName} | Cọc: {depositAmount:N0}đ{statusText}\n" +
            $"Check-in: {currentStay.ActualCheckIn:dd/MM/yyyy HH:mm} | Thời gian ở: {hours:0.#} giờ";
    }
}


