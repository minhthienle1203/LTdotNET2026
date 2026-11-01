# ĐẶC TẢ DỰ ÁN – HỆ THỐNG QUẢN LÝ KHÁCH SẠN & ĐẶT PHÒNG

> **Tên dự án tạm dùng:** `HotelManagement`  
> **Loại ứng dụng:** Windows Forms Desktop Application  
> **Mục tiêu:** Ứng dụng quản lý nội bộ khách sạn, chạy offline trên một máy Windows, lưu dữ liệu trong SQL Server cục bộ.  
> **Quy mô giao diện:** 20 màn hình nghiệp vụ có thể điều hướng độc lập; không tính `MainForm` khung chứa, hộp thoại xác nhận hoặc thông báo nhỏ.

## 1. Tổng quan dự án

### 1.1. Mô tả

Hệ thống hỗ trợ nhân viên/lễ tân quản lý danh sách phòng, theo dõi trạng thái phòng trên sơ đồ từng tầng, lưu thông tin khách, lập đặt phòng, nhận phòng, ghi nhận dịch vụ khách sử dụng, trả phòng và lập hóa đơn. Hệ thống cũng tổng hợp công suất phòng, doanh thu phòng và doanh thu dịch vụ theo tháng.

Ứng dụng được thiết kế cho **một máy tính Windows tại quầy lễ tân**. Cơ sở dữ liệu được lưu trên SQL Server cục bộ. Ứng dụng không cần Internet, không gọi API, không đồng bộ với máy chủ bên ngoài.

### 1.2. Mục tiêu

- Thay thế việc theo dõi phòng và khoản thu bằng sổ hoặc bảng tính rời rạc.
- Giúp lễ tân nhận biết nhanh phòng trống, đang có khách, đang dọn và đã đặt trước.
- Lưu xuyên suốt thông tin khách, đặt phòng, lượt lưu trú, dịch vụ phát sinh và hóa đơn.
- Tự động tính tiền phòng, tiền dịch vụ, tiền cọc đã nhận và số tiền còn phải thu.
- Có số liệu báo cáo theo tháng để đánh giá công suất và doanh thu.
- Áp dụng nội dung đã học: C#, OOP, WinForms, Event-driven, Validation, Exception Handling, File I/O ở mức cần thiết và ADO.NET/SqlClient.

### 1.3. Người dùng và vai trò

 Người dùng nghiệp vụ chính là **nhân viên/lễ tân**. Chưa xây dựng phân quyền nhiều vai trò hoặc quy trình phê duyệt. Không có tài khoản khách hàng và không có màn hình tự đặt phòng từ phía khách.

---

## 2. Phạm vi chức năng

### 2.1. Chức năng nằm trong phạm vi

| Mã | Nhóm chức năng | Nội dung |
|---|---|---|
| F01 | Tổng quan / Sơ đồ phòng | Xem phòng theo tầng; màu trạng thái; xem thông tin nhanh; lọc theo tầng/trạng thái. |
| F02 | Danh mục phòng | Quản lý tầng, loại phòng, phòng, giá theo giờ/ngày, sức chứa và trạng thái hoạt động. |
| F03 | Khách hàng | Lưu và tra cứu thông tin khách do lễ tân nhập. |
| F04 | Đặt phòng trước | Tạo, xem, cập nhật và hủy đặt phòng; lưu thời gian dự kiến, loại phòng, khách và tiền cọc. |
| F05 | Check-in | Chọn phòng trống hoặc phòng đã đặt; xác nhận CCCD/hộ chiếu; tạo lượt lưu trú; chuyển phòng sang Đang ở. |
| F06 | Danh mục dịch vụ | Quản lý các dịch vụ có thể tính phí như đồ uống, giặt ủi, thuê xe. |
| F07 | Dịch vụ phát sinh | Ghi nhận dịch vụ, số lượng và thời điểm sử dụng vào lượt lưu trú/phòng tương ứng. |
| F08 | Check-out | Tính tiền phòng và dịch vụ; trừ tiền cọc; xác nhận thanh toán mô phỏng; hoàn tất lượt lưu trú. |
| F09 | Hóa đơn | Lưu hóa đơn; hiển thị chi tiết; thao tác In mô phỏng bằng thông báo hoàn thành. |
| F10 | Dọn phòng | Chuyển phòng sang Đang dọn sau check-out; cho phép xác nhận dọn xong để phòng trở lại Trống. |
| F11 | Báo cáo | Báo cáo công suất phòng, doanh thu phòng và doanh thu dịch vụ theo tháng. |

### 2.2. Ngoài phạm vi

- Khách tự đăng ký, đăng nhập, tự đặt phòng hoặc tự xem lịch phòng.
- Website, ứng dụng điện thoại, Web API, máy chủ từ xa, đồng bộ nhiều chi nhánh hoặc nhiều máy qua mạng.
- Cổng thanh toán ngân hàng, ví điện tử, quẹt thẻ, hóa đơn điện tử hoặc kết nối đơn vị thanh toán.
- Kết nối đối tác thuê xe, dịch vụ giặt ủi, nhà hàng hoặc nhà cung cấp bên ngoài. Nhân viên chỉ chọn dịch vụ trong danh mục và nhập số lượng/đơn giá.
- Gửi SMS, email, thông báo Internet, bản đồ, định vị hoặc xác minh CCCD/hộ chiếu trực tuyến.
- Quản lý kho đồ uống, nguyên vật liệu, lương nhân viên, kế toán tổng hợp, thuế và quản lý chuỗi khách sạn.
- Phân quyền chi tiết theo chức vụ, chấm công hoặc quản lý tài khoản nhân viên.
- In thực tế qua máy in ở phiên bản yêu cầu hiện tại. Nút **In hóa đơn** chỉ mô phỏng thao tác và hiển thị thông báo hoàn thành; dữ liệu hóa đơn vẫn lưu trong CSDL.

> Ứng dụng không kết nối mạng trong lúc sử dụng. Máy vẫn cần có SQL Server Express LocalDB (hoặc SQL Server Express cài trên chính máy đó) để lưu dữ liệu cục bộ.

---

## 3. Công nghệ và môi trường phát triển

| Thành phần | Công nghệ / phiên bản | Ghi chú |
|---|---|---|
| Ngôn ngữ | C# | Dùng OOP, class, properties, enum, nullable reference types. |
| Nền tảng | **.NET 10** | Cách gọi chính xác là .NET 10 |
| Target Framework | `net10.0-windows` | WinForms là công nghệ giao diện chỉ dành cho Windows. |
| SDK | `dotnet-sdk-10.0.401` | Theo môi trường đã chọn. |
| IDE | JetBrains Rider 2026.2.2 | Dùng tạo, thiết kế, build, debug WinForms. |
| Giao diện | Windows Forms | Lập trình hướng sự kiện; dùng Form, UserControl, DataGridView, BindingSource, ErrorProvider, MessageBox. |
| CSDL | Microsoft SQL Server Express LocalDB | |
| Truy cập CSDL | ADO.NET – `Microsoft.Data.SqlClient` | Dùng `SqlConnection`, `SqlCommand`, `SqlParameter`, `SqlDataReader`, `SqlTransaction`. |
| Quản lý mã nguồn | Git + SmartGit Education | Dùng branch riêng theo chức năng, commit có nội dung rõ ràng. |
| Cài đặt | `dotnet publish` + Inno Setup (đề xuất) | Tạo bản publish Release và đóng gói thành file cài đặt `.exe`. |

### 3.1. Cấu hình dự án WinForms

Tệp `.csproj` cần có cấu hình tương đương:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Data.SqlClient" Version="7.1.1" />
  </ItemGroup>
</Project>
```

Không tự chép cứng một phiên bản NuGet khác nhau ở từng máy. Một thành viên thêm/cập nhật package thì commit cả `.csproj` và `packages.lock.json` nếu nhóm bật chế độ khóa phiên bản.

### 3.2. Lựa chọn cách truy cập dữ liệu

Đồ án thống nhất sử dụng **ADO.NET trực tiếp qua `Microsoft.Data.SqlClient`** để phù hợp yêu cầu SQL Client và nội dung đã học. Không trộn ADO.NET và Entity Framework Core trong cùng một nghiệp vụ ở phiên bản đầu.

| Thành phần | Vai trò |
|---|---|
| `SqlConnection` | Mở kết nối đến SQL Server cục bộ. |
| `SqlCommand` | Thực thi SELECT/INSERT/UPDATE/DELETE hoặc Stored Procedure. |
| `SqlParameter` | Truyền giá trị đầu vào an toàn; không nối chuỗi dữ liệu người dùng vào SQL. |
| `SqlDataReader` | Đọc dữ liệu tuần tự khi cần lấy danh sách/kết quả truy vấn. |
| `SqlTransaction` | Đảm bảo các bước check-in/check-out liên quan được thực hiện đồng bộ. |
| `DataTable` / `BindingSource` | Hỗ trợ đưa dữ liệu lên DataGridView và lọc/hiển thị trên WinForms. |

Mọi thao tác CSDL phải đi qua lớp `Repository` hoặc `DataAccess`, không viết SQL trực tiếp trong sự kiện `Button_Click` của Form. Entity vẫn là các lớp C# thuần (POCO); Repository tự ánh xạ dữ liệu đọc từ `SqlDataReader`/`DataTable` sang Entity hoặc DTO. Dù môn học có giới thiệu Entity Framework Core, phiên bản này không dùng EF Core song song để tránh hai cơ chế truy cập dữ liệu trong cùng đồ án. Nếu giảng viên yêu cầu EF Core, cần thống nhất thay lớp Repository trước khi triển khai, không trộn hai cách trong một nghiệp vụ.

### 3.3. Cấu hình kết nối cục bộ

Chuỗi kết nối mẫu cho LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=HotelManagementDb;Integrated Security=True;TrustServerCertificate=True;
```

- `Server`: instance LocalDB trên chính máy đang chạy chương trình.
- `Database`: tên cơ sở dữ liệu của đồ án.
- `Integrated Security=True`: dùng Windows Authentication, không lưu tài khoản/mật khẩu SQL trong mã nguồn.
- `TrustServerCertificate=True`: chỉ dùng cho môi trường SQL Server cục bộ của đồ án khi cần xử lý chứng chỉ tự ký.

