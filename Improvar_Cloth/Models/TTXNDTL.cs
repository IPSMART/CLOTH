using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TTXNDTL
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        public short? BLSLNO { get; set; }

        [Required]
        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [Required]
        [StringLength(1)]
        public string STKDRCR { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }

        [StringLength(500)]
        public string PCSREM { get; set; }

        [StringLength(1)]
        public string FREESTK { get; set; }

        [StringLength(40)]
        public string BATCHNO { get; set; }

        [StringLength(30)]
        public string BALENO { get; set; }

        [Required]
        [StringLength(6)]
        public string GOCD { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? BLQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? AMT { get; set; }

        public double? FLAGMTR { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? MTRL_COST { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? OTH_COST { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SCMDISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? IGSTAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? IGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CGSTAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SGSTAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CESSAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CESSPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DUTYAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DUTYPER { get; set; }

        public double? WGHT { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(16)]
        public string AGDOCNO { get; set; }

        public DateTime? AGDOCDT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? SHORTQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TDDISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }

        [StringLength(30)]
        public string AGSTCHAUTONO { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        public DateTime? PRCEFFDT { get; set; }

        [StringLength(25)]
        public string BARNO { get; set; }
        public bool Checked { get; set; }
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        public string ITNM { get; set; }
        public string STYLENO { get; set; }
        public string FABITCD { get; set; }
        public string FABITNM { get; set; }
        public string UOM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TXBLVAL { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? NETAMT { get; set; }
        [StringLength(8)]
        public string GLCD { get; set; }
        public short TXNSLNO { get; set; }
        public string MTRLJOBNM { get; set; }
        [StringLength(15)]
        public string STKNAME { get; set; }
        public string DISCTYPE_DESC { get; set; }
        public double? TOTDISCAMT { get; set; }
    }
}