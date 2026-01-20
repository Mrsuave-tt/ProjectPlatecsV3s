using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;

namespace ProjectPlatec.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var totalStudents = await _context.Students.CountAsync();
            var todayAttendance = await _context.Attendances
                .Where(a => a.Date == today)
                .ToListAsync();

            var presentCount = todayAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentCount = todayAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var lateCount = todayAttendance.Count(a => a.Status == AttendanceStatus.Late);
            var notMarked = totalStudents - todayAttendance.Count;

            ViewBag.TotalStudents = totalStudents;
            ViewBag.PresentCount = presentCount;
            ViewBag.AbsentCount = absentCount;
            ViewBag.LateCount = lateCount;
            ViewBag.NotMarked = notMarked;
            ViewBag.Today = today;

            return View();
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Reports(string reportType = "Daily", DateTime? startDate = null, DateTime? endDate = null)
        {
            var model = new AttendanceReportViewModel
            {
                ReportType = reportType
            };

            if (reportType == "Daily")
            {
                var date = startDate ?? DateTime.Today;
                model.StartDate = date;
                model.EndDate = date;

                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .Where(a => a.Date == date)
                    .ToListAsync();

                var totalStudents = await _context.Students.CountAsync();
                var presentCount = attendances.Count(a => a.Status == AttendanceStatus.Present);
                var absentCount = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                var lateCount = attendances.Count(a => a.Status == AttendanceStatus.Late);

                model.ReportItems.Add(new AttendanceReportItem
                {
                    Date = date,
                    PresentCount = presentCount,
                    AbsentCount = absentCount,
                    LateCount = lateCount,
                    TotalStudents = totalStudents,
                    PresentPercentage = totalStudents > 0 ? (double)presentCount / totalStudents * 100 : 0
                });

                model.TotalPresent = presentCount;
                model.TotalAbsent = absentCount;
                model.TotalLate = lateCount;
            }
            else if (reportType == "Weekly")
            {
                var start = startDate ?? DateTime.Today.AddDays(-6);
                var end = endDate ?? DateTime.Today;
                model.StartDate = start;
                model.EndDate = end;

                var totalStudents = await _context.Students.CountAsync();
                var reportItems = new List<AttendanceReportItem>();

                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    var attendances = await _context.Attendances
                        .Where(a => a.Date == date)
                        .ToListAsync();

                    var presentCount = attendances.Count(a => a.Status == AttendanceStatus.Present);
                    var absentCount = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                    var lateCount = attendances.Count(a => a.Status == AttendanceStatus.Late);

                    reportItems.Add(new AttendanceReportItem
                    {
                        Date = date,
                        PresentCount = presentCount,
                        AbsentCount = absentCount,
                        LateCount = lateCount,
                        TotalStudents = totalStudents,
                        PresentPercentage = totalStudents > 0 ? (double)presentCount / totalStudents * 100 : 0
                    });
                }

                model.ReportItems = reportItems;
                model.TotalPresent = reportItems.Sum(r => r.PresentCount);
                model.TotalAbsent = reportItems.Sum(r => r.AbsentCount);
                model.TotalLate = reportItems.Sum(r => r.LateCount);
            }

            return View(model);
        }
    }
}