Chuỗi kết nối đặt ở cấu hình riêng, chẳng hạn `appsettings.json` được đọc bằng `System.Text.Json`/Configuration package đã cài, hoặc tệp cấu hình ngoài thư mục cài đặt. Không commit mật khẩu hoặc chuỗi kết nối chứa thông tin bí mật lên Git.

### 3.4. Lưu ý về Rider và WinForms Designer

Nếu Designer của một Form gặp lỗi, vẫn có thể chỉnh sửa mã `Form.cs`/`Form.Designer.cs`, nhưng không sửa thủ công phần mã do Designer sinh ra nếu chưa hiểu cấu trúc.

---

## 4. Kiến trúc và cấu trúc thư mục

### 4.1. Kiến trúc phân tầng

Luồng xử lý thống nhất:

```text
WinForms Form / UserControl
        ↓ gọi
Service (nghiệp vụ + kiểm tra quy tắc)
        ↓ gọi
Repository / DataAccess (ADO.NET + SQL)
        ↓
SQL Server LocalDB
```

| Tầng | Trách nhiệm | Không thực hiện |
|---|---|---|
| Presentation | Hiển thị Form, nhận thao tác, binding dữ liệu, thông báo lỗi/thành công. | Không viết câu SQL và không tự tính nghiệp vụ phức tạp. |
| Business / Core | Entity, enum, DTO nội bộ, Validation, tính tiền, điều phối quy trình. | Không phụ thuộc vào WinForms Control hoặc MessageBox. |
| Data / Infrastructure | Kết nối SQL, Repository, truy vấn có tham số, transaction. | Không hiển thị giao diện và không tự quyết định quy tắc nghiệp vụ. |

### 4.2. Cấu trúc solution đề xuất

```text
HotelManagement/
├── HotelManagement.sln
├── src/
│   ├── HotelManagement.WinForms/
│   │   ├── Program.cs
│   │   ├── Forms/
│   │   │   ├── Main/
│   │   │   │   └── MainForm.cs                  # Khung điều hướng; không tính vào 20 màn hình
│   │   │   ├── Dashboard/
│   │   │   │   └── OverviewRoomMapView.cs      # S01 – Tổng quan và sơ đồ phòng
│   │   │   ├── Floors/
│   │   │   │   └── FloorManagementView.cs      # S02 – Quản lý tầng
│   │   │   ├── RoomTypes/
│   │   │   │   ├── RoomTypeListView.cs          # S03 – Danh sách loại phòng
│   │   │   │   └── RoomTypeFormView.cs          # S04 – Thêm/cập nhật loại phòng
│   │   │   ├── Rooms/
│   │   │   │   ├── RoomListView.cs              # S05 – Danh sách phòng
│   │   │   │   └── RoomFormView.cs              # S06 – Thêm/cập nhật phòng
│   │   │   ├── Customers/
│   │   │   │   ├── CustomerListView.cs          # S07 – Danh sách khách hàng
│   │   │   │   └── CustomerFormView.cs          # S08 – Thêm/cập nhật khách hàng
│   │   │   ├── Reservations/
│   │   │   │   ├── ReservationListView.cs       # S09 – Danh sách đặt phòng
│   │   │   │   └── ReservationFormView.cs       # S10 – Tạo/cập nhật đặt phòng
│   │   │   ├── CheckIn/
│   │   │   │   ├── CheckInView.cs               # S11 – Nhận phòng
│   │   │   │   └── ActiveStaysView.cs           # S12 – Danh sách lưu trú đang diễn ra
│   │   │   ├── Services/
│   │   │   │   ├── ServiceCatalogView.cs        # S13 – Danh mục dịch vụ
│   │   │   │   ├── ServiceFormView.cs            # S14 – Thêm/cập nhật dịch vụ
│   │   │   │   └── ServiceUsageView.cs           # S15 – Ghi nhận dịch vụ phát sinh
│   │   │   ├── CheckOut/
│   │   │   │   └── CheckOutView.cs              # S16 – Trả phòng và thanh toán
│   │   │   ├── Invoices/
│   │   │   │   ├── InvoiceListView.cs           # S17 – Danh sách hóa đơn
│   │   │   │   └── InvoiceDetailView.cs         # S18 – Chi tiết và in mô phỏng
│   │   │   ├── Housekeeping/
│   │   │   │   └── HousekeepingView.cs          # S19 – Dọn phòng
│   │   │   └── Reports/
│   │   │       └── MonthlyReportView.cs         # S20 – Báo cáo tháng
│   │   ├── Controls/                   # Control dùng chung
│   │   ├── Common/                     # Dialog, format, binding dùng chung
│   │   └── Resources/                  # Icon, hình giao diện
│   │
│   ├── HotelManagement.Core/
│   │   ├── Entities/                   # Entity mô tả ở file Entity riêng
│   │   ├── Enums/                      # RoomStatus, ReservationStatus,...
│   │   ├── DTOs/                       # Đối tượng dữ liệu phục vụ màn hình
│   │   ├── Interfaces/                 # Hợp đồng Repository nếu cần
│   │   ├── Services/                   # RoomService, BookingService,...
│   │   ├── Validators/                 # Kiểm tra dữ liệu
│   │   └── Exceptions/                 # Lỗi nghiệp vụ
│   │
│   └── HotelManagement.Infrastructure/
│       ├── Data/
│       │   ├── DbConnectionFactory.cs
│       │   ├── DbInitializer.cs
│       │   └── Repositories/
│       ├── Configuration/             # Đọc cấu hình cục bộ
│       └── Sql/                        # Câu lệnh SQL dùng chung nếu cần
│
├── database/
│   ├── 01_CreateDatabase.sql
│   ├── 02_CreateTables.sql
│   ├── 03_CreateConstraintsAndIndexes.sql
│   └── 04_SeedData.sql
├── installer/
│   └── HotelManagement.iss            # Script Inno Setup
├── tests/
│   └── HotelManagement.Tests/         # Unit test nghiệp vụ nếu đủ thời gian
├── docs/
│   ├── HotelManagement_Project_Spec.md
│   └── HotelManagement_Entities.md
├── .gitignore
└── README.md
```

### 4.3. Quy tắc phụ thuộc

- `WinForms` được tham chiếu `Core` và `Infrastructure`.
- `Infrastructure` được tham chiếu `Core`.
- `Core` không tham chiếu `WinForms` hoặc `Infrastructure`.
- Form chỉ gọi Service; Service gọi Repository; Repository thực thi SQL.
- Entity là lớp dữ liệu nghiệp vụ, không phải Form và không chứa logic truy vấn SQL.
- Các nghiệp vụ quan trọng như check-in, ghi dịch vụ, check-out phải nằm trong Service để có thể kiểm thử và không bị lặp ở nhiều màn hình.

---

## 5. Bố cục và điều hướng giao diện

### 5.1. Form chính và nguyên tắc đếm màn hình

`MainForm` là khung cửa sổ chính sau khi mở chương trình, chịu trách nhiệm điều hướng và hiển thị nội dung. `MainForm` **không tính** là một trong 20 màn hình nghiệp vụ.

Bố cục đề xuất:

- Thanh tiêu đề: tên khách sạn / tên ứng dụng.
- Thanh điều hướng trái hoặc trên, chia nhóm: Tổng quan; Quản lý phòng; Khách hàng; Đặt phòng; Lễ tân; Dịch vụ; Hóa đơn; Dọn phòng; Báo cáo.
- Khu vực nội dung: nạp `UserControl` tương ứng với màn hình được chọn vào một `Panel`.
- Thanh trạng thái: ngày giờ hệ thống và trạng thái kết nối CSDL.

**Quy ước để đáp ứng yêu cầu 15–20 màn hình:**

1. Đồ án triển khai **20 màn hình nghiệp vụ**, đánh mã từ `S01` đến `S20` như bảng bên dưới.
2. Mỗi màn hình là một điểm đến điều hướng độc lập trong khu vực nội dung của `MainForm`. Có thể triển khai bằng `UserControl`; không bắt buộc phải tạo 20 cửa sổ `Form` độc lập.
3. Các màn hình danh sách và màn hình thêm/cập nhật được tách riêng khi chúng có luồng nhập liệu, trường dữ liệu và thao tác lưu riêng.
4. `MessageBox`, hộp thoại xác nhận, tab, panel chi tiết nhỏ và popup chọn dữ liệu **không được tính** thành màn hình riêng.
5. Màn hình tạo/cập nhật được mở từ nút Thêm hoặc Sửa của màn hình danh sách; sau khi lưu/hủy thì quay lại màn hình danh sách.
6. Không bổ sung màn hình đăng nhập vì phiên bản đồ án hiện tại không yêu cầu quản lý tài khoản hoặc phân quyền.

### 5.2. Danh sách 20 màn hình nghiệp vụ

