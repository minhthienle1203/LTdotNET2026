using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Enums;
using HotelManagement.Core.Exceptions;

namespace HotelManagement.WinForms.Forms.Customers;

public sealed record CustomerFormContext(
    string ReturnTo,
    string? TransactionCode = null,
    string? RoomInfo = null);
public sealed class CustomerFormView : UserControl
{
    private static readonly Color Navy = Color.FromArgb(27, 58, 101);
    private static readonly Color Accent = Color.FromArgb(150, 150, 150);
    private static readonly Color ButtonBack = Color.FromArgb(225, 225, 225);
    private static readonly Color ButtonText = Color.FromArgb(70, 70, 70);
    private static readonly Color Ok = Color.FromArgb(22, 140, 90);
    private static readonly Color Bad = Color.FromArgb(200, 40, 40);
    private static readonly Color Muted = Color.FromArgb(110, 122, 140);
    private static readonly Regex CccdRegex = new(@"^[0-9]{12}$");
    private static readonly Regex PassportRegex = new(@"^[A-Z0-9]{6,9}$", RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^0\d{9}$");

    private readonly Action<string, object?> _navigate;
    private readonly Action<Customer, bool>? _saveCustomer;
    private readonly Func<string, bool>? _isIdentityDuplicate;
    private readonly CustomerFormContext? _context;
    private readonly Customer _customer;
    private readonly bool _isNew;
    private readonly string _suggestedCode;
    private readonly string? _originalIdentity;

    private readonly TextBox _txtFullName = new();
    private readonly RadioButton _rdoCccd = new() { Text = "CCCD (12 số)", AutoSize = true, Checked = true };
    private readonly RadioButton _rdoPassport = new() { Text = "Hộ chiếu", AutoSize = true, Margin = new Padding(14, 3, 3, 3) };
    private readonly TextBox _txtIdentityNumber = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtAddress = new() { Multiline = true };
    private readonly Label _lblIdentityHint = new();
    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly Label[] _checks = new Label[5];
    private readonly Label _lblPass = new();

    public CustomerFormView(
        Action<string, object?> navigate,
        Customer? customer = null,
        CustomerFormContext? context = null,
        Action<Customer, bool>? saveCustomer = null,
        Func<string, bool>? isIdentityDuplicate = null,
        string? suggestedCode = null)
    {
        _navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        _saveCustomer = saveCustomer;
        _isIdentityDuplicate = isIdentityDuplicate;
        _context = context;
        _isNew = customer is null || customer.CustomerId == 0;
        _customer = customer ?? new Customer();
        _suggestedCode = suggestedCode ?? "KH-(tự động)";
        _originalIdentity = _isNew ? null : _customer.IdentityNumber;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(241, 245, 252);
        Padding = new Padding(10);
        MinimumSize = new Size(980, 560);
        _errors.ContainerControl = this;

        BuildLayout();
        _txtIdentityNumber.CharacterCasing = CharacterCasing.Upper;
        _txtPhone.MaxLength = 10;
        BindCustomerToForm();
        WireEvents();
        RefreshValidation();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = BackColor,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, _context is null ? 0 : 70));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildTopBar(), 0, 0);
        if (_context is not null) root.Controls.Add(BuildBanner(), 0, 1);
        root.Controls.Add(BuildContent(), 0, 2);
        Controls.Add(root);
    }

    private Control BuildTopBar()
    {
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 6), Padding = new Padding(10, 4, 8, 4)
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var crumb = new Label
        {
            Text = _isNew ? "S08 - Thêm khách hàng" : "S08 - Cập nhật khách hàng",
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 9, FontStyle.Bold), ForeColor = Navy
        };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, AutoSize = true, BackColor = Color.White
        };
        if (_context is not null)
        {
            var btnSource = CreateButton($"Quay lại {_context.ReturnTo} (Esc)", 250);
            btnSource.Click += (_, _) => _navigate(_context.ReturnTo, null);
            buttons.Controls.Add(btnSource);
        }
        var btnList = CreateButton("Danh sách khách hàng (S07)", 220);
        btnList.Click += (_, _) => _navigate("S07", null);
        buttons.Controls.Add(btnList);

        bar.Controls.Add(crumb, 0, 0);
        bar.Controls.Add(buttons, 1, 0);
        return bar;
    }

    private Control BuildBanner()
    {
        var ctx = _context!;
        var banner = new Panel
        {
            Dock = DockStyle.Fill, BackColor = Accent,
            Padding = new Padding(14, 6, 14, 6), Margin = new Padding(0, 0, 0, 6)
        };
        string tx = string.IsNullOrEmpty(ctx.TransactionCode) ? "" : $" - Mã GD: {ctx.TransactionCode}";
        string room = string.IsNullOrEmpty(ctx.RoomInfo) ? "" : $" - Phòng: {ctx.RoomInfo}";
        StackTop(banner,
            Lbl($"Mở từ màn hình {ctx.ReturnTo}{tx}{room}", 22, 9f, true, Color.White),
            Lbl($"Lưu xong sẽ quay lại màn hình {ctx.ReturnTo}.", 34, 8.5f, false, Color.White));
        return banner;
    }

    private Control BuildContent()
    {
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor,
            Margin = Padding.Empty, Padding = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        content.Controls.Add(BuildFormCard(), 0, 0);
        content.Controls.Add(BuildSidePanel(), 1, 0);
        return content;
    }

    private Control BuildFormCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18, 12, 18, 12),
            Margin = new Padding(0, 0, 8, 0), AutoScroll = true
        };

        var header = new Panel { Height = 66 };
        var code = new Label
        {
            Text = $"MÃ\n{(_isNew ? _suggestedCode : $"KH-{_customer.CustomerId:000}")}",
            Dock = DockStyle.Right, Width = 200, TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Arial", 9, FontStyle.Bold), ForeColor = Navy
        };
        var titleBox = new Panel { Dock = DockStyle.Fill };
        StackTop(titleBox,
            Lbl(_isNew ? "HỒ SƠ KHÁCH HÀNG: THÊM MỚI" : "HỒ SƠ KHÁCH HÀNG: CẬP NHẬT", 28, 11f, true, Navy),
            Lbl("Nhập chính xác theo giấy tờ tùy thân", 22, 8.5f, false, Muted));
        header.Controls.Add(titleBox);
        header.Controls.Add(code);

        var g1 = NewGrid(3, 62, 62);
        Place(g1, Field("Họ và tên khách hàng (*)", _txtFullName, Hint("Tự động chuẩn hóa chữ hoa")), 0, 0, 3);
        var pnlType = new FlowLayoutPanel { Height = 28, WrapContents = false, BackColor = Color.White };
        pnlType.Controls.Add(_rdoCccd);
        pnlType.Controls.Add(_rdoPassport);
        Place(g1, Field("Loại giấy tờ (*)", pnlType), 0, 1, 1);
        Place(g1, Field("Số định danh (*)", _txtIdentityNumber, _lblIdentityHint), 1, 1, 2);
        var s1 = Section("1", "THÔNG TIN ĐỊNH DANH PHÁP LÝ (BẮT BUỘC)", 190, g1);

        var g2 = NewGrid(2, 62, 92);
        Place(g2, Field("Số điện thoại di động (*)", _txtPhone, Hint("10 chữ số, bắt đầu bằng 0")), 0, 0, 1);
        Place(g2, Field("Địa chỉ Email", _txtEmail, Hint("Tùy chọn")), 1, 0, 1);
        Place(g2, Field("Địa chỉ thường trú / nơi ở hiện tại", _txtAddress), 0, 1, 2);
        var s2 = Section("2", "THÔNG TIN LIÊN LẠC & CƯ TRÚ", 200, g2);

        var actions = new FlowLayoutPanel
        {
            Height = 56, FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
            BackColor = Color.White, Padding = new Padding(0, 10, 0, 0)
        };
        var btnReset = CreateButton("Làm lại (F5)", 120);
        btnReset.Click += (_, _) => ResetForm();
        var btnCancel = CreateButton("Hủy bỏ (Esc)", 120);
        btnCancel.Click += (_, _) => Cancel();
        actions.Controls.Add(btnReset);
        actions.Controls.Add(btnCancel);

        if (_context is not null)
        {
            var btnSaveOnly = CreateButton("Chỉ lưu hồ sơ", 130);
            btnSaveOnly.Click += (_, _) => Save(selectForReturn: false);
            var btnSaveSelect = CreateButton($"Lưu & chọn cho {_context.ReturnTo} (F10)", 250, primary: true);
            btnSaveSelect.Click += (_, _) => Save(selectForReturn: true);
            actions.Controls.Add(btnSaveOnly);
            actions.Controls.Add(btnSaveSelect);
        }
        else
        {
            var btnSave = CreateButton("Lưu khách hàng (F10)", 190, primary: true);
            btnSave.Click += (_, _) => Save(selectForReturn: false);
            actions.Controls.Add(btnSave);
        }

        StackTop(card, header, s1, s2, actions);
        card.AutoScrollMinSize = new Size(640, 66 + 190 + 200 + 56 + 30);
        return card;
    }

    private Control BuildSidePanel()
    {
        var side = new Panel { Dock = DockStyle.Fill, BackColor = BackColor, AutoScroll = true, Margin = Padding.Empty };

        var titleRow = new Panel { Height = 26 };
        _lblPass.Dock = DockStyle.Right;
        _lblPass.Width = 80;
        _lblPass.TextAlign = ContentAlignment.MiddleRight;
        _lblPass.Font = new Font("Arial", 9, FontStyle.Bold);
        titleRow.Controls.Add(new Label
        {
            Text = "Kiểm tra hợp lệ", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 9.5f, FontStyle.Bold), ForeColor = Navy
        });
        titleRow.Controls.Add(_lblPass);

        var rows = new List<Control> { titleRow, Lbl("Hồ sơ cần đạt đủ 5/5 mới lưu được.", 30, 8f, false, Muted) };
        for (int i = 0; i < _checks.Length; i++)
        {
            _checks[i] = Lbl("", 24, 9f, false, Muted);
            _checks[i].Font = new Font("Segoe UI", 9f);
            rows.Add(_checks[i]);
        }
        rows.Add(Lbl("Phím tắt: F10 = Lưu, F5 = Làm lại, Esc = Hủy", 36, 8f, false, Muted));

        StackTop(side, MakeCard(290, rows.ToArray()));
        side.AutoScrollMinSize = new Size(300, 300);
        return side;
    }

    private void BindCustomerToForm()
    {
        _errors.Clear();
        _txtFullName.Text = _customer.FullName ?? string.Empty;

        bool passport = !_isNew && _customer.IdentityType == IdentityType.Passport;
        _rdoPassport.Checked = passport;
        _rdoCccd.Checked = !passport;

        _txtIdentityNumber.Text = _customer.IdentityNumber ?? string.Empty;
        _txtPhone.Text = _customer.Phone ?? string.Empty;
        _txtEmail.Text = _customer.Email ?? string.Empty;
        _txtAddress.Text = _customer.Address ?? string.Empty;
        UpdateIdentityLimit();
    }

    private void ApplyFormToCustomer()
    {
        _customer.FullName = _txtFullName.Text.Trim();
        _customer.IdentityType = _rdoPassport.Checked ? IdentityType.Passport : IdentityType.CCCD;
        _customer.IdentityNumber = _txtIdentityNumber.Text.Trim().ToUpperInvariant();
        _customer.Phone = _txtPhone.Text.Trim();
        _customer.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();
        _customer.Address = string.IsNullOrWhiteSpace(_txtAddress.Text) ? null : _txtAddress.Text.Trim();
        _customer.IsActive = true;
    }

    private void WireEvents()
    {
        _txtFullName.TextChanged += (_, _) => OnInputChanged(_txtFullName);
        _txtFullName.Leave += (_, _) => _txtFullName.Text = NormalizeName(_txtFullName.Text);
        _txtIdentityNumber.TextChanged += (_, _) => OnInputChanged(_txtIdentityNumber);
        _txtPhone.TextChanged += (_, _) => OnInputChanged(_txtPhone);
        _txtEmail.TextChanged += (_, _) => OnInputChanged(_txtEmail);
        _rdoCccd.CheckedChanged += (_, _) => OnIdentityTypeChanged();
        _rdoPassport.CheckedChanged += (_, _) => OnIdentityTypeChanged();
    }

    private void OnInputChanged(Control source)
    {
        _errors.SetError(source, string.Empty);
        RefreshValidation();
    }

    private void OnIdentityTypeChanged()
    {
        UpdateIdentityLimit();
        _errors.SetError(_txtIdentityNumber, string.Empty);
        RefreshValidation();
    }

    private void UpdateIdentityLimit() => _txtIdentityNumber.MaxLength = _rdoPassport.Checked ? 9 : 12;

    private Regex IdentityPattern => _rdoPassport.Checked ? PassportRegex : CccdRegex;

    private bool IsDuplicate(string identity)
    {
        if (_isIdentityDuplicate is null) return false;
        if (string.Equals(identity, _originalIdentity, StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            return _isIdentityDuplicate(identity);
        }
        catch
        {
            return false;
        }
    }

    private int[] Evaluate()
    {
        var s = new int[5];
        string id = _txtIdentityNumber.Text.Trim();
        string phone = _txtPhone.Text.Trim();
        string email = _txtEmail.Text.Trim();

        s[0] = string.IsNullOrWhiteSpace(_txtFullName.Text) ? 0 : 1;
        s[1] = id.Length == 0 ? 0 : (IdentityPattern.IsMatch(id) ? 1 : 2);
        s[2] = phone.Length == 0 ? 0 : (PhoneRegex.IsMatch(phone) ? 1 : 2);
        s[3] = email.Length == 0 ? 1 : (IsValidEmail(email) ? 1 : 2);
        s[4] = s[1] != 1 ? 0 : (IsDuplicate(id) ? 2 : 1);
        return s;
    }

    private void RefreshValidation()
    {
        int[] s = Evaluate();
        bool passport = _rdoPassport.Checked;
        string[] captions =
        {
            "Họ và tên khách hàng",
            passport ? "Số giấy tờ (Hộ chiếu 6–9 ký tự)" : "Số giấy tờ (CCCD 12 số)",
            "Số điện thoại di động (10 số)",
            _txtEmail.Text.Trim().Length == 0 ? "Địa chỉ Email (tùy chọn)" : "Địa chỉ Email",
            "Không trùng số định danh trong hệ thống"
        };

        int pass = 0;
        for (int i = 0; i < _checks.Length; i++)
        {
            string icon = s[i] == 1 ? "✔" : s[i] == 2 ? "✖" : "○";
            _checks[i].Text = $"{icon}   {captions[i]}";
            _checks[i].ForeColor = s[i] == 1 ? Ok : s[i] == 2 ? Bad : Muted;
            if (s[i] == 1) pass++;
        }
        _lblPass.Text = $"{pass}/5 PASS";
        _lblPass.ForeColor = pass == 5 ? Ok : Muted;

        string regexText = passport ? "^[A-Z0-9]{6,9}$" : "^[0-9]{12}$";
        string state = s[1] == 1 ? "   Hợp lệ" : s[1] == 2 ? "   Chưa hợp lệ" : "";
        _lblIdentityHint.Text = $"Regex: {regexText}{state}";
        _lblIdentityHint.ForeColor = s[1] == 1 ? Ok : s[1] == 2 ? Bad : Muted;
    }

    private bool ValidateForSave()
    {
        _errors.Clear();
        Control? first = null;
        void Fail(Control c, string message)
        {
            _errors.SetError(c, message);
            first ??= c;
        }

        if (string.IsNullOrWhiteSpace(_txtFullName.Text))
            Fail(_txtFullName, "Vui lòng nhập họ và tên khách hàng.");

        string id = _txtIdentityNumber.Text.Trim();
        if (id.Length == 0) Fail(_txtIdentityNumber, "Vui lòng nhập số định danh.");
        else if (!IdentityPattern.IsMatch(id))
            Fail(_txtIdentityNumber, _rdoPassport.Checked
                ? "Số hộ chiếu gồm 6–9 ký tự chữ hoặc số."
                : "CCCD phải gồm đúng 12 chữ số.");
        else if (IsDuplicate(id)) Fail(_txtIdentityNumber, "Số định danh đã tồn tại.");

        string phone = _txtPhone.Text.Trim();
        if (phone.Length == 0) Fail(_txtPhone, "Vui lòng nhập số điện thoại.");
        else if (!PhoneRegex.IsMatch(phone)) Fail(_txtPhone, "Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 0.");

        string email = _txtEmail.Text.Trim();
        if (email.Length > 0 && !IsValidEmail(email)) Fail(_txtEmail, "Email không hợp lệ.");

        first?.Focus();
        return first is null;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private void Save(bool selectForReturn)
    {
        if (!ValidateForSave())
        {
            RefreshValidation();
            return;
        }

        _txtFullName.Text = NormalizeName(_txtFullName.Text);
        ApplyFormToCustomer();

        try
        {
            _saveCustomer?.Invoke(_customer, _isNew);
        }
        catch (BusinessRuleException ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể lưu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Không thể lưu khách hàng.\n{ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (selectForReturn && _context is not null) _navigate(_context.ReturnTo, _customer);
        else _navigate("S07", null);
    }

    private void ResetForm()
    {
        BindCustomerToForm();
        RefreshValidation();
    }

    private void Cancel() => _navigate(_context?.ReturnTo ?? "S07", null);

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.F10:
                Save(selectForReturn: _context is not null);
                return true;
            case Keys.F5:
                ResetForm();
                return true;
            case Keys.Escape:
                Cancel();
                return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private static string NormalizeName(string value) =>
        Regex.Replace(value.Trim(), @"\s+", " ").ToUpper(new CultureInfo("vi-VN"));

    private static void StackTop(Control parent, params Control[] topToBottom)
    {
        for (int i = topToBottom.Length - 1; i >= 0; i--)
        {
            topToBottom[i].Dock = DockStyle.Top;
            parent.Controls.Add(topToBottom[i]);
        }
    }

    private static Label Lbl(string text, int height, float size, bool bold, Color color) => new()
    {
        Text = text, AutoSize = false, Height = height,
        Font = new Font("Arial", size, bold ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = color, TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label Hint(string text) => new() { Text = text };

    private static Panel MakeCard(int height, params Control[] topToBottom)
    {
        var outer = new Panel { Height = height, Padding = new Padding(0, 0, 0, 10) };
        var inner = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(14, 10, 14, 10) };
        StackTop(inner, topToBottom);
        outer.Controls.Add(inner);
        return outer;
    }

    private static Panel Section(string number, string title, int height, Control body)
    {
        var outer = new Panel { Height = height, Padding = new Padding(0, 0, 0, 14) };
        var head = new Label
        {
            Text = $"{number}. {title}", Dock = DockStyle.Top, Height = 30,
            TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Navy
        };
        body.Dock = DockStyle.Fill;
        outer.Controls.Add(body);
        outer.Controls.Add(head);
        return outer;
    }

    private static TableLayoutPanel NewGrid(int columns, params int[] rowHeights)
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rowHeights.Length, BackColor = Color.White
        };
        for (int c = 0; c < columns; c++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        foreach (int h in rowHeights)
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
        return grid;
    }

    private static void Place(TableLayoutPanel grid, Control control, int column, int row, int columnSpan)
    {
        control.Dock = DockStyle.Fill;
        grid.Controls.Add(control, column, row);
        if (columnSpan > 1) grid.SetColumnSpan(control, columnSpan);
    }

    private static Panel Field(string caption, Control input, Label? hint = null)
    {
        var box = new Panel { Margin = new Padding(0, 0, 12, 0) };
        bool multiline = input is TextBox { Multiline: true };
        input.Dock = multiline ? DockStyle.Fill : DockStyle.Top;
        if (input is TextBoxBase or ComboBox or DateTimePicker)
            input.Font = new Font("Arial", 9.5f);

        var head = new Panel { Dock = DockStyle.Top, Height = 22 };
        var cap = new Label
        {
            Text = caption, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 8.5f, FontStyle.Bold), ForeColor = Color.FromArgb(50, 65, 85)
        };
        hint ??= new Label();
        hint.AutoSize = false;
        hint.Dock = DockStyle.Fill;
        hint.TextAlign = ContentAlignment.MiddleRight;
        if (hint.ForeColor == SystemColors.ControlText) hint.ForeColor = Muted;
        hint.Font = new Font("Arial", 7.5f);

        head.Controls.Add(hint);
        head.Controls.Add(cap);
        box.Controls.Add(input);
        box.Controls.Add(head);
        return box;
    }
    private static Button CreateButton(string text, int width, bool primary = false)
    {
        var back = primary ? Accent : ButtonBack;
        var button = new Button
        {
            Text = text, Width = width, Height = 32,
            BackColor = back, ForeColor = primary ? Color.White : ButtonText,
            FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 8, FontStyle.Bold), Cursor = Cursors.Hand,
            Margin = new Padding(3), UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Accent;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.3f);
        return button;
    }
}
