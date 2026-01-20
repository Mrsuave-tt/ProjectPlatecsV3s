using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;
using ProjectPlatec.Services;

namespace ProjectPlatec.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            EmailService emailService,
            IConfiguration configuration,
            ILogger<StudentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            // Debug: Check user roles
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User {User} with roles {Roles} accessing Student/Index", user.Email, string.Join(", ", roles));
            }
            
            var students = await _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
            return View(students);
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if StudentId already exists
                if (await _context.Students.AnyAsync(s => s.StudentId == model.StudentId))
                {
                    ModelState.AddModelError("StudentId", "A student with this Student ID already exists.");
                    return View(model);
                }

                // Check if Email already exists in Students table
                if (await _context.Students.AnyAsync(s => s.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "A student with this email already exists.");
                    return View(model);
                }

                // Check if Student ID already exists as a username
                if (await _userManager.FindByNameAsync(model.StudentId) != null)
                {
                    ModelState.AddModelError("StudentId", "A user account with this Student ID already exists.");
                    return View(model);
                }

                // Check if Email already exists as a user account
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "A user account with this email already exists.");
                    return View(model);
                }

                // Create the student user account with Student ID as username
                var user = new ApplicationUser
                {
                    UserName = model.StudentId, // Use Student ID as username
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                // Create user account with Student ID as initial password
                var result = await _userManager.CreateAsync(user, model.StudentId);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Add student to Student role (if role exists, otherwise skip)
                try
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }
                catch
                {
                    // Role might not exist yet, that's okay
                }

                // Create the student record
                var student = new Student
                {
                    StudentId = model.StudentId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    CreatedDate = DateTime.Now,
                    UserId = user.Id
                };

                _context.Add(student);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Student created: {StudentId} with user account", student.StudentId);

                // Send email with credentials
                try
                {
                    var websiteUrl = _configuration["AppSettings:WebsiteUrl"] ?? Request.Scheme + "://" + Request.Host;
                    await _emailService.SendStudentCredentialsAsync(
                        model.Email,
                        $"{model.FirstName} {model.LastName}",
                        model.StudentId,
                        model.StudentId, // Password is same as Student ID
                        websiteUrl
                    );
                    TempData["SuccessMessage"] = $"Student created successfully. Login credentials have been sent to {model.Email}.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}. Error: {ErrorMessage}", model.Email, ex.Message);
                    var errorDetails = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    TempData["SuccessMessage"] = $"Student created successfully. Login: Student ID = {model.StudentId}, Password = {model.StudentId}.";
                    TempData["ErrorMessage"] = $"Email could not be sent: {errorDetails}. Please check your email configuration in appsettings.json.";
                }

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentViewModel
            {
                Id = student.Id,
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                DateOfBirth = student.DateOfBirth
            };

            return View(model);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return NotFound();
                }

                // Check if StudentId already exists (excluding current student)
                if (await _context.Students.AnyAsync(s => s.StudentId == model.StudentId && s.Id != id))
                {
                    ModelState.AddModelError("StudentId", "A student with this Student ID already exists.");
                    return View(model);
                }

                // Check if Email already exists (excluding current student)
                if (await _context.Students.AnyAsync(s => s.Email == model.Email && s.Id != id))
                {
                    ModelState.AddModelError("Email", "A student with this email already exists.");
                    return View(model);
                }

                student.StudentId = model.StudentId;
                student.FirstName = model.FirstName;
                student.LastName = model.LastName;
                student.Email = model.Email;
                student.PhoneNumber = model.PhoneNumber;
                student.DateOfBirth = model.DateOfBirth;

                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Student updated: {StudentId}", student.StudentId);
                    TempData["SuccessMessage"] = "Student updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // Delete associated user account if exists
                if (!string.IsNullOrEmpty(student.UserId))
                {
                    var user = await _userManager.FindByIdAsync(student.UserId);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }
                }

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Student deleted: {StudentId}", student.StudentId);
                TempData["SuccessMessage"] = "Student deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}