| Mã | Màn hình | Thành phần giao diện chính | Thao tác / phạm vi |
|---|---|---|---|
| **S01** | Tổng quan và sơ đồ phòng | Thẻ thống kê số phòng theo trạng thái; chọn tầng; bộ lọc trạng thái; lưới thẻ phòng; chú giải màu | Xem nhanh tình hình khách sạn; chọn phòng để mở các nghiệp vụ phù hợp; là màn hình đầu tiên sau khi mở ứng dụng. |
| **S02** | Quản lý tầng | `DataGridView`, thông tin số tầng/tên tầng/thứ tự/trạng thái, vùng thêm-sửa | Thêm, sửa, ngừng sử dụng tầng; kiểm tra tầng trước khi gán phòng. |
| **S03** | Danh sách loại phòng | `DataGridView`, tìm kiếm, lọc đang kinh doanh/ngừng kinh doanh | Xem danh sách loại phòng; mở S04 để thêm hoặc cập nhật; xem số phòng thuộc từng loại. |
| **S04** | Thêm/cập nhật loại phòng | Form nhập tên, mô tả, sức chứa, giá giờ, giá ngày, trạng thái | Lưu loại phòng mới hoặc cập nhật loại phòng; kiểm tra giá không âm và sức chứa lớn hơn 0. |
| **S05** | Danh sách phòng | `DataGridView`, tìm theo mã phòng, lọc tầng/loại/trạng thái | Xem và quản lý danh sách phòng; mở S06 để thêm/cập nhật; không xóa cứng phòng có lịch sử. |
| **S06** | Thêm/cập nhật phòng | Form nhập mã phòng, tầng, loại phòng, ghi chú, trạng thái hoạt động | Tạo hoặc cập nhật phòng; kiểm tra mã không trùng, tầng và loại phòng đang hoạt động. |
| **S07** | Danh sách khách hàng | Ô tìm kiếm theo tên/điện thoại/giấy tờ; `DataGridView` | Tra cứu khách; mở S08 để thêm/cập nhật hồ sơ; chọn khách để chuyển sang quy trình đặt phòng. |
| **S08** | Thêm/cập nhật khách hàng | Form họ tên, loại giấy tờ, số giấy tờ, điện thoại, email, địa chỉ | Lưu hồ sơ khách; kiểm tra dữ liệu bắt buộc, định dạng CCCD/hộ chiếu, số điện thoại và email. |
| **S09** | Danh sách đặt phòng | `DataGridView`, lọc theo ngày đến và trạng thái, tìm mã đặt phòng/khách | Xem đặt phòng; mở S10 để tạo/cập nhật; thực hiện hủy đặt phòng còn hiệu lực; chuyển đặt phòng đến hạn sang S11. |
| **S10** | Tạo/cập nhật đặt phòng | Chọn khách, ngày giờ đến/đi, loại phòng, phòng cụ thể không bắt buộc, tiền cọc, ghi chú | Kiểm tra khả dụng; tạo hoặc cập nhật đặt phòng; có thể chỉ giữ chỗ theo loại phòng hoặc gán một phòng cụ thể. |
| **S11** | Nhận phòng (Check-in) | Danh sách đặt phòng đến hạn; tìm khách; chọn phòng; số khách; thông tin giấy tờ | Check-in từ đặt phòng hoặc khách vãng lai; tạo `Stay`; chuyển phòng sang `OCCUPIED`. |
| **S12** | Danh sách lưu trú đang diễn ra | Danh sách phòng đang có khách, khách đại diện, giờ check-in, số khách, tổng dịch vụ tạm tính | Tra cứu các `Stay` đang mở; chọn lượt lưu trú để mở S15 ghi dịch vụ hoặc S16 trả phòng. |
| **S13** | Danh mục dịch vụ | `DataGridView`, tìm kiếm, lọc nhóm và trạng thái | Xem dịch vụ đồ uống/giặt ủi/thuê xe/khác; mở S14 để thêm/cập nhật/ngừng cung cấp. |
| **S14** | Thêm/cập nhật dịch vụ | Form tên dịch vụ, nhóm, đơn vị tính, đơn giá, mô tả, trạng thái | Lưu thông tin dịch vụ; không xóa cứng dịch vụ đã phát sinh trong lịch sử. |
| **S15** | Ghi nhận dịch vụ phát sinh | Thông tin lượt lưu trú/phòng; chọn dịch vụ; số lượng; đơn giá; bảng các khoản đã ghi | Thêm dịch vụ vào `Stay` đang `IN_PROGRESS`; lưu đơn giá tại thời điểm phát sinh và thành tiền. |
| **S16** | Trả phòng và thanh toán | Thông tin `Stay`, thời gian ở, cách tính giờ/ngày, bảng dịch vụ, tiền phòng, tiền cọc, số tiền phải thu/hoàn | Tính tiền; cho phép kiểm tra trước khi xác nhận; bấm Thanh toán để mô phỏng thành công và lưu hóa đơn/payment. Sau đó mở S18 để in mô phỏng. |
| **S17** | Danh sách hóa đơn | `DataGridView`, lọc ngày lập/trạng thái/mã hóa đơn/mã phòng/khách | Tra cứu hóa đơn đã tạo; mở S18 xem chi tiết và tiếp tục in nếu chưa in. |
| **S18** | Chi tiết hóa đơn và in mô phỏng | Thông tin khách/phòng/thời gian; chi tiết tiền phòng/dịch vụ/cọc; trạng thái thanh toán và in | Xem nội dung hóa đơn; bấm In hóa đơn để hiển thị thông báo hoàn thành và lưu `PrintedAt`; hoàn tất luồng checkout theo quy tắc nghiệp vụ. |
| **S19** | Dọn phòng | Danh sách phòng `CLEANING`, thông tin phòng và nút xác nhận dọn xong | Xác nhận phòng đã dọn; chuyển về `AVAILABLE` hoặc `RESERVED` nếu còn đặt phòng được gán có hiệu lực. |
| **S20** | Báo cáo tháng | Bộ chọn tháng/năm; thẻ tổng hợp; bảng số liệu; biểu đồ bổ sung | Xem công suất phòng, doanh thu phòng và doanh thu dịch vụ theo tháng. |

**Tổng số: 20 màn hình nghiệp vụ (`S01`–`S20`).** Danh sách trên là căn cứ thống nhất để lập task, tạo thư mục giao diện, chia người thực hiện và nghiệm thu. Không cộng thêm các popup hoặc hộp thoại xác nhận vào con số này.

### 5.3. Luồng điều hướng giữa các màn hình

```text
S01 Tổng quan & sơ đồ phòng
 ├── S02 Quản lý tầng
 ├── S03 Danh sách loại phòng ──> S04 Thêm/cập nhật loại phòng
 ├── S05 Danh sách phòng ───────> S06 Thêm/cập nhật phòng
 ├── S07 Danh sách khách hàng ──> S08 Thêm/cập nhật khách hàng
 ├── S09 Danh sách đặt phòng ───> S10 Tạo/cập nhật đặt phòng
 │                                  └──> S11 Nhận phòng
 ├── S11 Nhận phòng (khách vãng lai hoặc có đặt trước)
 ├── S12 Lưu trú đang diễn ra
 │    ├──> S15 Ghi nhận dịch vụ phát sinh
 │    └──> S16 Trả phòng & thanh toán ──> S18 Chi tiết hóa đơn/in mô phỏng
 ├── S13 Danh mục dịch vụ ───────> S14 Thêm/cập nhật dịch vụ
 ├── S17 Danh sách hóa đơn ──────> S18 Chi tiết hóa đơn/in mô phỏng
 ├── S19 Dọn phòng
 └── S20 Báo cáo tháng
```

Sơ đồ thể hiện các điểm điều hướng chính, không phải sơ đồ trình tự xử lý chi tiết. Việc mở S15/S16 phải mang theo `StayId`; việc mở S18 phải mang theo `InvoiceId`, tránh truy vấn lại bằng mã phòng hoặc thông tin hiển thị không duy nhất.

---

## 6. Đặc tả nghiệp vụ chi tiết

### 6.1. F01 – Sơ đồ phòng theo tầng

**Mục đích:** cho lễ tân quan sát nhanh tình trạng sử dụng phòng trong khách sạn.

**Luồng chính:**

1. Mở màn hình Tổng quan.
2. Hệ thống tải danh sách tầng đang hoạt động.
3. Người dùng chọn tầng.
4. Hệ thống hiển thị các phòng thuộc tầng đó dưới dạng thẻ hoặc ô lưới.
5. Mỗi ô hiển thị tối thiểu: mã phòng, loại phòng, giá cơ bản và trạng thái.
6. Người dùng chọn một phòng để xem chi tiết và mở nghiệp vụ phù hợp.

**Màu trạng thái theo yêu cầu:**

| Trạng thái | Màu | Ý nghĩa |
|---|---|---|
| `AVAILABLE` – Phòng trống | Xanh lá | Có thể nhận khách nếu không bị vướng đặt phòng trong khoảng thời gian cần nhận. |
| `OCCUPIED` – Đang có khách | Đỏ | Có một lượt lưu trú chưa check-out. |
| `CLEANING` – Đang dọn dẹp | Vàng | Đã trả phòng nhưng nhân viên chưa xác nhận dọn xong. Không được check-in. |
| `RESERVED` – Đã đặt trước | Cam | Phòng cụ thể đã được gán cho một đặt phòng còn hiệu lực. |

**Quy tắc:**

- Phòng `OCCUPIED` không được gán cho đặt phòng khác có khoảng thời gian trùng.
- Phòng `CLEANING` không được nhận khách.
- Phòng `RESERVED` chỉ được nhận khách của đặt phòng tương ứng; lễ tân có thể đổi sang phòng khác cùng loại nếu phòng mới hợp lệ.
- Khi hoàn tất dọn phòng, tính lại trạng thái: chuyển `CLEANING` → `RESERVED` nếu vẫn còn đặt phòng được gán có hiệu lực, nếu không thì → `AVAILABLE`.
- Nếu đặt phòng bị hủy, chỉ giải phóng phòng về `AVAILABLE` khi không còn đặt phòng được gán nào khác; nếu còn thì giữ `RESERVED`.
- Trạng thái `RESERVED` chỉ áp dụng khi đặt phòng đã được gán `RoomId` cụ thể. Đặt phòng chỉ giữ chỗ theo loại phòng vẫn hiển thị trong danh sách đặt phòng, không tự ý tô cam một phòng bất kỳ.

### 6.2. F02 – Quản lý tầng, loại phòng và phòng

**Dữ liệu tầng:** mã tầng, số tầng, tên hiển thị, thứ tự sắp xếp, trạng thái hoạt động.

**Dữ liệu loại phòng:** mã loại, tên loại, mô tả, sức chứa tối đa, giá theo giờ, giá theo ngày, trạng thái hoạt động.

**Dữ liệu phòng:** mã phòng, tầng, loại phòng, trạng thái hiện tại, ghi chú, trạng thái hoạt động.

**Thao tác:**

- Thêm, sửa thông tin phòng và loại phòng.
- Ngừng sử dụng thay vì xóa cứng nếu bản ghi đã phát sinh đặt phòng/lưu trú.
- Khi đổi loại phòng, không được vi phạm dữ liệu đặt phòng đang còn hiệu lực.
- Không cho phép trùng mã phòng trong toàn khách sạn.
- Chỉ phòng đang `AVAILABLE` mới được chuyển thủ công sang trạng thái `CLEANING` trong trường hợp bảo trì/dọn bổ sung; không cho phép chỉnh tay sang `OCCUPIED` hoặc `RESERVED`. Hai trạng thái này phải được cập nhật qua nghiệp vụ đặt phòng/check-in.

### 6.3. F03 – Quản lý khách hàng
**Thông tin lưu:** họ tên, loại giấy tờ (`CCCD` hoặc `PASSPORT`), số giấy tờ, số điện thoại, email (không bắt buộc), địa chỉ (không bắt buộc), ngày tạo.

