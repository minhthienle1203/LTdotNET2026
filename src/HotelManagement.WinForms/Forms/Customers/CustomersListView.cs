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
    private readonly DataGridView _dgv = new ();
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
        root.Controls.Add(BuildMainConten(), 0, 3);
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
        btnAdd.Click += (_, _) => _navigate("S08", Null);

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
        valueLabel.Controls.Add(noteLabel);
        valueLabel.Controls.Add(valueLabel);
        valueLabel.Controls.Add(titleLabel);
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

        _grid.Dock = DockStyle.Fill;
       listPanel.Controls.Add(_grid);
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

    }


    private object CreateButton(string themKhachHnagF2)
    {
        throw new NotImplementedException();
    }
}
