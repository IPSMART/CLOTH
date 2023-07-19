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
        public string AGAUTOSLNO { get; set; }
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
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        public double? TXBLVAL { get; set; }
        public double? IGSTPER { get; set; }
        public double? CGSTPER { get; set; }
        public double? SGSTPER { get; set; }
        public double? CESSPER { get; set; }
        public string STKTYP { get; set; }
        public string UOM { get; set; }
        public string PRODGRPGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        public string SCMDISCTYPE_DESC { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SCMDISCRATE { get; set; }
        public string GLCD { get; set; }
        public string HSNCODE { get; set; }
        public double DISCRATE { get; set; }
        public string DISCTYPE { get; set; }


    }
}