**Luồng chính:**

1. Lễ tân tìm khách theo số giấy tờ, số điện thoại hoặc tên.
2. Nếu đã tồn tại, chọn hồ sơ có sẵn và kiểm tra lại thông tin.
3. Nếu chưa tồn tại, tạo hồ sơ mới.
4. Lưu khách để liên kết với đặt phòng hoặc lượt lưu trú.

**Lưu ý:** đây là thao tác do nhân viên nhập. Không có màn hình để khách tự đăng ký hoặc tự cập nhật hồ sơ.

### 6.4. F04 – Đặt phòng trước
**Thông tin bắt buộc:** khách hàng, ngày/giờ dự kiến đến, ngày/giờ dự kiến đi, loại phòng.  
**Thông tin tùy chọn:** phòng cụ thể (nếu lễ tân muốn giữ một phòng), số tiền đặt cọc đã nhận, ghi chú.

**Luồng chính:**

1. Lễ tân tìm hoặc tạo hồ sơ khách.
2. Nhập ngày giờ dự kiến đến và đi.
3. Chọn loại phòng.
4. Hệ thống kiểm tra số phòng phù hợp còn khả dụng trong khoảng thời gian đó.
5. Có thể chỉ lưu yêu cầu theo loại phòng, hoặc gán trước một phòng cụ thể cùng loại.
6. Nhập tiền cọc thực tế đã nhận (có thể bằng 0).
7. Lưu đặt phòng với trạng thái `CONFIRMED`.
8. Nếu gán phòng cụ thể, phòng chuyển `AVAILABLE` → `RESERVED`.

**Quy tắc:**

- Thời điểm dự kiến đi phải lớn hơn thời điểm dự kiến đến.
- Phòng được gán phải thuộc đúng loại phòng đã chọn.
- Không cho phép hai đặt phòng còn hiệu lực gán cùng một phòng trong khoảng thời gian giao nhau.
- Đặt phòng chưa gán phòng vẫn phải được tính vào số chỗ đã giữ của loại phòng. Số phòng còn khả dụng = số phòng đang kinh doanh của loại đó trừ số phòng đã bị chiếm/được gán trong khoảng thời gian và trừ số đặt phòng cùng loại chưa gán phòng còn hiệu lực trong khoảng thời gian. Không đếm trùng một phòng đã có Stay và Reservation tương ứng.
- Khi hủy đặt phòng, trạng thái chuyển `CANCELLED`; nếu đã gán phòng thì tính lại trạng thái phòng: ưu tiên `OCCUPIED`, sau đó `CLEANING`, sau đó `RESERVED` nếu vẫn còn đặt phòng được gán có hiệu lực, còn lại `AVAILABLE`.
- Không sửa đặt phòng đã chuyển thành `CHECKED_IN` hoặc `COMPLETED`; nếu cần thay đổi phải thực hiện nghiệp vụ điều chỉnh riêng (không bắt buộc trong MVP).


### 6.5. F05 – Check-in / Nhận phòng
**Hai trường hợp:**

- **Khách có đặt trước:** chọn đặt phòng đến hạn, kiểm tra khách và ngày dự kiến, chọn phòng đã giữ hoặc phòng trống cùng loại.
- **Khách vãng lai:** tìm/tạo khách, chọn loại phòng và một phòng `AVAILABLE`, không có đặt phòng xung đột trong khoảng thời gian dự kiến.

**Luồng chính:**

1. Chọn đặt phòng hoặc tạo lượt nhận phòng vãng lai.
2. Xác nhận họ tên và loại/số CCCD hoặc hộ chiếu.
3. Nhập số lượng khách thực tế.
4. Chọn phòng hợp lệ.
5. Hệ thống lưu thời điểm check-in thực tế và giá giờ/ngày tại thời điểm nhận phòng.
6. Tạo `Stay` ở trạng thái `IN_PROGRESS`.
7. Cập nhật phòng thành `OCCUPIED`; đặt phòng (nếu có) thành `CHECKED_IN`.

**Quy tắc:**

- Không nhận phòng `OCCUPIED` hoặc `CLEANING`.
- Phòng `RESERVED` chỉ được nhận nếu khớp đặt phòng; trường hợp đổi phòng phải kiểm tra cùng loại và giải phóng phòng cũ.
- Số khách không vượt quá sức chứa của loại phòng.
- Một phòng không được có hai `Stay` cùng trạng thái `IN_PROGRESS`.
- Các bước tạo Stay và cập nhật Room/Reservation phải nằm trong cùng một SQL transaction.



### 6.6. F06 – Danh mục dịch vụ

Dữ liệu dịch vụ gồm: mã dịch vụ, tên, nhóm dịch vụ, đơn vị tính, đơn giá, trạng thái hoạt động và mô tả.


Nhóm dịch vụ dự kiến:



Đồ uống.

Giặt ủi.

Thuê xe.

Khác.


Nhân viên được thêm, sửa, ngừng cung cấp dịch vụ. Không xóa cứng dịch vụ đã xuất hiện trong lịch sử sử dụng.




### 6.7. F07 – Ghi nhận dịch vụ phát sinh

Luồng chính:



Lễ tân chọn phòng đang OCCUPIED hoặc lượt lưu trú IN_PROGRESS.

Chọn dịch vụ từ danh mục.

Nhập số lượng và xác nhận đơn giá.

Hệ thống ghi thời điểm sử dụng, số lượng, đơn giá tại thời điểm ghi nhận và thành tiền.

Tổng tiền dịch vụ của lượt lưu trú được cập nhật khi xem chi tiết hoặc tính checkout.


Quy tắc:



Chỉ ghi dịch vụ cho lượt lưu trú đang mở.

Số lượng phải lớn hơn 0.

Đơn giá tại thời điểm ghi nhận được lưu riêng, không thay đổi khi giá trong danh mục thay đổi.

Thành tiền = số lượng × đơn giá tại thời điểm ghi nhận.

Không cho phép thêm dịch vụ sau khi Stay đã COMPLETED.

Dịch vụ thuê xe/giặt ủi chỉ là khoản phí do nhân viên ghi nhận; không gọi hệ thống của đối tác.





### 6.8. F08 – Check-out / Trả phòng và tính tiền

Dữ liệu tính tiền:



Thời điểm check-in thực tế.

Thời điểm check-out thực tế.

Giá giờ và giá ngày đã chụp tại thời điểm check-in.

Cách tính tiền được lễ tân chọn: theo giờ hoặc theo ngày.

Tổng tiền dịch vụ.

Tiền cọc thực tế đã nhận.


Công thức đề xuất để thống nhất cho đồ án:


Gọi H là tổng số giờ lưu trú thực tế, làm tròn lên đến giờ nguyên gần nhất.



Nếu chọn theo giờ: RoomCharge = max(1, ceil(H)) × HourlyRateAtCheckIn.

Nếu chọn theo ngày: RoomCharge = max(1, ceil(H / 24)) × DailyRateAtCheckIn.

ServiceCharge = tổng Amount của các StayService.

SubTotal = RoomCharge + ServiceCharge.

DepositDeducted = min(DepositAmount, SubTotal).

AmountDue = max(0, SubTotal - DepositAmount).

RefundDue = max(0, DepositAmount - SubTotal).



Quy tắc trên là quy tắc tính đơn giản cho đồ án, chưa áp dụng giờ nhận/trả phòng chuẩn khách sạn, phụ thu quá giờ, miễn phí vài phút, khuyến mãi hoặc thuế. Nếu giảng viên có quy tắc khác, cập nhật công thức tại đây và trong CheckOutService.



Luồng chính:



Chọn phòng đang có khách.

Hệ thống tìm Stay IN_PROGRESS tương ứng.

Ghi nhận thời điểm check-out thực tế.

Lễ tân chọn tính theo giờ hoặc theo ngày.

Hệ thống tính tiền phòng, tiền dịch vụ, tiền cọc được trừ, số tiền phải thu hoặc hoàn lại.

Hiển thị bảng tổng hợp để lễ tân kiểm tra.

Lễ tân bấm Thanh toán. Ứng dụng mô phỏng thanh toán thành công ngay, không gọi cổng thanh toán; đồng thời lưu Invoice và Payment trong CSDL. Stay chuyển sang CHECKOUT_PENDING để khóa việc ghi thêm dịch vụ trong lúc chờ hoàn tất.

Lễ tân bấm In hóa đơn. Ứng dụng hiển thị thông báo hoàn thành và lưu thời điểm thao tác in mô phỏng.

Sau khi thao tác in mô phỏng thành công, hệ thống chuyển Stay sang COMPLETED, Reservation (nếu có) sang COMPLETED, Room sang CLEANING. Nếu ứng dụng bị đóng sau thanh toán nhưng trước khi in, hóa đơn vẫn được lưu; người dùng có thể mở lại hóa đơn để tiếp tục thao tác in và hoàn tất trả phòng.

Khi nhân viên dọn xong, thao tác Hoàn tất dọn phòng chuyển Room sang AVAILABLE, hoặc RESERVED nếu phòng đã được gán cho một đặt phòng khác còn hiệu lực.


Quy tắc:



Thời điểm check-out phải lớn hơn thời điểm check-in.

Không cho phép trả phòng hai lần cho cùng một Stay.

Hóa đơn chỉ được tạo một lần cho một Stay.

Cần dùng SQL transaction cho từng bước: (1) tạo hóa đơn + ghi thanh toán + chuyển Stay sang CHECKOUT_PENDING; (2) sau khi in mô phỏng, cập nhật PrintedAt + chuyển Stay/Reservation/Room sang trạng thái hoàn tất tương ứng.

Nút thanh toán chỉ mô phỏng giao dịch nội bộ, không đại diện cho một giao dịch ngân hàng thực tế.

Nút in không yêu cầu máy in và không phát sinh kết nối mạng; chỉ hiển thị thông báo thành công theo yêu cầu.




### 6.9. F09 – Hóa đơn

Thông tin tối thiểu trên hóa đơn:



Mã hóa đơn, ngày lập, mã phòng, thông tin khách.

Thời điểm nhận phòng và trả phòng.

Cách tính tiền, số giờ/ngày và tiền phòng.

Danh sách dịch vụ, số lượng, đơn giá và thành tiền.

