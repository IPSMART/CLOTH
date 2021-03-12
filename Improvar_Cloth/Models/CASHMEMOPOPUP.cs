using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class CASHMEMOPOPUP
    {
        public string BARNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double QNTY { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        public bool C_Checked { get; set; }
        public short SLNO { get; set; }
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        public string ITNM { get; set; }
        public short? DECIMALS { get; set; }
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        public string PARTCD { get; set; }
        public string UOM { get; set; }
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        public string CLRBARCODE { get; set; }
        public string SZBARCODE { get; set; }
        public string SIZECD { get; set; }
        public string SIZENM { get; set; }
        public double? NOS { get; set; }
        public double? CUTLENGTH { get; set; }
        public string SHADE { get; set; }
        public string AGDOCNO { get; set; }
        public string AGDOCDT { get; set; }
    }
}