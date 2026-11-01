namespace HotelManagement.WinForms.Forms.Main;

/// <summary>
/// Cửa sổ khung của ứng dụng. Các màn hình nghiệp vụ S01-S20 sẽ được nạp
/// vào _contentPanel dưới dạng UserControl khi triển khai từng chức năng.
/// MainForm không được tính là một trong 20 màn hình nghiệp vụ.
/// </summary>
public sealed class MainForm : Form
{
    private readonly Panel _contentPanel;

    public MainForm()
    {
        Text = "HotelManagement - Quản lý khách sạn";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Width = 1280;
        Height = 800;

        var navigationPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = Color.FromArgb(38, 49, 66)
        };

        var titleLabel = new Label
        {
            Text = "HOTEL MANAGEMENT",
            Dock = DockStyle.Top,
            Height = 72,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        navigationPanel.Controls.Add(titleLabel);

        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(24)
        };

        var welcomeLabel = new Label
        {
            Text = "Khung dự án đã được khởi tạo.\n\nCác màn hình nghiệp vụ S01-S20 sẽ được triển khai tại đây.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 14, FontStyle.Regular),
            ForeColor = Color.FromArgb(70, 80, 95)
        };
        _contentPanel.Controls.Add(welcomeLabel);

        Controls.Add(_contentPanel);
        Controls.Add(navigationPanel);
    }
}
