using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class Location
    {
        [Required]
        public string unitdest { get; set; }
        [Required]
        public string add1 { get; set; }
    }
}