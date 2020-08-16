using System.ComponentModel.DataAnnotations;
namespace Improvar.Models
{
    public class Login
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string IP { get; set; }
        public bool REMEMBERME { get; set; }
    }
}