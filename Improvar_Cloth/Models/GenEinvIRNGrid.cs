using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Improvar.Models
{
    public class GenEinvIRNGrid
    {
        public string AUTONO { get; set; }
        public int SLNO { get; set; }
        //public string DOCTYPE { get; set; }
        public string BLNO { get; set; }
        public string BLDT { get; set; }
        public string SLNM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double BLAMT { get; set; }
        public string WAYBILL { get; set; }
        public bool WAYBILLChecked { get; set; }
        public string IRNNO { get; set; }
        public string MESSAGE { get; set; }
        public bool Checked { get; set; }
        public string Remarks { get; set; }
        public string Reason { get; set; }
        public string EWB { get; set; }
        public string TRANSLNM { get; set; }
        public string LORRYNO { get; set; }

    }

}