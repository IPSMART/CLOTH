using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class PENDINGORDER
    {
        public string ORDDOCNO { get; set; }
        public string ORDDOCDT { get; set; }
        public string ITGRPNM { get; set; }
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        public string SIZECD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double ORDQTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double BALQTY { get; set; }
        public string ORDAUTONO { get; set; }
        public string ORDSLNO { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        public bool Ord_Checked { get; set; }
        public short SLNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double CURRENTADJQTY { get; set; }
        public string PDESIGN { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        public string ITGRPCD { get; set; }
        public string BARGENTYPE { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        public string SIZENM { get; set; }
        public string UOM { get; set; }
        public string HSNCODE { get; set; }
        public string PRTBARCODE { get; set; }
        public string CLRBARCODE { get; set; }
        public string SZBARCODE { get; set; }
        public string GLCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? GSTPER { get; set; }
        public string PRODGRPGSTPER { get; set; }
        public double? WPRATE { get; set; }
        public double? RPRATE { get; set; }
        public string WPPRICEGEN { get; set; }
        public string RPPRICEGEN { get; set; }
        public string BarImages { get; set; }
        public string BarImagesCount { get; set; }
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        public string MTBARCODE { get; set; }
    }
}