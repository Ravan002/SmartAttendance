namespace SmartAttendance.Models
{
    public class UpdateStudentRequest
    {
        public int RowIndex { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string Comment { get; set; } = "No comments yet";
    }
}
