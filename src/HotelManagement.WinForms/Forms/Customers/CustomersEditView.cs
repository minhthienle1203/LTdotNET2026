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


public sealed class CustomerFormView : UserControl {
    private static readonly Color Navy = Color.FromArgb(27, 58, 101);
    private static readonly Color Accent = Color.FromArgb(24, 102, 218);
    private static readonly Color Ok = Color.FromArgb(22, 140, 90);
    private static readonly Color Bad = Color.FromArgb(200, 40, 40);
    private static readonly Color Muted = Color.FromArgb(110, 122, 140);
    private static readonly Color TextDark = Color.FromArgb(35, 48, 65);
    private static readonly Regex CccdRegex = new(@"^[0-9]{12}$");
    private static readonly Regex PassportRegex = new(@"^[A-Z0-9]{6,9}$", RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new(@"^0\d{9}$");
    private static readonly PropertyInfo IdentityTypeProp = typeof(Customer).GetProperty(nameof(Customer.IdentityType))!;
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


