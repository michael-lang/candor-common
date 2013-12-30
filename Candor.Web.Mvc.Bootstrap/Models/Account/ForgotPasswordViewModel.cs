using System.ComponentModel.DataAnnotations;

namespace CandorMvcApplication.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email Address")]
        public string UserName { get; set; }
    }
}