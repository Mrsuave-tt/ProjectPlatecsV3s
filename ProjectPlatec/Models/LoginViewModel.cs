using System.ComponentModel.DataAnnotations;

namespace ProjectPlatec.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email or Student ID")]
        public string EmailOrStudentId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