Tổng tiền phòng, tổng tiền dịch vụ, tiền cọc được trừ.

Số tiền cần thu thêm hoặc số tiền cần hoàn lại.

Trạng thái thanh toán và trạng thái in mô phỏng.


Hóa đơn đã thanh toán không được sửa trực tiếp. Nếu nhập sai dữ liệu trước khi hoàn tất, quay lại màn hình tính checkout để điều chỉnh trước khi xác nhận thanh toán.





### 6.10. F10 – Dọn phòng

- Danh sách phòng `CLEANING` được hiển thị riêng hoặc lọc trên sơ đồ phòng.
- Nhân viên chọn phòng, kiểm tra và bấm **Hoàn tất dọn phòng**.
- Hệ thống cập nhật trạng thái `AVAILABLE`.
- Không yêu cầu quản lý checklist vệ sinh, người dọn, vật tư hoặc thời gian làm vệ sinh trong MVP.

### 6.11. F11 – Báo cáo tháng

Báo cáo có bộ lọc tháng/năm và tối thiểu ba chỉ số:



Công suất phòng (%): đề xuất tính bằng Tổng số giờ phòng có khách trong tháng / (Số phòng đang kinh doanh × Tổng số giờ trong tháng) × 100. Thời gian Stay giao với tháng báo cáo chỉ tính phần giao nhau; tính cả Stay IN_PROGRESS, CHECKOUT_PENDING và COMPLETED có khoảng lưu trú giao với tháng. Cách tính giờ giúp xử lý cả lưu trú ngắn theo giờ.

Doanh thu phòng: tổng RoomCharge của các hóa đơn đã thanh toán và đã hoàn tất thao tác in mô phỏng trong tháng (Invoice.Status = PAID, PrintedAt khác null).

Doanh thu dịch vụ: tổng ServiceCharge của các hóa đơn đã thanh toán và đã hoàn tất thao tác in mô phỏng trong tháng.


Quy ước: tháng báo cáo dựa trên ngày tạo/hoàn tất hóa đơn; công suất dựa trên thời gian Stay thực tế giao với tháng được chọn. Tiền cọc là khoản thanh toán trước, không phải khoản giảm doanh thu; chỉ được trừ khi tính số tiền cần thu tại checkout.


Giao diện nên có thẻ tổng số, bảng tổng hợp; biểu đồ cột hoặc đường là phần trình bày bổ sung, không thay thế số liệu.

## 7. Quy tắc trạng thái nghiệp vụ

### 7.1. Trạng thái phòng

```text
AVAILABLE ── đặt phòng gán phòng cụ thể ──> RESERVED
AVAILABLE ── khách vãng lai check-in ─────> OCCUPIED
RESERVED  ── check-in đúng đặt phòng ─────> OCCUPIED
OCCUPIED  ── xác nhận thanh toán ─────────> OCCUPIED (Stay: CHECKOUT_PENDING)
OCCUPIED  ── in mô phỏng hoàn tất ────────> CLEANING
CLEANING  ── xác nhận dọn xong ───────────> RESERVED nếu còn đặt trước, nếu không thì AVAILABLE
RESERVED  ── hủy đặt phòng cuối cùng ─────> AVAILABLE
```

Không cho phép chuyển trạng thái tùy ý nếu chưa qua nghiệp vụ tương ứng.

### 7.2. Trạng thái đặt phòng

| Trạng thái | Ý nghĩa |
|---|---|
| `CONFIRMED` | Đặt phòng đã được lưu và còn hiệu lực. |
| `CHECKED_IN` | Khách đã nhận phòng. |
| `COMPLETED` | Khách đã trả phòng. |
| `CANCELLED` | Đặt phòng đã hủy. |
| `NO_SHOW` | Khách không đến; có thể triển khai nếu còn thời gian, không bắt buộc MVP. |

### 7.3. Trạng thái lưu trú

- `IN_PROGRESS`: khách đang ở và có thể ghi nhận dịch vụ.
- `CHECKOUT_PENDING`: đã xác nhận thanh toán, đang chờ thao tác in mô phỏng để hoàn tất trả phòng; không cho phép thêm dịch vụ.
- `COMPLETED`: đã hoàn tất thao tác checkout/in mô phỏng.

### 7.4. Trạng thái hóa đơn và thanh toán

- `UNPAID`: hóa đơn đã được tạo nhưng chưa xác nhận thanh toán.
- `PAID`: đã xác nhận thanh toán mô phỏng.
- `PaymentStatus`: `COMPLETED` cho giao dịch mô phỏng thành công; không dùng trạng thái phản hồi từ cổng thanh toán ngoài.

---

## 8. Validation và xử lý lỗi

Validation được thực hiện ở hai mức, theo nội dung WinForms đã học:

- **Field-level:** kiểm tra khi rời ô nhập hoặc thay đổi giá trị; hiển thị lỗi gần trường bằng `ErrorProvider`.
- **Form-level:** kiểm tra toàn bộ dữ liệu khi người dùng bấm Lưu/Check-in/Thanh toán.

### 8.1. Quy tắc dữ liệu cơ bản

| Dữ liệu | Validation |
|---|---|
| Mã phòng | Bắt buộc, không trùng, giới hạn độ dài hợp lý. |
| Tầng / loại phòng | Phải chọn bản ghi đang hoạt động. |
| Giá giờ/ngày | Không âm; dùng `decimal`, không dùng `float`/`double` cho tiền. |
| Sức chứa | Số nguyên lớn hơn 0. |
| Họ tên khách | Bắt buộc, loại bỏ khoảng trắng thừa. |
| CCCD | Nếu chọn CCCD, đề xuất kiểm tra đúng 12 chữ số. |
| Hộ chiếu | Nếu chọn hộ chiếu, kiểm tra độ dài và ký tự chữ/số theo quy tắc đơn giản của đồ án. |
| Số điện thoại | Bắt buộc; chỉ chứa chữ số (có thể bỏ dấu cách/gạch trước khi kiểm tra), độ dài hợp lệ theo quy tắc nhóm thống nhất. |
| Email | Không bắt buộc; nếu có thì kiểm tra định dạng cơ bản. |
| Ngày đến/đi | Ngày đi phải sau ngày đến. |
| Đặt phòng | Phòng được chỉ định phải cùng loại và không trùng lịch đặt/lưu trú. |
| Check-in | Phòng phải hợp lệ; số khách không vượt sức chứa; không có Stay đang mở khác trong phòng. |
| Dịch vụ | Số lượng > 0; đơn giá >= 0; Stay phải đang `IN_PROGRESS`. |
| Check-out | Checkout > Check-in; chỉ checkout Stay đang `IN_PROGRESS`; chưa có hóa đơn hoàn tất. |
| Tiền cọc | Không âm; số tiền được trừ không vượt tổng tiền phải thanh toán. |

Các mẫu Regex chỉ là kiểm tra định dạng đầu vào, không thay thế kiểm tra nghiệp vụ hoặc xác minh giấy tờ thật.

### 8.2. Xử lý Exception

- Bọc các thao tác dễ phát sinh lỗi (CSDL, file, chuyển đổi dữ liệu) trong `try-catch` phù hợp.
- Thông báo lỗi dễ hiểu cho người dùng; không hiển thị toàn bộ stack trace.
- Ghi log chi tiết lỗi vào tệp cục bộ nếu cần phục vụ debug.
- Dùng `using`/`await using` để đóng `SqlConnection`, `SqlCommand`, `SqlDataReader` và giải phóng tài nguyên.
- `finally` chỉ dùng khi cần dọn dẹp thủ công; ưu tiên `using`.
- Không dùng `catch (Exception) { }` để nuốt lỗi.
- Xử lý riêng lỗi trùng khóa, vi phạm khóa ngoại, mất kết nối CSDL và lỗi dữ liệu không hợp lệ.

### 8.3. Quy tắc SQL an toàn

- Tất cả truy vấn nhận dữ liệu nhập từ Form phải dùng `SqlParameter`.
- Không nối chuỗi trực tiếp từ TextBox vào câu SQL.
- Dùng transaction cho các thao tác cập nhật nhiều bảng trong một nghiệp vụ.
- Không để Form tự mở kết nối SQL riêng lẻ.

---

## 9. Quy ước code và giao diện

### 9.1. Đặt tên

| Thành phần | Quy tắc | Ví dụ |
|---|---|---|
| Class / Form | PascalCase | `RoomManagementForm`, `CheckOutService` |
| Method | PascalCase, bắt đầu bằng động từ | `CreateReservation()`, `CalculateInvoice()` |
| Biến / parameter | camelCase | `roomId`, `expectedCheckOut` |
| Interface | Bắt đầu bằng I | `IRoomRepository` |
| Enum | PascalCase; giá trị PascalCase hoặc UPPER_SNAKE_CASE, thống nhất toàn dự án | `RoomStatus.Available` |
| Hằng số | PascalCase hoặc UPPER_SNAKE_CASE, thống nhất | `MaxGuestCount` |
| SQL table/column | PascalCase hoặc snake_case, chọn một chuẩn; đề xuất PascalCase | `Room`, `RoomId`, `RoomCode` |

### 9.2. Quy ước WinForms

- Tách Form theo màn hình/nghiệp vụ; màn hình có danh sách dùng `DataGridView`.
- Tái sử dụng `UserControl`/Control chung cho nút, ô tìm kiếm, thẻ phòng, vùng chú giải trạng thái.
- Không đặt toàn bộ nghiệp vụ trong `btnSave_Click`.
- Event Handler chỉ lấy dữ liệu từ giao diện, gọi Service, cập nhật binding và hiển thị thông báo.
- Khi truy vấn có thể mất thời gian, dùng `async/await` để tránh khóa UI Thread.
- Dùng `BindingSource` hoặc `BindingList<T>` cho dữ liệu cần đồng bộ giao diện; dùng `DataTable` khi phù hợp với dữ liệu dạng bảng.
- Dùng `DateTimePicker`, `NumericUpDown`, `ComboBox` thay vì bắt người dùng nhập ngày/số dưới dạng chuỗi khi có thể.
- Chuẩn hóa cách hiển thị tiền theo VND và ngày giờ theo định dạng Việt Nam.
- Giao diện sử dụng tiếng Việt có dấu, nhãn và thông báo nhất quán.

