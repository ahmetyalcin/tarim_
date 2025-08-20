using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TarimHibe.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; } = "";
        [PersonalData]
        public string LastName { get; set; } = "";
    }
}
