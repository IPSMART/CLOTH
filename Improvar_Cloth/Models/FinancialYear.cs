using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class FinancialYear
    {
        [Required]
        public string finan { get; set; }
        [Required]
        public string usr_id { get; set; }
    }
}