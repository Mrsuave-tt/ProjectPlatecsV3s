using Microsoft.AspNetCore.Identity;

namespace ProjectPlatec.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FirstName { get; set; }

        [PersonalData]
        public string? LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}

