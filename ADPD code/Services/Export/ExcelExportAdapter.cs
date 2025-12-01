using ADPD_code.Models;
using ADPD_code.Services.Export;
using ClosedXML.Excel; 
using System.ComponentModel;
using System.Collections.Generic; 
using System.IO;

public class ExcelExportAdapter : IExportService
{
    // Đảm bảo tên phương thức khớp với IExportService (Giả sử là ExportStudentsToExcel)
    public byte[] ExportStudentsToExcel(List<StudentExportModel> students)
    {
        // 1. Tạo Workbook mới
        using (var workbook = new XLWorkbook())
        {
            // 2. Thêm Worksheet
            var worksheet = workbook.Worksheets.Add("Students");

            // Create header (Sử dụng chỉ mục 1 dựa trên 1)
            string[] headers =
            {
                "STT", "MSSV", "Họ và tên", "Lớp", "Email", "SĐT",
                "Điểm TB", "Tỷ lệ tham dự (%)", "Trạng thái"
            };

            // Viết Header vào dòng 1
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                // In đậm Header
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            // Fill rows (Bắt đầu từ dòng 2)
            int row = 2;
            foreach (var s in students)
            {
                // ClosedXML sử dụng Value
                worksheet.Cell(row, 1).Value = s.Index;
                worksheet.Cell(row, 2).Value = s.MSSV;
                worksheet.Cell(row, 3).Value = s.FullName;
                worksheet.Cell(row, 4).Value = s.ClassName;
                worksheet.Cell(row, 5).Value = s.Email;
                worksheet.Cell(row, 6).Value = s.Phone;
                worksheet.Cell(row, 7).Value = s.GPA;
                // Chuyển đổi thành chuỗi khi ghi vào ô
                worksheet.Cell(row, 8).Value = s.AttendanceRate + "%";
                worksheet.Cell(row, 9).Value = s.Status;
                row++;
            }

            // 3. Định dạng và Trả về mảng byte
            worksheet.Columns().AdjustToContents(); 

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream); 
                return stream.ToArray(); 
            }
        }
    }
}