using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SmartAttendance.Models;
using System.Diagnostics;

namespace SmartAttendance.Controllers
{
    public class HomeController : Controller
    {

        private static List<AttendanceViewModel> _records = new();

        [HttpPost("ReceiveAttendance")]
        public IActionResult ReceiveAttendance([FromBody] List<AttendanceViewModel> records)
        {
            _records = new();
            if (records == null || records.Count == 0)
                return BadRequest(new { message = "Invalid or empty data" });

            var utcNow = DateTime.UtcNow;
            var bakuTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));

            Random random = new Random();
            foreach (var record in records)
            {
                if (record.IsPresent && string.IsNullOrEmpty(record.CheckTime))
                {
                    record.CheckTime = bakuTime.AddSeconds(-new Random().Next(10, 300)).ToString("HH:mm:ss");
                }
                if (!record.IsPresent)
                {
                    record.CheckTime = "--:--:--";
                }

                _records.Add(record);
            }

            return Ok(new { message = "Success Response" });
        }
        public IActionResult Index()
        {
            return View(_records);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost("DeleteStudent")]
        public IActionResult DeleteStudent([FromBody] int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _records.Count)
            {
                _records.RemoveAt(rowIndex);
                return Ok(new { message = "Student deleted successfully" });
            }
            return BadRequest(new { message = "Invalid row index" });
        }

        [HttpPost("UpdateStudent")]
        public IActionResult UpdateStudent([FromBody] UpdateStudentRequest request)
        {
            if (request.RowIndex >= 0 && request.RowIndex < _records.Count)
            {
                var student = _records[request.RowIndex];
                student.FullName = request.FullName;
                student.IsPresent = request.IsPresent;
                student.Comment = request.Comment;

                return Ok(new { message = "Student updated successfully" });
            }
            return BadRequest(new { message = "Invalid row index" });
        }

        [HttpGet("GetData")]
        public IActionResult GetData()
        {
            return PartialView("_AttendanceTablePartial", _records);
        }

        [HttpGet("ExportToExcel")]
        public IActionResult ExportToExcel()
        {
            try
            {
                var fileContents = new MemoryStream();
                string teacherName = "John Doe";
                string courseName = "Mathematics";
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string fileName = $"{courseName}_{teacherName.Replace(" ", "")}_{date}.xlsx";

                using (var package = new ExcelPackage(fileContents))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Attendance");

                    // Veri alanını yerleştireceğiz
                    worksheet.Cells[5, 1].Value = "No";
                    worksheet.Cells[5, 2].Value = "Full Name";
                    worksheet.Cells[5, 3].Value = "Status";
                    worksheet.Cells[5, 4].Value = "Check Time";
                    worksheet.Cells[5, 5].Value = "Comment";

                    // Başlıkları kalın ve ortalanmış yapalım
                    worksheet.Cells[5, 1, 5, 5].Style.Font.Bold = true;

                    // Veri satırlarını eklemek
                    int row = 6;
                    foreach (var attendance in _records)
                    {
                        worksheet.Cells[row, 1].Value = row-5;
                        worksheet.Cells[row, 2].Value = attendance.FullName;
                        worksheet.Cells[row, 3].Value = attendance.IsPresent ? "Present" : "Absent";
                        worksheet.Cells[row, 4].Value = attendance.CheckTime;
                        worksheet.Cells[row, 5].Value = attendance.Comment;
                        row++;
                    }


                    worksheet.Cells[5, 1, row - 1, 5].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[5, 1, row - 1, 5].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[5, 1, row - 1, 5].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[5, 1, row - 1, 5].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    worksheet.Cells[5, 1, row - 1, 5].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[5, 1, row - 1, 5].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    worksheet.Cells[5, 1, row - 1, 5].Style.Font.Size = 14;
                    worksheet.Cells[5, 1, row-1, 5].AutoFitColumns();

                    package.Save();
                }

                fileContents.Position = 0;

                if (fileContents.Length == 0)
                {
                    return Content("The file is empty. Please check the data.");
                }

                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Content($"An error occurred: {ex.Message}");
            }
        }

    }

}
