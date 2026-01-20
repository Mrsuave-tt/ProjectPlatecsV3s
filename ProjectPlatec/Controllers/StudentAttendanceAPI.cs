using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;

namespace ProjectPlatec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentAttendanceAPI(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetAttendance(string studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
                return NotFound(new { message = "Student not found" });

            var attendance = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.Date,
                    a.Status
                }).ToListAsync();

            return Ok(attendance);
        }
    }
}