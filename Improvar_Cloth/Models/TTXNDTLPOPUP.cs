using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TTXNDTLPOPUP
    {
        public string AUTONO { get; set; }
        public string AGDOCNO { get; set; }
        public string AGDOCDT { get; set; }
        public string ITNM { get; set; }
        public string BARNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double QNTY { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        public bool P_Checked { get; set; }
        public short SLNO { get; set; }
    
    }
}