### 9.3. Xử lý dữ liệu và tiền

- Dùng `decimal` trong C# và `DECIMAL(18,0)` trong SQL Server cho tiền VND của đồ án.
- Không tính tiền bằng `double`.
- Không xóa cứng dữ liệu danh mục đã được sử dụng; dùng `IsActive`.
- Lưu giá dịch vụ tại thời điểm phát sinh và giá phòng tại thời điểm check-in để lịch sử hóa đơn không bị thay đổi khi cập nhật bảng giá.

---

## 10. Phân chia công việc cho 3 thành viên

### 10.1. Nguyên tắc phân chia

- Phân chia theo **module nghiệp vụ hoàn chỉnh**, không chia một người làm giao diện còn một người làm SQL cho cùng một màn hình.
- Mỗi thành viên phụ trách đồng bộ Form + Service + Repository của phạm vi được giao.
- `Core/Entities`, enum, schema SQL và hợp đồng giữa các Service phải được thống nhất trước khi tách nhánh làm song song.
- Có một người tích hợp solution/installer; nhiệm vụ này không có nghĩa người đó làm thay nghiệp vụ của hai thành viên còn lại.
- Các con số khối lượng dưới đây là ước lượng để cân bằng, không phải thời gian bắt buộc.

### 10.2. Phân công đề xuất

Phân công theo đúng 20 màn hình đã đánh mã ở mục 5.2. Mỗi thành viên chịu trách nhiệm trọn module: giao diện, Service, Repository và validation trong phạm vi được giao.

| Thành viên | Màn hình phụ trách | Module / nghiệp vụ | Phần code chính | Khối lượng ước lượng |
|---|---|---|---|---:|
| **Thành viên 1** - [Tên người phụ trách]| **S01–S06, S19** (7 màn hình) | Tổng quan/sơ đồ phòng; quản lý tầng; danh sách và biểu mẫu loại phòng; danh sách và biểu mẫu phòng; dọn phòng. | `Forms/Dashboard`, `Forms/Floors`, `Forms/RoomTypes`, `Forms/Rooms`, `Forms/Housekeeping`; `RoomService`, `FloorService`, `RoomTypeService`; `RoomRepository`, `FloorRepository`, `RoomTypeRepository`. Đồng thời khởi tạo solution, cấu hình kết nối và nền tảng database, điều phối tích hợp. | **34%** (F01, F02, F10)|
| **Thành viên 2** - [Bùi Hữu Trí]| **S07–S12** (6 màn hình) | Danh sách/biểu mẫu khách hàng; danh sách/biểu mẫu đặt phòng; check-in; danh sách lưu trú đang diễn ra. | `Forms/Customers`, `Forms/Reservations`, `Forms/CheckIn`; `CustomerService`, `ReservationService`, `CheckInService`; `CustomerRepository`, `ReservationRepository`, `StayRepository` (phần tạo và truy vấn lượt lưu trú). | **33%** (F03, F04, F05)|
| **Thành viên 3** - [Tên người phụ trách]| **S13–S18, S20** (7 màn hình) | Danh mục và biểu mẫu dịch vụ; ghi nhận dịch vụ; check-out/thanh toán; danh sách/chi tiết hóa đơn; báo cáo tháng. | `Forms/Services`, `Forms/CheckOut`, `Forms/Invoices`, `Forms/Reports`; `ServiceCatalogService`, `ServiceUsageService`, `CheckOutService`, `InvoiceService`, `ReportService`; `ServiceRepository`, `StayServiceRepository`, `InvoiceRepository`, `PaymentRepository`, `ReportRepository`. | **33%** (F06, F07, F08, F09, F11)|

**Lý do phân chia:**

- Thành viên 1 phụ trách dữ liệu nền về cấu trúc khách sạn và trạng thái phòng. S01, S02–S06 và S19 dùng chung dữ liệu `Floor`, `RoomType`, `Room`, nên có mức liên kết cao; việc giao cùng một người giảm xung đột khi cập nhật trạng thái phòng.
- Thành viên 2 phụ trách trọn luồng trước và tại thời điểm khách nhận phòng: hồ sơ khách → đặt phòng → check-in → danh sách lượt lưu trú đang mở. Đây là nhóm có logic kiểm tra trùng lịch và sức chứa tương đối phức tạp.
- Thành viên 3 phụ trách luồng phát sinh doanh thu: danh mục dịch vụ → ghi dịch vụ → checkout → hóa đơn/thanh toán; S20 đọc dữ liệu hóa đơn và lượt lưu trú để tổng hợp báo cáo.
- Số màn hình được chia theo tỷ lệ **7–6–7**. Khối lượng ước lượng không chỉ dựa trên số màn hình mà còn xét độ phức tạp của nghiệp vụ, số bảng dữ liệu và trách nhiệm tích hợp.

### 10.3. Ranh giới trách nhiệm và phụ thuộc

| Hạng mục | Người làm chính | Người phối hợp |
|---|---|---|
| Solution, project references, cấu hình chung và `MainForm` | Thành viên 1 | Cả nhóm review trước khi thêm package hoặc đổi cấu trúc |
| SQL schema và script khởi tạo ban đầu | Thành viên 1 điều phối | Thành viên 2 và 3 rà soát các bảng/khóa ngoại liên quan module của mình |
| Entity và enum chung | Cả nhóm thống nhất; thành viên 1 tích hợp | Thành viên 2/3 không tự đổi tên/kiểu trường đã chốt |
| S01–S06 và S19: giao diện, Service, Repository | Thành viên 1 | Thành viên 2/3 cung cấp yêu cầu trạng thái phòng và dữ liệu liên quan |
| S07–S12: khách hàng, đặt phòng, check-in, Stay | Thành viên 2 | Thành viên 1 phối hợp quy tắc phòng; thành viên 3 thống nhất cấu trúc `Stay` và tiền cọc |
| S13–S18: dịch vụ, checkout, hóa đơn | Thành viên 3 | Thành viên 2 cung cấp hợp đồng `Stay`; thành viên 1 phối hợp cập nhật trạng thái phòng khi checkout hoàn tất |
| S20: báo cáo tháng | Thành viên 3 | Thành viên 1 cung cấp dữ liệu phòng/tổng số phòng; thành viên 2 kiểm tra dữ liệu Stay; thành viên 3 chịu trách nhiệm truy vấn tổng hợp |
| Điều hướng và component dùng chung | Thành viên 1 tích hợp | Mỗi thành viên đăng ký màn hình, tên điều khiển và luồng mở màn hình theo mã S01–S20 |
| Kiểm thử tích hợp toàn hệ thống | Cả nhóm | Mỗi người kiểm thử màn hình và nghiệp vụ trong phạm vi được giao |
| Publish và Inno Setup | Thành viên 1 tích hợp | Thành viên 2/3 kiểm tra luồng nghiệp vụ trên bản cài đặt máy sạch |

Mỗi màn hình chỉ có **một người chịu trách nhiệm chính**. Người khác có thể review hoặc cung cấp hợp đồng dữ liệu, nhưng không tự sửa trực tiếp các file giao diện/Service/Repository thuộc module của thành viên phụ trách nếu chưa thống nhất.

### 10.4. Các hợp đồng Service cần thống nhất sớm

Đây là hợp đồng ở mức ý tưởng để tránh các module gọi chéo tùy tiện; nhóm có thể điều chỉnh tên method nhưng phải thống nhất trước khi code.

| Service | Phương thức chính | Kết quả |
|---|---|---|
| `FloorService` | `GetFloors()`, `CreateFloor()`, `UpdateFloor()`, `DeactivateFloor()` | S02: quản lý tầng. |
| `RoomTypeService` | `GetRoomTypes()`, `CreateRoomType()`, `UpdateRoomType()` | S03–S04: danh sách và biểu mẫu loại phòng. |
| `RoomService` | `GetRoomsByFloor()`, `GetRoomStatus()`, `CreateRoom()`, `UpdateRoom()`, `CompleteCleaning()` | S01, S05–S06, S19: sơ đồ, danh sách/biểu mẫu phòng và xác nhận phòng sẵn sàng. |
| `CustomerService` | `SearchCustomers()`, `CreateCustomer()`, `UpdateCustomer()` | S07–S08: hồ sơ khách đã lưu. |
| `ReservationService` | `CheckAvailability()`, `CreateReservation()`, `CancelReservation()` | S09–S10: kết quả còn phòng, thông tin đặt phòng. |
| `CheckInService` | `CheckInReservation()`, `CheckInWalkIn()` | S11–S12: `Stay` mới và danh sách lượt lưu trú đang mở. |
| `ServiceCatalogService` | `GetServices()`, `CreateService()`, `UpdateService()`, `DeactivateService()` | S13–S14: danh mục và biểu mẫu dịch vụ. |
| `ServiceUsageService` | `AddServiceUsage()`, `GetStayServices()` | S15: dòng dịch vụ phát sinh và tổng dịch vụ. |
| `CheckOutService` | `CalculateCheckout()`, `CompleteCheckout()` | S16: bảng tính tiền và kết quả checkout. |
| `InvoiceService` | `GetInvoice()`, `ConfirmPayment()`, `MarkPrinted()` | S17–S18: hóa đơn, giao dịch mô phỏng, trạng thái in. |
| `ReportService` | `GetMonthlyReport()` | S20: công suất, doanh thu phòng và doanh thu dịch vụ. |

### 10.5. Những phần không được tự ý sửa khi chưa báo nhóm

- Tên Entity, tên thuộc tính, kiểu dữ liệu, khóa chính/khóa ngoại.
- Giá trị enum trạng thái và quy tắc chuyển trạng thái.
- Công thức tiền phòng, tiền dịch vụ, tiền cọc và công suất.
- Các method Service mà module khác đang gọi.
- Cấu trúc solution, connection string mẫu và script tạo bảng.

Nếu cần thay đổi, cập nhật file mô tả Entity và đặc tả nghiệp vụ trong cùng commit/nhánh.

---

## 11. Quy trình làm việc nhóm với Git và SmartGit

### 11.1. Nhánh

- Không commit trực tiếp lên `main`.
- Một nhánh chỉ xử lý một nhóm thay đổi liên quan.
- Trước khi làm việc, fetch/pull `main` hoặc `Develop` để đồng bộ code mới nhất về máy, tránh lệch code quá xa ; trước khi merge, cập nhật nhánh với thay đổi mới nhất.

