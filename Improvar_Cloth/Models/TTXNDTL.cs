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

        public double? BLQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }

        public double? AMT { get; set; }

        public double? FLAGMTR { get; set; }

        public double? MTRL_COST { get; set; }

        public double? OTH_COST { get; set; }

        public double? DISCAMT { get; set; }

        public double? SCMDISCAMT { get; set; }

        public double? IGSTAMT { get; set; }

        public double? IGSTPER { get; set; }

        public double? CGSTAMT { get; set; }

        public double? CGSTPER { get; set; }

        public double? SGSTAMT { get; set; }

        public double? SGSTPER { get; set; }

        public double? CESSAMT { get; set; }

        public double? CESSPER { get; set; }

        public double? DUTYAMT { get; set; }

        public double? DUTYPER { get; set; }

        public double? WGHT { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(16)]
        public string AGDOCNO { get; set; }

        public DateTime? AGDOCDT { get; set; }

        public double? SHORTQNTY { get; set; }

        public double? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }

        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }

        public double? TDDISCAMT { get; set; }

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
        public string MTRLJOBNM { get; set; }
        public string ITNM { get; set; }
        public string FABITCD { get; set; }
        public string FABITNM { get; set; }
        public string STYLENO { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string PARTNM { get; set; }
        public string COLRNM { get; set; }
        public string SIZENM { get; set; }
        [StringLength(15)]
        public string SHADE { get; set; }
        public string UOM { get; set; }
        public short TXNSLNO { get; set; }
    }
}