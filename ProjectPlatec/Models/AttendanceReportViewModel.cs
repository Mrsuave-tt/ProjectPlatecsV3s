namespace ProjectPlatec.Models
{
    public class AttendanceReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; } = "Daily"; // Daily or Weekly
        public List<AttendanceReportItem> ReportItems { get; set; } = new();
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLate { get; set; }
    }

    public class AttendanceReportItem
    {
        public DateTime Date { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int TotalStudents { get; set; }
        public double PresentPercentage { get; set; }
    }
}

