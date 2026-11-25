using System.Text.Json;
using HotelManagement.Core.Entities;
using HotelManagement.Core.Services;
using HotelManagement.Infrastructure.Data;
using HotelManagement.WinForms.Forms.Customers;
using HotelManagement.Infrastructure.Data.Repositories;
using HotelManagement.WinForms.Forms.Dashboard;
using HotelManagement.WinForms.Forms.Housekeeping;
using HotelManagement.WinForms.Forms.Invoices;
using HotelManagement.WinForms.Forms.Main;
using HotelManagement.WinForms.Forms.Reports;
using HotelManagement.WinForms.Forms.Reservations;
using HotelManagement.WinForms.Forms.Rooms;
using HotelManagement.WinForms.Forms.Services;
using HotelManagement.WinForms.Forms.Stays;

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
            var hotelServiceRepository = new HotelServiceRepository(connectionFactory);
            var hotelServiceService = new HotelServiceService(hotelServiceRepository);
            var invoiceRepository = new InvoiceRepository(connectionFactory);
            var invoiceService = new InvoiceService(invoiceRepository);
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
            mainForm.RegisterPage("S05", context => new RoomListView(roomService, floorService, mainForm.NavigateToScreen, context));
            mainForm.RegisterPage("S06", context=> new RoomFormView( roomService, floorService ,mainForm.NavigateToScreen, context));
            var customerRepository = new CustomerRepository(connectionFactory);
            var customerService = new CustomerService(customerRepository);
            mainForm.RegisterPage("S07",
                _ => new CustomersListView(mainForm.NavigateToScreen, customerService.SearchCustomers));
            mainForm.RegisterPage(
                "S08",
                param =>
                {
                    Customer? customer = param as Customer;
                    CustomerFormContext? ctx = param as CustomerFormContext;
                    bool isNew = customer is null || customer.CustomerId == 0;

                    return new CustomerFormView(
                        mainForm.NavigateToScreen,
                        customer,
                        ctx,
                        customerService.Save,
                        id => customerService.IsIdentityDuplicate(id, isNew ? null : customer!.CustomerId),
                        isNew ? customerService.GetSuggestedCode() : null);
                }
            );

            mainForm.RegisterPage(
                "S09",
                ctx => new ReservationListView(mainForm.NavigateToScreen, ctx)
            );
            mainForm.RegisterPage(
                "S10",
                ctx => new ReservationFormView(mainForm.NavigateToScreen, ctx)
            );


            // Đăng kí trang dichj vụ
            mainForm.RegisterPage(
                "S13",
                _ => new ServiceCatalogView(mainForm.NavigateToScreen, hotelServiceService)
            );
            // Đăng kí trang ghi nhận dịch vụ
            mainForm.RegisterPage(
                "S15",
                _ => new ServiceUsageView(mainForm.NavigateToScreen, hotelServiceService)
            );
            // Đăng kí trang checkout
            mainForm.RegisterPage(
                "S16",
                _ => new CheckOutView(mainForm.NavigateToScreen)
            );
            // Đăng kí trang hóa đơn
            mainForm.RegisterPage(
                "S17",
                _ => new InvoiceListView(mainForm.NavigateToScreen, invoiceService)
            );
            //đăng kí trang quản lí dọn phòng
            mainForm.RegisterPage("S19", _=> new HousekeepingView(roomService, mainForm.NavigateToScreen));

            //đăng kí trang báo cáo tháng
            mainForm.RegisterPage("S20", _=> new MonthlyReportView(mainForm.NavigateToScreen));

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
