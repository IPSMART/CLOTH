using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TSALEBARNOPOPUP
    {//k
        public short SLNO { get; set; }
        public string BARNO { get; set; }
        public short? TXNSLNO { get; set; } 
        [StringLength(4)]
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        [StringLength(8)]
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        public string STYLENO { get; set; }
        public string FABITCD { get; set; }
        public string FABITNM { get; set; }
        [StringLength(1)]
        public string STKTYPE { get; set; }
        [StringLength(15)]
        public string STKNAME { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        [StringLength(4)]
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        [StringLength(4)]
        public string SIZECD { get; set; }
        public string SIZENM { get; set; }
        [StringLength(15)]
        public string SHADE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        public string UOM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISCRATE { get; set; }
        [StringLength(8)]
        public string HSNCODE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? GSTPER { get; set; }
        public string PSTYLENO { get; set; }
        public double? BALSTOCK { get; set; }
        public double? FLAGMTR { get; set; }
        [StringLength(1)]
        public string DISCTYPE { get; set; }
        [StringLength(30)]
        public string BALENO { get; set; }
        public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }
        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }
        [StringLength(10)]
        public string LOCABIN { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public string DOCDT { get; set; }
        public double BALQNTY { get; set; }
        public double BALNOS { get; set; }
        public bool Checked { get; set; }
    }
}