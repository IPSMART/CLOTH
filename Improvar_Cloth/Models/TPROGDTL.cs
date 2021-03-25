using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TPROGDTL
    {
        public short? EMD_NO { get; set; }
        
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }
        
        [StringLength(30)]
        public string AUTONO { get; set; }
        
        public short SLNO { get; set; }
        
        [StringLength(5)]
        public string DOCCD { get; set; }
        
        [StringLength(6)]
        public string DOCNO { get; set; }

        public DateTime? DOCDT { get; set; }
        
        [StringLength(30)]
        public string PROGAUTONO { get; set; }

        public short PROGSLNO { get; set; }
        
        [StringLength(1)]
        public string STKDRCR { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }

        public double? STKQNTY { get; set; }

        public double? RATE { get; set; }
        
        [StringLength(25)]
        public string BARNO { get; set; }
        
        [StringLength(100)]
        public string ITREMARK { get; set; }

        public double? CUTLENGTH { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        [StringLength(30)]
        public string ISSPROGAUTONO { get; set; }
        public short? ISSPROGSLNO { get; set; }
        public bool Checked { get; set; }
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        [StringLength(10)]
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        public string FABITCD { get; set; }
        public string STYLENO { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        [StringLength(4)]
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        [StringLength(4)]
        public string SIZECD { get; set; }
        public string SIZENM { get; set; }
        public string UOM { get; set; }
        public bool  CheckedSample { get; set; }
        public string SAMPLE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? BALQNTY { get; set; }
        [StringLength(15)]
        public string PROGUNIQNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? BALNOS { get; set; }
        public string PROGAUTOSLNO { get; set; }
        public string BarImages { get; set; }
        public string BarImagesCount { get; set; }
        public short? DECIMALS { get; set; }
        public string CLRBARCODE { get; set; }
        public string SZBARCODE { get; set; }
        public double TOTALREQQTY { get; set; }
        public double USEDQTY { get; set; }
        public string COMMONUNIQBAR { get; set; }
    }
}