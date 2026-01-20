using System.ComponentModel.DataAnnotations;

namespace ProjectPlatec.Models
{
    public class AttendanceViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Status")]
        public AttendanceStatus Status { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }

    public class MarkAttendanceViewModel
    {
        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        public List<AttendanceViewModel> Students { get; set; } = new();
    }
}

