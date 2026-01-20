using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;

namespace ProjectPlatec.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentDashboardController> _logger;

        public StudentDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentDashboardController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: StudentDashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                // Not a student, redirect to admin dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            // Get student's attendance records
            var today = DateTime.Today;
            var thisMonth = DateTime.Today.AddDays(-30);
            var last7Days = DateTime.Today.AddDays(-7);

            var recentAttendance = await _context.Attendances
                .Where(a => a.StudentId == student.Id && a.Date >= thisMonth)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToListAsync();

            var todayAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == student.Id && a.Date == today);

            var totalPresent = await _context.Attendances
                .CountAsync(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Present);

            var totalAbsent = await _context.Attendances
                .CountAsync(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Absent);

            var totalLate = await _context.Attendances
                .CountAsync(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Late);

            // Get recent absences and late marks for notifications (last 7 days)
            var recentAbsences = await _context.Attendances
                .Where(a => a.StudentId == student.Id && 
                           a.Date >= last7Days && 
                           (a.Status == AttendanceStatus.Absent || a.Status == AttendanceStatus.Late))
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.TodayAttendance = todayAttendance;
            ViewBag.RecentAttendance = recentAttendance;
            ViewBag.TotalPresent = totalPresent;
            ViewBag.TotalAbsent = totalAbsent;
            ViewBag.TotalLate = totalLate;
            ViewBag.RecentAbsences = recentAbsences;

            return View();
        }

        // GET: StudentDashboard/MyAttendance
        public async Task<IActionResult> MyAttendance(DateTime? startDate, DateTime? endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                return NotFound("Student profile not found.");
            }

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var attendances = await _context.Attendances
                .Where(a => a.StudentId == student.Id && a.Date >= start && a.Date <= end)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            return View(attendances);
        }
    }
}

