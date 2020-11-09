using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class RepBarcodeImage : Permission
    {
        public string BARNO { get; set; }
        public string ROWSPERPAGE { get; set; }
        public int COLPERPAGE { get; set; }
        public string LINE1 { get; set; }
        public string LINE2 { get; set; }
    }
}