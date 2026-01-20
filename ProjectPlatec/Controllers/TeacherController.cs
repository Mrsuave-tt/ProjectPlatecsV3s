using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPlatec.Data;
using ProjectPlatec.Models;
using ProjectPlatec.Services;

namespace ProjectPlatec.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            EmailService emailService,
            IConfiguration configuration,
            ILogger<TeacherController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Teacher
        public async Task<IActionResult> Index()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            return View(teachers);
        }

        // GET: Teacher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Teacher created a new account with password.");
                    await _userManager.AddToRoleAsync(user, "Teacher");

                    // Send email with credentials
                    try
                    {
                        var emailSubject = "Your Teacher Account Credentials";
                        var emailBody = $@"
                            <h2>Welcome to ProjectPlatec</h2>
                            <p>Your teacher account has been created successfully.</p>
                            <p><strong>Login Details:</strong></p>
                            <ul>
                                <li>Email: {model.Email}</li>
                                <li>Password: {model.Password}</li>
                            </ul>
                            <p>Please change your password after first login.</p>
                            <p>Best regards,<br>ProjectPlatec Team</p>
                        ";

                        await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
                        TempData["SuccessMessage"] = $"Teacher created successfully. Login credentials have been sent to {model.Email}.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to teacher {Email}", model.Email);
                        TempData["SuccessMessage"] = $"Teacher created successfully. Login: {model.Email}, Password: {model.Password}.";
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Teacher/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teacher/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            var model = new RegisterViewModel
            {
                Email = teacher.Email!,
                FirstName = teacher.FirstName!,
                LastName = teacher.LastName!
            };

            return View(model);
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, RegisterViewModel model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                teacher.Email = model.Email;
                teacher.UserName = model.Email;
                teacher.FirstName = model.FirstName;
                teacher.LastName = model.LastName;

                var result = await _userManager.UpdateAsync(teacher);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Teacher updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Teacher/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teacher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher != null)
            {
                var result = await _userManager.DeleteAsync(teacher);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Teacher deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete teacher.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}