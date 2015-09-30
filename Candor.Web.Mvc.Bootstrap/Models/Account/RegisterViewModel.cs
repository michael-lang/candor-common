using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Candor.Security;

namespace CandorMvcApplication.Models.Account
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Email Address")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public void Load()
        {
            //this is for loading any lookup lists the view inputs require.
        }
        public User ToUser()
        {
            return new User() { Name = UserName, PasswordHash = Password };
        }
    }
}