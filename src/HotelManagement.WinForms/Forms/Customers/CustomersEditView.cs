using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;
using HotelManagement.Core.Entities;

namespace HotelManagement.WinForms.Forms.Customers;

public sealed record CustomerFormContext(
    string ReturnTo,
    string? TransactionCode = null,
    string? RoomInfo = null);


public sealed class CustomerFormView : UserControl
{
    private static readonly Color Navy = Color.FromArgb(27, 58, 101);
    private static readonly Color Accent = Color.FromArgb(24, 102, 218);
    private static readonly Color Ok = Color.FromArgb(22, 140, 90);
    private static readonly Color Bad = Color.FromArgb(200, 40, 40);
    private static readonly Color Muted = Color.FromArgb(110, 122, 140);
    private static readonly Color TextDark = Color.FromArgb(35, 48, 65);
    private static readonly Regex CccdRegex = new(@"^[0-9]{12}$");
    private static readonly Regex PassportRegex = new(@"^[A-Z0-9]{6,9}$", RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^0\d{9}$");

    private static readonly PropertyInfo
        IdentityTypeProp = typeof(Customer).GetProperty(nameof(Customer.IdentityType))!;

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

    private readonly RadioButton _rdoPassport = new()
        { Text = "Hộ chiếu", AutoSize = true, Margin = new Padding(14, 3, 3, 3) };

    private readonly TextBox _txtIdentityNumber = new();
    private readonly ComboBox _cmbNationality = new();
    private readonly DateTimePicker _dtpIssueDate = new();
    private readonly TextBox _txtIssuePlace = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtAddress = new() { Multiline = true };
    private readonly TextBox _txtNote = new() { Multiline = true };
    private readonly CheckBox _chkVip = new();
    private readonly CheckBox _chkSyncBca = new();
    private readonly Label _lblRank = new();
    private readonly Label _lblIdentityHint = new();
    private readonly Label _lblCode = new();
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
        _isNew = customer is null;
        _customer = customer ?? new Customer();
        _suggestedCode = suggestedCode ?? "KH-(tự động)";
        _originalIdentity = _isNew ? null : _customer.IdentityNumber;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(241, 245, 252);
        Padding = new Padding(10);
        MinimumSize = new Size(980, 600);

        _errors.ContainerControl = this;

        BuildLayout();
        ConfigureInputs();
        BindCustomerToForm();
        WireEvents();
        RefreshValidation();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = BackColor,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, _context is null ? 0 : 70));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildTopBar(), 0, 0);
        if (_context is not null)
            root.Controls.Add(BuildBanner(), 0, 1);
        root.Controls.Add(BuildContent(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildTopBar()
    {
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 6),
            Padding = new Padding(10, 4, 8, 4)
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var crumb = new Label
        {
            Text = "Update Customer",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 9, FontStyle.Bold),
            ForeColor = Navy
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = true,
            BackColor = Color.White
        };

        if (_context is not null)
        {
            var btnSource = CreateButton($"Quay lại {_context.ReturnTo} (Esc)", Accent, 250);
            btnSource.Click += (_, _) => _navigate(_context.ReturnTo, null);
            buttons.Controls.Add(btnSource);
        }

        var btnList = CreateButton("danh sách khách hàng (S07)", Color.FromArgb(94, 109, 128), 270);
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
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(37, 99, 235),
            Padding = new Padding(14, 6, 14, 6),
            Margin = new Padding(0, 0, 0, 6)
        };

        string tx = string.IsNullOrEmpty(ctx.TransactionCode) ? "" : $" - Mã GD: {ctx.TransactionCode}";
        string room = string.IsNullOrEmpty(ctx.RoomInfo) ? "" : $" - Phòng: {ctx.RoomInfo}";

        var line1 = Lbl($"Mở từ màn hình {ctx.ReturnTo}{tx}{room}", 22, 9f, true, Color.White);
        var line2 = Lbl($"Lưu xong sẽ quay lại màn hình {ctx.ReturnTo}.", 34, 8.5f, false, Color.White);

        StackTop(banner, line1, line2);
        return banner;
    }

    private Control BuildContent()
    {
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor,
            Margin = Padding.Empty,
            Padding = Padding.Empty
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
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18, 12, 18, 12),
            Margin = new Padding(0, 0, 8, 0),
            AutoScroll = true
        };
        ề
        var header = new Panel { Height = 66 };
        var right = new Label
        {
            Text = $"MÃ \n{(_isNew ? _suggestedCode : _customer.CustomerId.ToString())}",
            Dock = DockStyle.Right,
            Width = 200,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Arial", 9, FontStyle.Bold),
            ForeColor = Navy
        };
        var titleBox = new Panel { Dock = DockStyle.Fill };
        var title = Lbl(
            _isNew
                ? "HỒ SƠ KHÁCH HÀNG: THÊM MỚI KHÁCH HÀNG "
                : "HỒ SƠ KHÁCH HÀNG: CẬP NHẬT KHÁCH HÀNG ]",
            28, 11f, true, Navy);
        var subtitle = Lbl("Nhập chính xác theo quy định",
            22, 8.5f, false, Muted);
        StackTop(titleBox, title, subtitle);
        header.Controls.Add(titleBox);
        header.Controls.Add(right);

        var g1 = NewGrid(3, 62, 62, 62);
        Place(g1, Field("Họ và tên khách hàng (*)", _txtFullName,
            Hint("chuẩn hóa")), 0, 0, 3);

        var pnlType = new FlowLayoutPanel { Height = 28, WrapContents = false, BackColor = Color.White };
        pnlType.Controls.Add(_rdoCccd);
        pnlType.Controls.Add(_rdoPassport);
        Place(g1, Field("Loại giấy tờ (*)", pnlType), 0, 1, 1);

        Place(g1, Field("Số định danh (*)", _txtIdentityNumber, _lblIdentityHint), 1, 1, 2);
        Place(g1, Field("Quốc tịch", _cmbNationality), 0, 2, 1);
        Place(g1, Field("Ngày cấp", _dtpIssueDate), 1, 2, 1);
        Place(g1, Field("Nơi cấp", _txtIssuePlace), 2, 2, 1);
        var s1 = Section("1", "THÔNG TIN ĐỊNH DANH PHÁP LÝ (BẮT BUỘC)", 236, g1);

        var g2 = NewGrid(2, 62, 92);
        Place(g2, Field("Số điện thoại di động (*)", _txtPhone, Hint("Chuẩn 10 chữ số (VN)")), 0, 0, 1);
        Place(g2, Field("Địa chỉ Email", _txtEmail, Hint("Tùy chọn ")), 1, 0, 1);
        Place(g2, Field("Địa chỉ thường trú / Nơi ở hiện tại", _txtAddress), 0, 1, 2);
        var s2 = Section("2", "THÔNG TIN LIÊN LẠC & CƯ TRÚ HIỆN TẠI", 200, g2);

        var g3 = NewGrid(3, 96, 36);
        Place(g3, Field("Hạng khách", _lblRank), 0, 0, 1);
        Place(g3, Field("Ghi chú phục vụ)", _txtNote,
            Hint("Đặc quyền, dị ứng, phòng tầng cao")), 1, 0, 2);

        var pnlChecks = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(0, 4, 0, 0)
        };
        _chkVip.Text = "Lưu vào danh bạ khách thường xuyên ";
        _chkSyncBca.Text = "Đồng bộ thông tin đăng ký tạm trú ";
        _chkVip.AutoSize = true;
        _chkSyncBca.AutoSize = true;
        _chkVip.Margin = new Padding(0, 3, 24, 3);
        pnlChecks.Controls.Add(_chkVip);
        pnlChecks.Controls.Add(_chkSyncBca);
        Place(g3, pnlChecks, 0, 1, 3);
        var s3 = Section("3", "GHI CHÚ ĐẶC THÙ & SỞ THÍCH KHÁCH HÀNG", 180, g3);

        var actions = new FlowLayoutPanel
        {
            Height = 56,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(0, 10, 0, 0)
        };
        var btnReset = CreateButton("Làm lại / Reset (F5)", Color.FromArgb(94, 109, 128), 150);
        btnReset.Click += (_, _) => ResetForm();
        var btnCancel = CreateButton("Hủy bỏ (Esc)", Color.FromArgb(190, 60, 60), 120);
        btnCancel.Click += (_, _) => Cancel();
        actions.Controls.Add(btnReset);
        actions.Controls.Add(btnCancel);

        if (_context is not null)
        {
            var btnSaveOnly = CreateButton("Chỉ lưu hồ sơ", Color.FromArgb(20, 126, 110), 130);
            btnSaveOnly.Click += (_, _) => Save(selectForReturn: false);
            actions.Controls.Add(btnSaveOnly);

            var btnSaveSelect = CreateButton(
                $"Lưu khách hàng & Chọn cho {_context.ReturnTo} (F10)", Accent, 290);
            btnSaveSelect.Click += (_, _) => Save(selectForReturn: true);
            actions.Controls.Add(btnSaveSelect);
        }
        else
        {
            var btnSave = CreateButton("Lưu khách hàng (F10)", Accent, 190);
            btnSave.Click += (_, _) => Save(selectForReturn: false);
            actions.Controls.Add(btnSave);
        }

        StackTop(card, header, s1, s2, s3, actions);
        card.AutoScrollMinSize = new Size(640, 66 + 236 + 200 + 180 + 56 + 30);
        return card;
    }

    private Control BuildSidePanel()
    {
        var side = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            AutoScroll = true,
            Margin = Padding.Empty
        };


        var titleRow = new Panel { Height = 26 };
        _lblPass.Dock = DockStyle.Right;
        _lblPass.Width = 80;
        _lblPass.TextAlign = ContentAlignment.MiddleRight;
        _lblPass.Font = new Font("Arial", 9, FontStyle.Bold);
        var cardTitle = new Label
        {
            Text = "Kiểm tra hợp lệ ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 9.5f, FontStyle.Bold),
            ForeColor = Navy
        };
        titleRow.Controls.Add(cardTitle);
        titleRow.Controls.Add(_lblPass);

        var desc = Lbl("check",
            38, 8f, false, Muted);

        var rows = new List<Control> { titleRow, desc };
        for (int i = 0; i < _checks.Length; i++)
        {
            _checks[i] = Lbl("", 24, 9f, false, Muted);
            _checks[i].Font = new Font("Segoe UI", 9f);
            rows.Add(_checks[i]);
        }

        rows.Add(Lbl("Phím tắt", 36, 8f, false, Muted));
        var cardValidation = MakeCard(290, rows.ToArray());
        StackTop(side, cardValidation);
        side.AutoScrollMinSize = new Size(300, 290 + 10);
        return side;
    }


    private void ConfigureInputs()
    {
        _txtIdentityNumber.CharacterCasing = CharacterCasing.Upper;

        _cmbNationality.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbNationality.Items.AddRange(new object[]
        {
            "Việt Nam (VN)", "Hoa Kỳ (US)", "Nhật Bản (JP)", "Hàn Quốc (KR)",
            "Trung Quốc (CN)", "Pháp (FR)", "Anh (GB)", "Khác"
        });
        _cmbNationality.SelectedIndex = 0;

        _dtpIssueDate.Format = DateTimePickerFormat.Short;
        _dtpIssueDate.MaxDate = DateTime.Today;
        _dtpIssueDate.Value = DateTime.Today;

        _lblRank.AutoSize = false;
        _lblRank.Height = 32;
        _lblRank.BackColor = Color.FromArgb(25, 35, 50);
        _lblRank.ForeColor = Color.White;
        _lblRank.TextAlign = ContentAlignment.MiddleCenter;
        _lblRank.Font = new Font("Arial", 8.5f, FontStyle.Bold);

        _lblIdentityHint.Font = new Font("Arial", 7.5f, FontStyle.Bold);

        _lblCode.Text = _isNew ? _suggestedCode : _customer.CustomerId.ToString();
    }

    private void BindCustomerToForm()
    {
        _errors.Clear();

        _txtFullName.Text = _customer.FullName ?? string.Empty;

        bool passport = !_isNew && IsPassportValue(IdentityTypeProp.GetValue(_customer));
        _rdoPassport.Checked = passport;
        _rdoCccd.Checked = !passport;

        _txtIdentityNumber.Text = _customer.IdentityNumber ?? string.Empty;
        _txtPhone.Text = _customer.Phone ?? string.Empty;
        _txtEmail.Text = _customer.Email ?? string.Empty;
        _txtAddress.Text = _customer.Address ?? string.Empty;

        string? nationality = TryGet(_customer, "Nationality") as string;
        int idx = nationality is null ? -1 : _cmbNationality.FindStringExact(nationality);
        _cmbNationality.SelectedIndex = idx >= 0 ? idx : 0;

        if (TryGet(_customer, "IssueDate") is DateTime issue
            && issue >= _dtpIssueDate.MinDate && issue <= _dtpIssueDate.MaxDate)
            _dtpIssueDate.Value = issue;
        else
            _dtpIssueDate.Value = DateTime.Today;

        _txtIssuePlace.Text = TryGet(_customer, "IssuePlace") as string ?? string.Empty;
        _txtNote.Text = TryGet(_customer, "Note") as string ?? string.Empty;
        _chkVip.Checked = TryGet(_customer, "IsVip") is true;
        _chkSyncBca.Checked = _isNew;

        _lblRank.Text = _isNew ? "Khách hàng mới "
            : _chkVip.Checked ? "VIP" : "Khách hiện hữu";

        UpdateIdentityHintAndLimit();
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
        UpdateIdentityHintAndLimit();
        _errors.SetError(_txtIdentityNumber, string.Empty);
        RefreshValidation();
    }

    private void UpdateIdentityHintAndLimit()
    {
        _txtIdentityNumber.MaxLength = _rdoPassport.Checked ? 9 : 12;
    }


    private Regex IdentityPattern => _rdoPassport.Checked ? PassportRegex : CccdRegex;

    private bool IsDuplicate(string identity)
    {
        if (_isIdentityDuplicate is null) return false;
        if (string.Equals(identity, _originalIdentity, StringComparison.OrdinalIgnoreCase)) return false;
        return _isIdentityDuplicate(identity);
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
            passport ? "Loại & Số giấy tờ (Hộ chiếu 6–9 ký tự)" : "Loại & Số giấy tờ (CCCD 12 số)",
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
        if (id.Length == 0)
            Fail(_txtIdentityNumber, "Vui lòng nhập số định danh.");
        else if (!IdentityPattern.IsMatch(id))
            Fail(_txtIdentityNumber, _rdoPassport.Checked
                ? "Số hộ chiếu gồm 6–9 ký tự chữ hoặc số."
                : "CCCD phải gồm đúng 12 chữ số.");
        else if (IsDuplicate(id))
            Fail(_txtIdentityNumber, "Số định danh đã tồn tại ");

        string phone = _txtPhone.Text.Trim();
        if (phone.Length == 0)
            Fail(_txtPhone, "Vui lòng nhập số điện thoại.");
        else if (!PhoneRegex.IsMatch(phone))
            Fail(_txtPhone, "Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 0.");

        string email = _txtEmail.Text.Trim();
        if (email.Length > 0 && !IsValidEmail(email))
            Fail(_txtEmail, "Email không hợp lệ.");

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
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Không thể lưu khách hàng.\n{ex.Message}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (selectForReturn && _context is not null)
            _navigate(_context.ReturnTo, _customer);
        else
            _navigate("S07", null);
    }

    private void ApplyFormToCustomer()
    {
        _customer.FullName = _txtFullName.Text.Trim();
        IdentityTypeProp.SetValue(_customer, ResolveIdentityType(_rdoPassport.Checked));
        _customer.IdentityNumber = _txtIdentityNumber.Text.Trim().ToUpperInvariant();
        _customer.Phone = _txtPhone.Text.Trim();
        _customer.Email = _txtEmail.Text.Trim();
        _customer.Address = _txtAddress.Text.Trim();
        _customer.IsActive = true;

        TrySet(_customer, "Nationality", _cmbNationality.Text);
        TrySet(_customer, "IssueDate", _dtpIssueDate.Value.Date);
        TrySet(_customer, "IssuePlace", _txtIssuePlace.Text.Trim());
        TrySet(_customer, "Note", _txtNote.Text.Trim());
        TrySet(_customer, "IsVip", _chkVip.Checked);
    }

    private void ResetForm()
    {
        BindCustomerToForm();
        RefreshValidation();
    }

    private void Cancel()
    {
        _navigate(_context?.ReturnTo ?? "S07", null);
    }

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

}

