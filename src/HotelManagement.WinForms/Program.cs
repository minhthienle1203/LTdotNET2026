using System.Text.Json;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using HotelManagement.WinForms.Forms.Customers;
using HotelManagement.Infrastructure.Data.Repositories;
using HotelManagement.WinForms.Forms.Dashboard;
using HotelManagement.WinForms.Forms.Invoices;
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
            var connectionFactory = new DbConnectionFactory(connectionString);

            // khởi tạo Database
            new DbInitializer().Initialize(connectionFactory);

            //dang ki cac repo, service
            var floorRepository = new FloorRepository(connectionFactory);
            var roomRepository = new RoomRepository(connectionFactory);
            var floorService = new FloorService(floorRepository);
            var roomService = new RoomService(roomRepository);
            // tạo một MainForm cố định

            var mainForm = new MainForm();
            // đăng kí các trang để load lên
            mainForm.RegisterPage(
                "S01",
                context => new OverviewRoomMapView(
                    roomService,
                    floorService,
                    mainForm.NavigateToScreen,
                    context)
            );
            mainForm.RegisterPage(
                "S07",
                _ => new CustomersListView(mainForm.NavigateToScreen)
            );

            // Đăng kí trang S13
            mainForm.RegisterPage(
                "S13",
                _ => new ServiceCatalogView(mainForm.NavigateToScreen)
            );
            // Đăng kí trang S15
            mainForm.RegisterPage(
                "S15",
                _ => new ServiceUsageView(mainForm.NavigateToScreen)
            );
            // Đăng kí trang S17
            mainForm.RegisterPage(
                "S17",
                _ => new InvoiceListView(mainForm.NavigateToScreen)
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