#### 11.1.1. Cấu trúc nhánh

| Nhánh | Mục đích | Quy tắc |
|---|---|---|
| `main` | Phiên bản ổn định, dùng để nộp hoặc phát hành | Không commit trực tiếp; chỉ nhận thay đổi đã được kiểm tra từ `develop`. |
| `develop` | Nhánh tích hợp chung của cả nhóm | Nhận Pull Request từ các nhánh chức năng sau khi review và build thành công. |
| `feature/<Fxx>-<ten-chuc-nang>` | Phát triển một chức năng nghiệp vụ | Tạo từ `develop`; hoàn thành thì mở Pull Request về `develop`. |
| `fix/<Fxx>-<ten-loi>` | Sửa một lỗi cụ thể đã xác định | Phạm vi sửa hẹp; không gom thêm chức năng mới. |


#### 11.1.2. Quy tắc chia nhỏ chức năng

- Một nhánh nên tương ứng với **một đầu việc có kết quả nghiệp vụ rõ ràng**, thường là một chức năng F hoặc một phần độc lập của F lớn.
- Một nhánh chức năng nên hoàn thiện theo chiều dọc: giao diện liên quan (nếu có) → Service → Repository/SQL → validation → kiểm thử. Không chia riêng một nhánh chỉ làm Form, một nhánh khác chỉ làm Service và một nhánh khác chỉ làm Repository cho cùng một chức năng, vì sẽ làm tăng phụ thuộc khi tích hợp.
- Không bắt buộc mỗi màn hình là một nhánh. Các màn hình danh sách và biểu mẫu có cùng nghiệp vụ có thể nằm trong một nhánh; chỉ tách khi chúng có thể hoàn thành, review và kiểm thử độc lập.
- Với F có phạm vi lớn, được phép tách thành nhiều nhánh nhỏ. Ví dụ F02 gồm quản lý tầng, loại phòng và phòng nên tách thành ba đầu việc; không gom tất cả vào một nhánh `feature/room-management`.
- Không tạo nhánh chỉ để sửa một vài dòng hoặc một control nhỏ nếu thay đổi đó thuộc cùng đầu việc đang thực hiện; tiếp tục commit trên nhánh hiện tại.
- Mỗi nhánh chỉ có một người chịu trách nhiệm chính. Thành viên khác có thể review hoặc phối hợp, nhưng phải thống nhất trước khi cùng sửa các file dùng chung.

#### 11.1.3. Danh sách nhánh chức năng đề xuất

Danh sách dưới đây dựa theo phân công tại mục 10.2 và mã chức năng F / màn hình S đã thống nhất. Đây là các nhánh có thể tạo lần lượt khi bắt đầu từng đầu việc, **không cần tạo tất cả ngay từ đầu**.

| Người phụ trách | Phạm vi chức năng / màn hình | Tên nhánh đề xuất |
|---|---|---|
| Thành viên 1 | F01 – S01: Sơ đồ phòng theo tầng | `feature/F01-room-dashboard` |
| Thành viên 1 | F02 – S02: Quản lý tầng | `feature/F02-floor-management` |
| Thành viên 1 | F02 – S03–S04: Quản lý loại phòng | `feature/F02-room-type-management` |
| Thành viên 1 | F02 – S05–S06: Quản lý phòng | `feature/F02-room-management` |
| Thành viên 1 | F10 – S19: Dọn phòng | `feature/F10-housekeeping` |
| Thành viên 2 | F03 – S07–S08: Quản lý khách hàng | `feature/F03-customer-management` |
| Thành viên 2 | F04 – S09–S10: Đặt phòng trước | `feature/F04-reservation-management` |
| Thành viên 2 | F05 – S11–S12: Check-in và danh sách lưu trú | `feature/F05-checkin-stays` |
| Thành viên 3 | F06 – S13–S14: Danh mục dịch vụ | `feature/F06-service-catalog` |
| Thành viên 3 | F07 – S15: Dịch vụ phát sinh | `feature/F07-stay-service` |
| Thành viên 3 | F08 – S16: Check-out và tính tiền | `feature/F08-checkout` |
| Thành viên 3 | F09 – S17–S18: Danh sách, chi tiết và in hóa đơn | `feature/F09-invoice` |
| Thành viên 3 | F11 – S20: Báo cáo tháng | `feature/F11-monthly-report` |

Các nhánh nền tảng có thể được tạo trước khi triển khai nghiệp vụ:

- `chore/solution-bootstrap`: tạo solution, project references, cấu hình WinForms và các project nền.
- `chore/database-foundation`: tạo schema SQL, khóa chính/ngoại, dữ liệu mẫu và cấu hình kết nối ban đầu.
- `chore/shared-contracts`: thống nhất Entity, Enum và hợp đồng Service/Repository dùng chung.

Các nhánh nền tảng cần được review và merge vào `develop` trước khi các nhánh chức năng phụ thuộc vào chúng được tạo. Thành viên 1 điều phối, thành viên 2 và 3 phải rà soát phần liên quan đến nghiệp vụ của mình trước khi merge.

#### 11.1.4. Vòng đời của một nhánh chức năng

1. Xác định đầu việc cụ thể trong bảng theo dõi tại mục 10.4; ghi rõ mã F, màn hình S và người phụ trách.
2. Checkout và Pull nhánh `develop` mới nhất.
3. Tạo một nhánh `feature/Fxx-ten-chuc-nang` từ `develop`.
4. Chỉ sửa các file cần thiết cho đầu việc đó; commit thành các bước nhỏ, có nội dung rõ ràng.
5. Build và kiểm thử chức năng; kiểm tra Diff để chắc chắn không đưa file cá nhân, database runtime hoặc thay đổi ngoài phạm vi vào commit.
6. Push nhánh và tạo Pull Request về `develop`; mô tả PR phải ghi mã F, mã S, nội dung hoàn thành, cách kiểm thử và phần còn thiếu (nếu có).
7. Sau khi được review và merge, cập nhật trạng thái chức năng tương ứng trong mục 10.4; xóa nhánh chức năng đã hoàn tất nếu không còn cần thiết.

Trước khi tạo nhánh mới hoặc mở Pull Request, cần fetch để cập nhật thông tin remote. Khi nhánh đang làm bị chậm so với `develop`, cập nhật nhánh bằng cách merge hoặc rebase theo quy ước nhóm đã chọn; không tự ý force-push lên nhánh dùng chung.

### 11.2. Quy tắc commit

Mẫu: `<type>: <mô tả ngắn>`.

| Type | Khi dùng | Ví dụ |
|---|---|---|
| `feat` | Thêm chức năng | `feat: thêm sơ đồ phòng theo tầng` |
| `fix` | Sửa lỗi | `fix: chặn check-in phòng đang dọn` |
| `refactor` | Tái cấu trúc không đổi nghiệp vụ | `refactor: tách logic tính tiền khỏi form` |
| `docs` | Cập nhật tài liệu | `docs: bổ sung entity đặt phòng` |
| `test` | Thêm/sửa kiểm thử | `test: kiểm tra tính tiền theo ngày` |

### 11.3. Quy trình trên SmartGit

1. Clone repository về máy.
2. Checkout `main` và Pull trước khi tạo nhánh.
3. Tạo nhánh theo module.
4. Chỉ Stage các file thuộc thay đổi của nhánh; xem Diff trước khi Commit.
5. Commit với thông điệp theo quy ước.
6. Push nhánh lên remote.
7. Tạo Pull Request/Merge Request hoặc gửi nhóm review trước khi merge vào nhánh trung gian `Develop`.
8. Nhóm trưởng review, kiểm tra conflit trước khi merge từ `Develop` vào `main`
9. Sau khi merge, checkout `main` hoặc `Develop` tùy thuộc  và Pull lại.

### 11.4. Không commit

- `.idea/`, file cấu hình cá nhân của Rider nếu chứa đường dẫn riêng.
- `bin/`, `obj/`, thư mục publish và file cài đặt build ra (trừ khi nhóm quyết định phát hành Release riêng).
- File database thật `.mdf`, `.ldf` có dữ liệu cá nhân; chỉ commit script SQL và dữ liệu mẫu không nhạy cảm.
- Password, connection string chứa mật khẩu, file cấu hình local riêng.
- Log runtime và file tạm.

---

## 12. Kiểm thử và tiêu chí nghiệm thu

### 12.1. Test case nghiệp vụ tối thiểu

