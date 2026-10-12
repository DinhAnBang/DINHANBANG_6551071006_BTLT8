# CÂU 1 – QUẢN LÝ HÀNG HÓA TIỆM TẠP HÓA UTC2

## 1. Mở và chạy

**Phương án đơn giản (project độc lập)**

1. Giải nén `CAU1_HoanChinh.zip` ra một thư mục mới (không giải nén đè bản cũ).
2. Mở `CAU1.slnx` bằng Visual Studio 2026.
3. Đảm bảo SQL Server LocalDB có database `TiemTapHoa` và bảng `dbo.HangHoa` chứa 6 dòng dữ liệu như đề. Nếu chưa có, chạy `Sql/CAU1_TiemTapHoa.sql` trong SSMS.
4. Đợi Visual Studio tự **Restore NuGet packages** (hoặc nhấn chuột phải Solution → Restore NuGet Packages).
5. Chọn Build → Rebuild Solution, rồi F5 chạy.

**Nếu bạn muốn tiếp tục trên project có EF Core Power Tools từ ảnh:**

- Sao lưu project hiện tại trước.
- Chép `Form1.cs`, `Form1.Designer.cs`, `Program.cs`, thư mục `Data`, `Reports` từ bản này vào project hiện tại.
- **Giữ nguyên** các file `Models/HangHoa.cs`, `Models/TiemTapHoaContext.cs` mà EF Core Power Tools đã scaffold. Không chép các file `Models` trong bản ZIP để tránh ghi đè code tự sinh. Bản ZIP có sẵn các model tương thích nhằm có thể chạy độc lập.
- Đảm bảo `CAU1.csproj` của project hiện tại có các `PackageReference` EF Core SqlServer, EF Core Design, ClosedXML và FastReport.Net.Demo (hoặc bản FastReport .NET WinForms được cấp phép, tương thích cùng API). Có thể tham khảo nội dung `CAU1.csproj` trong ZIP.
- Trong Properties của `Reports/BangGia.frx`, đặt **Copy to Output Directory = Copy if newer**, hoặc dùng `ItemGroup` trong csproj tương ứng.

## 2. Kết nối CSDL

Mặc định phần mềm dùng:

`Server=(localdb)\MSSQLLocalDB;Database=TiemTapHoa;Trusted_Connection=True;TrustServerCertificate=True;`

Nếu máy bạn dùng instance khác, sửa `Data/DbFactory.cs` đúng server đang dùng; không sửa tên database hoặc bảng nếu không cần thiết.

## 3. Các NuGet package phiên bản của bản ZIP

- Microsoft.EntityFrameworkCore.SqlServer 10.0.1
- Microsoft.EntityFrameworkCore.Design 10.0.1
- ClosedXML 0.105.0
- FastReport.Net.Demo 2026.2.8

**Lưu ý về FastReport:** `FastReport.OpenSource` thường dùng cho sinh báo cáo nhưng không cung cấp đầy đủ cửa sổ xem trước WinForms kiểu `Report.ShowPrepared()` như bản .NET WinForms. Vì vậy bản ZIP dùng gói `FastReport.Net.Demo` có chức năng xem trước và xuất PDF. Gói **Demo** có thể đóng dấu `Demo version` khi xem trước, in hoặc xuất PDF. Nếu trường có bản FastReport WinForms đầy đủ/giấy phép, nên dùng bản đó để không bị giới hạn demo. Đây là giới hạn bản quyền từ nhà sản xuất chứ không phải lỗi chương trình.

## 4. Các file quan trọng

- `CAU1.csproj` – .NET 10, thư viện NuGet, copy report.
- `Models/` – model và DbContext khớp bảng `HangHoa` (có thể thay bằng file EF Core Power Tools của bạn).
- `Data/DbFactory.cs` – connection string LocalDB.
- `Data/HangHoaRow.cs` – dữ liệu hiển thị DataGridView.
- `Form1.Designer.cs` – toàn bộ giao diện, 8 nút theo đề.
- `Form1.cs` – CRUD, debounce, lọc, report, Excel.
- `Reports/BangGia.frx` – mẫu bảng giá FastReport, DataTable `HangHoa`, tham số `NgayIn`.
- `Sql/CAU1_TiemTapHoa.sql` – script CSDL + 6 dữ liệu mẫu + truy vấn kiểm tra.

## 5. Kiểm thử bắt buộc để chụp hình nộp

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Mở form | 6 mặt hàng |
| 2 | Chọn từng hàng | Thông tin tương ứng điền vào ô nhập |
| 3 | Thử thêm tên trống / giá = 0 | Cảnh báo, không lưu |
| 4 | Thêm, sửa, xóa một mặt hàng thử | CSDL và lưới cập nhật; xóa hỏi Yes/No |
| 5 | Tìm `nước` | 2 dòng |
| 6 | Tích chỉ còn kinh doanh | 1 dòng Aquafina |
| 7 | Bỏ bộ lọc, Xem bảng giá | 5 dòng, không có Nước mắm, STT 1..5 |
| 8 | Xuất PDF | Nội dung khớp với bản xem trước (trừ đặc điểm do giấy phép demo) |
| 9 | Tìm `nước`, xuất Excel (lưới) | Đúng 2 dòng, ô tiền là số |
| 10 | Xuất Excel tồn kho | 5 mặt hàng, công thức G2=E2*F2, tổng G7=SUM(G2:G6) bằng 7.300.000 |
| 11 | Mở file Excel rồi thử xuất lại cùng tên | Hiện hướng dẫn đóng file thay vì crash |
| 12 | Thêm hàng còn bán rồi xem report lại | Report tự có thêm hàng mới |

**Lưu ý:** Các số lượng/giá trị mục 7, 10 chỉ đúng với dữ liệu mẫu chưa bị bạn thêm/sửa/xóa.

## 6. Các giới hạn kiểm chứng

Bản ZIP đã được kiểm tra cấu trúc file, XML report và phép tính mẫu. Môi trường xây dựng hiện tại không có Windows, Visual Studio/.NET 10 SDK và SQL Server LocalDB; **chưa thể thực thi Build và F5 trực tiếp**. Nếu Visual Studio hiện báo lỗi, gửi ảnh Error List để sửa theo đúng package bạn cài trong máy.
