using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class MODULE
    {
        public string MODULE_NAME { get; set; }
        public string MENU_TABLE { get; set; }
        public string CONTROL_TABLE { get; set; }
        public string MENU_SORT_BY { get; set; }
        public char PERMISSION { get; set; }
        public string MODULE_CODE { get; set; }
    }
}