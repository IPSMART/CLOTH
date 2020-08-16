using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class Password
    {
        [Required]
        [Display(Name = "User Id")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }
        [Required]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
        public string CPF { get; set; }
        [Display(Name = "OTP")]
        public string OTP { get; set; }
        public int NOOFTXTCHAR { get; set; }
        public int NOOFLOWERCHAR { get; set; }
        public int NOOFUPPERCHAR { get; set; }
        public int NOOFSPCHAR { get; set; }
        public int NOOFNUMCHAR { get; set; }
        public int MINPWDLENGTH { get; set; }
        public int MAXPWDLENGTH { get; set; }
      
    }
}