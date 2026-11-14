using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using HotelManagement.Core.Entities;
using Microsoft.CSharp.RuntimeBinder;

namespace HotelManagement.WinForms.Forms.Customers;

public sealed class CustomersListView : UserControl
{
    private readonly Action<String,object?> _navigate;
    private readonly Func<string, IEnumerable<Customer>> _searchCustomers;
    private readonly TextBox _txtKeywords= new ();
    private readonly DataGridView _Gird = new ();
    private readonly Label _lblCount = new ();
    private readonly BindingSource _bindingSource = new ();
    private readonly BindingList<Customer> _customers = new();
    private readonly Label _lblDetailName = new ();
    private readonly Label _lblDetailId = new ();
    private readonly Label _lblDetailIdentity = new ();
    private readonly Label _lblDetailPhone = new ();
    private readonly Label _lblDetailEmail = new ();
    private readonly Label _lblDetailAddress = new ();
    private readonly Label _lblTotal= new ();
    private readonly Label _lblDomestic = new();
    private readonly Label _lblInternational = new();
    private readonly Label _lblActive = new ();

    public CustomersListView(Action<String, object?> navigate,
        Func<string, IEnumerable<Customer>> searchCustomers = null)
    {
        _navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        _searchCustomers = searchCustomers;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(241, 245, 252);
        Padding = new Padding(10);
        MinimumSize = new Size(900, 560);
        BuildLayout();
        configureGird();
        BindData();
        Load += (_, _) => ReloadCustomers();

    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = BackColor,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildHeader(),0, 0);
        root.Controls.Add(BuildStatistics(), 0, 1);
        root.Controls.Add(BuildSearchBar(), 0, 2);
        root.Controls.Add(BuildMainContent(), 0, 3);
        Controls.Add((root));
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 6),
            Padding = new Padding(10, 3, 8, 3)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var titlePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(0, 5, 0, 0)
        };
        var section = new Label
        {
            Text = "S07-Danh sach Khach hagn",
            AutoSize = true,
            Font = new Font("Arial", 12,FontStyle.Bold),
            ForeColor = Color.FromArgb(27,58,101),
            Margin = new Padding(2, 5, 15, 0)
        };
        var subtitle = new Label
        {
            Text = "Customers list view",
            AutoSize = true,
            Font = new Font("Arial", 8.5f),
            ForeColor = Color.Gray,
            Margin = new Padding(0, 9, 0, 0)

        };
        titlePanel.Controls.Add(section);
        titlePanel.Controls.Add(subtitle);
        var button = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(0, 2, 0, 0)

        };
        var btnAdd = CreateButton("+Them khach hnag(F2)", Color.FromArgb(24, 102, 218));
        btnAdd.Width = 158;
        btnAdd.Click += (_, _) => _navigate("S08", null);

        var btnExport = CreateButton("xuat ex", Color.FromArgb(75, 91, 112));
        btnExport.Width = 100;
        btnExport.Click +=(_,_)=> MessageBox.Show(this,"chuc nang xuat ex","Xuat du lieu",MessageBoxButtons.OK,MessageBoxIcon.Information);
        button.Controls.Add(btnAdd);
        button.Controls.Add(btnExport);
        return header;

    }

    private Control BuildStatistics()
    {
        var cards = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = BackColor,
            Margin = new Padding(0, 0, 0, 6),
            Padding = Padding.Empty,
        };
        for (int i=0; i < 4;i++)  cards.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        cards.Controls.Add(CreateStatCard("Tong khach hang",_lblTotal,"Tat ca ho so trong quan ly"),0,0);
        cards.Controls.Add(CreateStatCard("Khach hang noi dia",_lblDomestic,"Giay to cccd"),1,0);
        cards.Controls.Add(CreateStatCard("Khach hang quoc te",_lblInternational,"Ho chieu/giay to"),2,0);
        cards.Controls.Add(CreateStatCard("Ho so dang quan ly",_lblActive,"Khach hang con hieu luc"),3,0);
        return cards;
    }

    private Control CreateStatCard(string title, Label valueLabel, string note)
    {
        var card = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 6, 0),
            Padding = new Padding(11, 7, 8, 5)
        };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 19,
            Font = new Font("Arial", 8, FontStyle.Bold),
            ForeColor = Color.FromArgb(90, 105, 125)
        };
        valueLabel.Text = "0";
        valueLabel.Dock = DockStyle.Top;
        valueLabel.Height = 31;
        valueLabel.Font = new Font("Arial", 8, FontStyle.Bold);
        valueLabel.ForeColor = Color.FromArgb(27, 77, 151);
        var noteLabel = new Label
        {
            Text = note,
            Dock = DockStyle.Fill,
            Font = new Font("Arial", 7.5f),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        card.Controls.Add(noteLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        return card;

    }

    private Control BuildSearchBar()
    {
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 6),
            Padding = new Padding(8, 4, 8, 4)
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        var label = new Label
        {
            Text = "Tim khach hang",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(50, 65, 85)
        };
        _txtKeywords.Dock = DockStyle.Fill;
        _txtKeywords.Margin = new Padding(3, 3, 8, 3);
        _txtKeywords.Font = new Font("Arial", 9);
        _txtKeywords.PlaceholderText = "Nhap ten, sdt,cccd ho chieu,....";
        _txtKeywords.TextChanged += (_, _) => ApplySearch();
        var btnSearch = CreateButton("tim kiem", Color.FromArgb(30, 91, 190));
        btnSearch.Click += (_, _) => ReloadCustomers();
        var btnReset = CreateButton("lam moi", Color.FromArgb(94, 109, 128));
        btnReset.Click += (_, _) =>
        {
            _txtKeywords.Clear();
            ReloadCustomers();

        };
        bar.Controls.Add(label,0,0);
        bar.Controls.Add(_txtKeywords,1,0);
        bar.Controls.Add(btnSearch,2,0);
        bar.Controls.Add(btnReset,3,0);
        return bar;

    }

    private Control BuildMainContent()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = BackColor,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        var listPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(7),
            Margin = new Padding(0, 0, 6, 0)
        };
        var listHeader = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 34,
            BackColor = Color.White

        };
        var listTitle = new Label
        {
            Text = "Ho so khach hang",
            Dock = DockStyle.Left,
            Width = 430,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Arial", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 56, 80)

        };
        _lblCount.Dock = DockStyle.Right;
        _lblCount.Width = 125;
        _lblCount.TextAlign = ContentAlignment.MiddleRight;
        _lblCount.Font = new Font("Arial", 8);
        _lblCount.ForeColor = Color.Gray;
        listHeader.Controls.Add(_lblCount);
        listHeader.Controls.Add(listTitle);

        _Gird.Dock = DockStyle.Fill;
       listPanel.Controls.Add(_Gird);
       listPanel.Controls.Add(listHeader);
       var deltailPanel = BuildDetailPanel();
       main.Controls.Add(listPanel,0,0);
       main.Controls.Add(deltailPanel,1,0);
       return main;

    }

    private Control BuildDetailPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10),
            Margin = Padding.Empty
        };
        var heading = new Label
        {
            Text = "Thong tin khach hang",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Arial", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 65, 105)


        };
        var info = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.White,
            Padding = new Padding(0, 5, 0, 0)
        };
        for (int i=0; i < 6; i++)info.RowStyles.Add(new RowStyle(SizeType.Percent, 100f/6f));
        AddDetailField(info, 0, "Ho va Ten",_lblDetailName,42);
        AddDetailField(info, 1, "Ma khach hang",_lblDetailId,42);
        AddDetailField(info, 2, "Giay to dinh danh",_lblDetailIdentity,52);
        AddDetailField(info, 3, "Sdt",_lblDetailPhone,42);
        AddDetailField(info, 4, "Email",_lblDetailEmail,42);
        AddDetailField(info, 5, "Dia chi",_lblDetailAddress,58);
        var action = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 43,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(0, 5, 0, 0)
        };
        var btnEdit = CreateButton("Sua ho so", Color.FromArgb(219, 145, 35));
        btnEdit.Width = 105;
        btnEdit.Click += (_, _) => EditSelectedCustomer();
        var btnReservation = CreateButton("Tao dat phong", Color.FromArgb(20, 126, 110));
        btnReservation.Width = 125;
        btnReservation.Click += (_, _) => OpenReservation();

        action.Controls.Add(btnEdit);
        action.Controls.Add(btnReservation);

        panel.Controls.Add(info);
        panel.Controls.Add(action);
        panel.Controls.Add(heading);
        return panel;

    }

    private static void AddDetailField(TableLayoutPanel parent, int row, string caption, Label value, int height)
    {
        var field = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(0, 2, 0, 2)

        };
        var title = new Label
        {
            Text = caption,
            Dock = DockStyle.Top,
            Height = 17,
            Font = new Font("Arial", 7.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(150, 177, 133)
        };
        value.Text = "_";
        value.Dock = DockStyle.Fill;
        value.Font = new Font("Arial", 8, FontStyle.Regular);
        value.ForeColor = Color.FromArgb(35, 48, 65);
        value.AutoEllipsis = true;

        field.Controls.Add(value);
        field.Controls.Add(title);
        parent.Controls.Add(field,0,row);
    }

    private void configureGird()
    {
        _Gird.AllowUserToAddRows=false;
        _Gird.AllowUserToDeleteRows=false;
        _Gird.AllowUserToResizeRows=false;
        _Gird.AutoGenerateColumns=false;
        _Gird.ReadOnly = true;
        _Gird.MultiSelect=false;
        _Gird.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
        _Gird.RowHeadersVisible=false;
        _Gird.BackgroundColor = Color.White;
        _Gird.BorderStyle=BorderStyle.None;
        _Gird.GridColor = Color.FromArgb(225, 232, 241);
        _Gird.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.AllCells;
        _Gird.ColumnHeadersHeight = 38;
        _Gird.RowTemplate.Height = 38;
        _Gird.EnableHeadersVisualStyles = false;
        _Gird.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(232, 239, 240);
        _Gird.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(35, 48, 65);
        _Gird.ColumnHeadersDefaultCellStyle.Font=new Font("Arial", 8, FontStyle.Bold);
        _Gird.DefaultCellStyle.Font=new Font("Arial", 8f);
        _Gird.DefaultCellStyle.ForeColor = Color.FromArgb(35, 48, 65);
        _Gird.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 230, 252);
        _Gird.DefaultCellStyle.SelectionForeColor = Color.FromArgb(25, 40, 60);
        _Gird.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247,249,253);
        AddGridColumn("CustomerId", "MÃ KH", nameof(Customer.CustomerId), 75);
        AddGridColumn("FullName", "HỌ VÀ TÊN", nameof(Customer.FullName), 135);
        AddGridColumn("IdentityType", "LOẠI GIẤY TỜ", nameof(Customer.IdentityType), 85);
        AddGridColumn("IdentityNumber", "SỐ ĐỊNH DANH", nameof(Customer.IdentityNumber), 115);
        AddGridColumn("Phone", "SỐ ĐIỆN THOẠI", nameof(Customer.Phone), 105);
        AddGridColumn("Address", "ĐỊA CHỈ", nameof(Customer.Address), 125);

        _Gird.SelectionChanged += (_, _) => UpdateCustomerDetails();
        _Gird.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                EditSelectedCustomer();
        };
    }
    private void AddGridColumn(string name, string header, string property, float weight)
    {
        _Gird.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = header,
            DataPropertyName = property,
            FillWeight = weight,
            SortMode = DataGridViewColumnSortMode.Automatic
        });
    }

    private void BindData()
    {
        _bindingSource.DataSource = _customers;
        _Gird.DataSource = _bindingSource;
    }
    public void ReloadCustomers()
    {
        try
        {
            IEnumerable<Customer> result =
                _searchCustomers?.Invoke(_txtKeywords.Text.Trim())
                ?? Enumerable.Empty<Customer>();

            _customers.Clear();
            foreach (var customer in result)
            {
                if (customer.IsActive)
                    _customers.Add(customer);
            }

            _bindingSource.DataSource = _customers;
            UpdateStatistics();
            UpdateCustomerDetails();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Không thể tải danh sách khách hàng.\n{ex.Message}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ApplySearch()
    {
        if (_searchCustomers is not null)
        {
            ReloadCustomers();
            return;
        }

        string keyword = _txtKeywords.Text.Trim();

        var filtered = _customers
            .Where(c =>
                ContainsText(c.FullName, keyword) ||
                ContainsText(c.Phone, keyword) ||
                ContainsText(c.IdentityNumber, keyword) ||
                ContainsText(c.Address, keyword))
            .ToList();

        _bindingSource.DataSource = new BindingList<Customer>(filtered);
        _lblCount.Text = $"Hiển thị {filtered.Count:N0} hồ sơ";
        UpdateCustomerDetails();
    }

    private static bool ContainsText(string? source, string keyword)
    {
        return string.IsNullOrWhiteSpace(keyword) ||
               (!string.IsNullOrEmpty(source) &&
                source.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateStatistics()
    {
        int total = _customers.Count;
        int domestic = _customers.Count(c =>
            ContainsText(c.IdentityType.ToString(), "CCCD") ||
            ContainsText(c.IdentityType.ToString(), "CMND"));

        int international = Math.Max(0, total - domestic);

        _lblTotal.Text = total.ToString("N0");
        _lblDomestic.Text = domestic.ToString("N0");
        _lblInternational.Text = international.ToString("N0");
        _lblActive.Text = total.ToString("N0");
        _lblCount.Text = $"Tổng: {total:N0} hồ sơ";
    }

    private void UpdateCustomerDetails()
    {
        var customer = GetSelectedCustomer();

        if (customer is null)
        {
            _lblDetailName.Text = "Chưa chọn khách hàng";
            _lblDetailId.Text = "—";
            _lblDetailIdentity.Text = "—";
            _lblDetailPhone.Text = "—";
            _lblDetailEmail.Text = "—";
            _lblDetailAddress.Text = "—";
            return;
        }

        _lblDetailName.Text = customer.FullName ?? "—";
        _lblDetailId.Text = $"{customer.CustomerId}  |  {(customer.IsActive ? "Đang hoạt động" : "Ngừng hoạt động")}";
        _lblDetailIdentity.Text = $"{customer.IdentityType}  -  {customer.IdentityNumber}";
        _lblDetailPhone.Text = customer.Phone ?? "—";
        _lblDetailEmail.Text = customer.Email ?? "—";
        _lblDetailAddress.Text = customer.Address ?? "—";
    }

    private Customer? GetSelectedCustomer()
    {
        return _Gird.CurrentRow?.DataBoundItem as Customer;
    }

    private void EditSelectedCustomer()
    {
        var customer = GetSelectedCustomer();
        if (customer is null)
        {
            MessageBox.Show(
                this,
                "Vui lòng chọn khách hàng cần cập nhật.",
                "Chưa chọn khách hàng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _navigate("S08", customer);
    }

    private void OpenReservation()
    {
        var customer = GetSelectedCustomer();
        if (customer is null)
        {
            MessageBox.Show(
                this,
                "Vui lòng chọn khách hàng trước khi tạo đặt phòng.",
                "Chưa chọn khách hàng",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _navigate("S10", customer);
    }

    private static Button CreateButton(string text, Color color)
    {
        var button = new Button
        {
            Text = text,
            Height = 32,
            MinimumSize = new Size(88, 32),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Ariel", 8, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(3),
            Padding = new Padding(5, 0, 5, 0),
            UseVisualStyleBackColor = false
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.12f);
        return button;
    }
}





