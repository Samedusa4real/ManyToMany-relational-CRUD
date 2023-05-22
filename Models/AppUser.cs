using Microsoft.AspNetCore.Identity;

namespace PustokTemplate.Models
{
    public class AppUser:IdentityUser
    {
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }

    }
}
