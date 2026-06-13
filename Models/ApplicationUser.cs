using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Baitaptuan5.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Há» và tên")]
        public string? FullName { get; set; }

        [Display(Name = "Äịa chá»‰")]
        public string? Address { get; set; }

        [Display(Name = "Tuổi")]
        public string? Age { get; set; }
    }
}

