using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;

namespace ProjectPlatec.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Attendance/Mark
        public async Task<IActionResult> Mark(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var students = await _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == selectedDate)
                .ToListAsync();

            var viewModel = new MarkAttendanceViewModel
            {
                Date = selectedDate,
                Students = students.Select(s =>
                {
                    var existing = existingAttendances.FirstOrDefault(a => a.StudentId == s.Id);
                    return new AttendanceViewModel
                    {
                        StudentId = s.Id,
                        StudentName = s.FullName,
                        StudentIdNumber = s.StudentId,
                        Date = selectedDate,
                        Status = existing?.Status ?? AttendanceStatus.Present,
                        Remarks = existing?.Remarks
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Attendance/Mark
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mark(MarkAttendanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = User.Identity?.Name ?? "System";
            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == model.Date)
                .ToListAsync();

            foreach (var studentAttendance in model.Students)
            {
                var existing = existingAttendances.FirstOrDefault(a => a.StudentId == studentAttendance.StudentId);

                if (existing != null)
                {
                    // Update existing attendance
                    existing.Status = studentAttendance.Status;
                    existing.Remarks = studentAttendance.Remarks;
                    existing.MarkedBy = currentUser;
                    existing.MarkedAt = DateTime.Now;
                    _context.Update(existing);
                }
                else
                {
                    // Create new attendance
                    var attendance = new Attendance
                    {
                        StudentId = studentAttendance.StudentId,
                        Date = model.Date,
                        Status = studentAttendance.Status,
                        Remarks = studentAttendance.Remarks,
                        MarkedBy = currentUser,
                        MarkedAt = DateTime.Now
                    };
                    _context.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Attendance marked for date: {Date}", model.Date);
            TempData["SuccessMessage"] = $"Attendance marked successfully for {model.Date:MM/dd/yyyy}.";
            return RedirectToAction(nameof(Mark), new { date = model.Date });
        }

        // GET: Attendance/View
        public async Task<IActionResult> View(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.Date == selectedDate)
                .OrderBy(a => a.Student.LastName)
                .ThenBy(a => a.Student.FirstName)
                .ToListAsync();

            ViewBag.SelectedDate = selectedDate;
            return View(attendances);
        }
    }
}

