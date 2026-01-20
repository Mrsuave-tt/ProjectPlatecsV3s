using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;

namespace ProjectPlatec.Controllers
{

        [Route("api/[controller]")]
    [ApiController]
    public class API(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ApplicationDbContext _context = context;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByNameAsync(model.EmailOrStudentId)
                       ?? await _userManager.FindByEmailAsync(model.EmailOrStudentId);

            if (user == null)
                return Unauthorized(new { message = "Invalid login" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid login" });

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student == null)
                return Unauthorized(new { message = "Not a student" });

            return Ok(new
            {
                user.Id,
                StudentId = student.StudentId,
                FullName = $"{user.FirstName} {user.LastName}",
                student.Email
            });
        }
    }

    public class LoginRequest
    {
        public string EmailOrStudentId { get; set; }
        public string Password { get; set; }
    }
}