| ID | Tình huống | Dữ liệu / thao tác | Kết quả mong đợi |
|---|---|---|---|
| TC01 | Thêm phòng hợp lệ | Mã phòng mới, tầng và loại phòng hợp lệ | Lưu thành công; phòng xuất hiện trên sơ đồ. |
| TC02 | Trùng mã phòng | Thêm phòng có mã đã tồn tại | Báo lỗi trùng; không tạo bản ghi mới. |
| TC03 | Giá phòng âm | Nhập giá giờ hoặc giá ngày âm | Validation chặn lưu. |
| TC04 | Đặt phòng hợp lệ | Ngày đi sau ngày đến, loại phòng còn chỗ | Tạo đặt phòng trạng thái `CONFIRMED`. |
| TC05 | Đặt phòng sai khoảng thời gian | Ngày đi nhỏ hơn hoặc bằng ngày đến | Báo lỗi; không lưu. |
| TC06 | Trùng lịch cùng phòng | Hai đặt phòng gán một phòng trong thời gian giao nhau | Từ chối đặt phòng thứ hai. |
| TC07 | Check-in phòng trống | Chọn phòng `AVAILABLE`, khách hợp lệ | Tạo Stay; phòng chuyển `OCCUPIED`. |
| TC08 | Check-in phòng đang dọn | Chọn phòng `CLEANING` | Không cho check-in. |
| TC09 | Check-in vượt sức chứa | Số khách lớn hơn sức chứa | Báo lỗi; không tạo Stay. |
| TC10 | Ghi dịch vụ | Stay đang mở, số lượng dương | Lưu dịch vụ và đơn giá chụp tại thời điểm ghi nhận. |
| TC11 | Ghi dịch vụ cho Stay đã đóng | Chọn Stay `COMPLETED` | Từ chối ghi thêm. |
| TC12 | Tính tiền theo giờ | Có thời gian check-in/out và giá giờ | Tiền phòng khớp công thức đã thống nhất. |
| TC13 | Tính tiền theo ngày | Thời gian lưu trú có phần lẻ ngày | Làm tròn lên theo quy tắc đã thống nhất. |
| TC14 | Trừ tiền cọc | Có tiền cọc nhỏ hơn tổng tiền | Số tiền phải thu bằng tổng tiền trừ cọc. |
| TC15 | Cọc lớn hơn tổng | Deposit lớn hơn SubTotal | Số tiền phải thu bằng 0; hiển thị số tiền cần hoàn lại. |
| TC16 | Thanh toán mô phỏng | Bấm Thanh toán trên bảng tính hợp lệ | Ghi hóa đơn/payment; không gọi mạng. |
| TC17 | In mô phỏng | Bấm In hóa đơn | Hiển thị thông báo hoàn thành; lưu thời điểm in. |
| TC18 | Hoàn tất checkout | Checkout hoàn tất | Stay `COMPLETED`, phòng `CLEANING`. |
| TC19 | Hoàn tất dọn phòng | Chọn phòng `CLEANING`, xác nhận dọn xong | Phòng chuyển `AVAILABLE`, hoặc `RESERVED` nếu còn đặt phòng gán trước có hiệu lực. |
| TC20 | Báo cáo tháng | Chọn tháng có dữ liệu | Hiển thị công suất, doanh thu phòng và dịch vụ đúng theo dữ liệu mẫu. |
| TC21 | Mất kết nối CSDL | Tắt LocalDB hoặc cấu hình sai | Thông báo không kết nối được; ứng dụng không crash đột ngột. |
| TC22 | Khởi động lần đầu | Máy có LocalDB nhưng chưa có database | Chương trình khởi tạo database/schema/seed theo quy trình đã thiết kế. |
| TC23 | Kiểm tra đủ 20 màn hình | Mở lần lượt các mục điều hướng S01–S20 | Cả 20 màn hình đều mở được, hiển thị đúng tiêu đề/nội dung; không tính MainForm và popup. |
| TC24 | Điều hướng danh sách – biểu mẫu | Từ S03/S05/S07/S09/S13 mở màn hình thêm-sửa tương ứng, sau đó Lưu hoặc Hủy | Điều hướng đúng màn hình; dữ liệu được lưu hoặc hủy đúng; quay lại danh sách và tải lại dữ liệu. |

### 12.2. Tiêu chí nghiệm thu tổng thể

- Có đủ **20 màn hình nghiệp vụ S01–S20** theo danh sách tại mục 5.2; các màn hình có thể điều hướng và luồng mở/quay lại hoạt động đúng. `MainForm` và các hộp thoại nhỏ không tính vào số lượng này.
- Ứng dụng khởi động được trên Windows bằng file cài đặt.
- Không cần Internet để mở ứng dụng và thực hiện nghiệp vụ.
- Kết nối được SQL Server cục bộ và dữ liệu còn sau khi tắt/mở lại ứng dụng.
- Sơ đồ phòng thể hiện đúng bốn màu trạng thái.
- Không thể check-in phòng đang có khách hoặc đang dọn.
- Có thể đặt trước, check-in, ghi dịch vụ, checkout và lưu hóa đơn xuyên suốt.
- Công thức tiền và báo cáo đã được nhóm kiểm chứng bằng dữ liệu mẫu.
- Validation chặn được dữ liệu rỗng, sai định dạng, sai khoảng thời gian và sai trạng thái.
- Lỗi CSDL được thông báo rõ; không làm ứng dụng đóng đột ngột.
- Các nút Thanh toán và In hoạt động theo mô phỏng offline đã mô tả.

---

## 13. Build, Publish và tạo file cài đặt

### 13.1. Điều kiện triển khai

- Hệ điều hành Windows tương thích với WinForms.
- Nếu dùng bản cài đặt self-contained thì không cần cài .NET Desktop Runtime riêng; runtime .NET được đóng gói cùng ứng dụng.
- Máy triển khai vẫn cần SQL Server Express LocalDB (hoặc SQL Server Express cục bộ) vì self-contained .NET **không bao gồm SQL Server Database Engine**.
- Không cần SQL Server Management Studio trên máy người dùng cuối nếu ứng dụng tự khởi tạo database và không yêu cầu người dùng quản trị SQL.

### 13.2. Publish ứng dụng

Tại thư mục chứa solution hoặc project, chạy:

```powershell
dotnet restore
dotnet build -c Release
dotnet publish .\src\HotelManagement.WinForms\HotelManagement.WinForms.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o .\artifacts\publish
```

Kết quả nằm trong `artifacts/publish`. Đây là thư mục ứng dụng đã publish; có thể chạy thử `HotelManagement.WinForms.exe` trước khi đóng gói.

### 13.3. Đóng gói Installer

Dùng Inno Setup để tạo `HotelManagement_Setup.exe`:

- Copy toàn bộ nội dung `artifacts/publish` vào thư mục cài đặt, ví dụ `{autopf}\HotelManagement`.
- Tạo shortcut Start Menu/Desktop nếu người dùng chọn.
- Đưa script SQL khởi tạo vào thư mục dữ liệu hoặc resource phù hợp.
- Không ghi database đang hoạt động vào `Program Files`; dữ liệu runtime nên lưu tại thư mục dữ liệu người dùng, ví dụ `%LOCALAPPDATA%\HotelManagement\Data`.
- Kiểm tra LocalDB đã được cài. Nếu chưa, hiển thị hướng dẫn cài LocalDB hoặc tích hợp bộ cài LocalDB riêng theo điều kiện phân phối của Microsoft.
- Khi chạy lần đầu, ứng dụng kiểm tra database; nếu chưa có thì tạo database, tạo bảng và nạp dữ liệu khởi tạo.

**Phân biệt hai gói:**

1. `HotelManagement_Setup.exe`: bộ cài ứng dụng do nhóm tạo.
2. `SQL Server Express LocalDB`: thành phần Database Engine cần có riêng hoặc được đóng gói như prerequisite. Không được quảng bá rằng file Setup đã bao gồm SQL Server nếu nhóm chưa thực sự tích hợp bộ cài và kiểm thử phần đó.

### 13.4. Kiểm thử trên máy sạch

Trước khi nộp, thử trên máy ảo hoặc máy Windows khác:

1. Chưa có source code và chưa có Rider.
2. Cài LocalDB theo cách nhóm đã hướng dẫn.
3. Chạy `HotelManagement_Setup.exe`.
4. Mở ứng dụng; kiểm tra database tự khởi tạo.
5. Thử tạo khách, đặt phòng, check-in, ghi dịch vụ, checkout, thanh toán, in mô phỏng.
6. Đóng và mở lại ứng dụng; xác nhận dữ liệu còn.
7. Ngắt Internet/Wi-Fi; xác nhận toàn bộ chức năng vẫn hoạt động.

---

## 14. Lộ trình triển khai gợi ý

| Giai đoạn | Công việc | Kết quả cần có |
|---|---|---|
| 1. Chốt nền | Thống nhất Entity, enum, quy tắc trạng thái, công thức tính tiền, connection string, cấu trúc solution | Tài liệu Entity và project spec được nhóm xác nhận; solution build được. |
| 2. CSDL và khung giao diện | Tạo database scripts, Repository nền, MainForm, điều hướng, component chung và khung 20 màn hình S01–S20 | Mở được WinForms, kết nối LocalDB, điều hướng được đến các màn hình đã tạo. |
| 3. Danh mục và đặt phòng | Quản lý phòng/loại/tầng/dịch vụ; khách hàng; đặt phòng | CRUD và kiểm tra khả dụng hoạt động. |
| 4. Check-in và dịch vụ | Nhận phòng, chuyển trạng thái, ghi nhận dịch vụ | Tạo Stay và thêm dịch vụ chính xác. |
| 5. Check-out và báo cáo | Tính tiền, cọc, hóa đơn, thanh toán/in mô phỏng, báo cáo tháng | Hoàn tất vòng đời một lượt lưu trú và số liệu báo cáo. |
| 6. Tích hợp và đóng gói | Chạy test case, sửa lỗi, publish, tạo Setup, kiểm thử máy sạch | File cài đặt chạy offline và giữ dữ liệu. |

---

## 15. Các quyết định cần ghi nhớ

1. Đây là **ứng dụng WinForms offline**, có đủ 20 màn hình nghiệp vụ S01–S20 theo mục 5.2.
2. Dữ liệu lưu trong SQL Server cục bộ; không dùng dữ liệu giả trong bộ nhớ làm nơi lưu chính.
3. Dùng ADO.NET `Microsoft.Data.SqlClient`; tất cả SQL có tham số.
4. Các thao tác khách hàng do lễ tân nhập; không làm cổng khách hàng.
5. Thanh toán và in là mô phỏng, nhưng phải lưu trạng thái/kết quả trong database.
6. Phòng chỉ trở lại `AVAILABLE` sau khi hoàn tất dọn phòng và không còn đặt trước gán vào phòng; nếu còn đặt trước thì trạng thái là `RESERVED`.
7. Thành viên không tự ý đổi Entity, enum, công thức nghiệp vụ hoặc hợp đồng Service đã thống nhất.
8. Installer ứng dụng và Database Engine là hai thành phần khác nhau; phải kiểm thử prerequisite LocalDB trên máy cài mới.

---

## 16. Tài liệu tham khảo kỹ thuật

- Microsoft – Target frameworks: <https://learn.microsoft.com/en-us/dotnet/standard/frameworks> (WinForms dùng TFM nền tảng Windows như `net10.0-windows`).
- Microsoft – .NET application publishing: <https://learn.microsoft.com/en-us/dotnet/core/deploying> (framework-dependent và self-contained deployment).
- Microsoft – SQL Server Express LocalDB: <https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb> (cài đặt, instance và kết nối LocalDB).
- Microsoft – Microsoft.Data.SqlClient: <https://learn.microsoft.com/en-us/sql/connect/ado-net/microsoft-ado-net-sql-server> (ADO.NET Provider cho SQL Server).
- JetBrains – Windows Forms Designer: <https://www.jetbrains.com/help/rider/Working_with_Windows_Forms.html>.
- JetBrains – Rider 2024.3: <https://www.jetbrains.com/rider/whatsnew/2024-3/> (bổ sung WinForms Designer cho dự án .NET 6+).

