﻿namespace SmartAttendance.Models
{
    public class AttendanceViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string CheckTime { get; set; } = string.Empty;
        public string Comment { get; set; } = "No comments yet";
    }
}
