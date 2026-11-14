using System.Text.Json;
using HotelManagement.Infrastructure.Data;
using HotelManagement.WinForms.Forms.Dashboard;
using HotelManagement.WinForms.Forms.Main;
using HotelManagement.WinForms.Forms.Services;
namespace HotelManagement.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            string settingsPath = Path.Combine(
                AppContext.BaseDirectory,
                "appsettings.json");

            using var settings = JsonDocument.Parse(
                File.ReadAllText(settingsPath));

            string connectionString = settings.RootElement
                                          .GetProperty("ConnectionStrings")
                                          .GetProperty("HotelManagementDb")
                                          .GetString()
                                      ?? throw new Exception("Connection String không hợp lệ.");

            // kết nối SQLite
            var connectionFactory =
                new DbConnectionFactory(connectionString);

            // khởi tạo Database
            new DbInitializer().Initialize(connectionFactory);

            // Chỉ tạo một MainForm.
            var mainForm = new MainForm();

            // Đăng kí trang S01
            mainForm.RegisterPage(
                "S01",
                _ => new OverviewRoomMapView(mainForm.NavigateToScreen)
            );

            // Đăng kí trang S13 của m vào đây
            mainForm.RegisterPage(
                "S13",
                _ => new ServiceCatalogView(mainForm.NavigateToScreen)
            );

            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể khởi động ứng dụng:\n{ex.Message}",
                "Lỗi khởi tạo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
