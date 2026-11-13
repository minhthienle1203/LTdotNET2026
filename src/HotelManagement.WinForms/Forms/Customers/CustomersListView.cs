using System.ComponentModel;
using System.Drawing;
using HotelManagement.Core.Entities;
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


}
