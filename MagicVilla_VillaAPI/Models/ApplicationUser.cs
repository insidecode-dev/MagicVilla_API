using Microsoft.AspNetCore.Identity;

namespace MagicVilla_VillaAPI.Models
{
    // we craeted this class to add more properties those don't exist in IdentityUser
    public class ApplicationUser:IdentityUser
    {
        // Name property does not exist as built-in IdentityUser class
        public string Name { get; set; }
    }
}
