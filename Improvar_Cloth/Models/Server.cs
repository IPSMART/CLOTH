using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Improvar.Models
{
    public class Server
    {
        [Required]
        public string Host { get; set; }
        [Required]      
        public int Port { get; set; }
        [Required]
        public string ServerName { get; set; }
        [Required]
        public string ServiceName { get; set; }
        [Required]
        [Display(Name = "User Name")]
        public string DBUserName { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string DBPassword { get; set; }

    }
}