using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CandorMvcApplication.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email Address")]
        public string UserName { get; set; }
    }
}