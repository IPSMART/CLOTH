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

        [StringLength(10)]
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
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? BLQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? AGDOCDT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? SHORTQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }
        public string SCMDISCTYPE_DESC { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TDDISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }
        public string TDDISCTYPE_DESC { get; set; }

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
        public string ITSTYLE { get; set; }
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
        [StringLength(8)]
        public string CLASS1CD { get; set; }
        public short TXNSLNO { get; set; }
        public string MTRLJOBNM { get; set; }
        [StringLength(15)]
        public string STKNAME { get; set; }
        public string DISCTYPE_DESC { get; set; }
        
        public double? TOTDISCAMT { get; set; }
        public string ALL_GSTPER { get; set; }
        public string PRODGRPGSTPER { get; set; }
        [StringLength(4)]
        public string BALEYR { get; set; }
        public double? GSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? LISTPRICE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? LISTDISCPER { get; set; }
        public List<DISC_TYPE> DISC_TYPE { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public string PARTNM { get; set; }
        public string SIZENM { get; set; }
        public string COLRNM { get; set; }
        public string MTBARCODE { get; set; }
        public string PRTBARCODE { get; set; }
       
        public string CLRBARCODE { get; set; }
        public string SZBARCODE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? WPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RPRATE { get; set; }
        public string BARGENTYPE { get; set; }
        public int? PAGENO { get; set; }
        public int? PAGESLNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? OTHRAMT { get; set; }
        [StringLength(3)]
        public string BLUOMCD { get; set; }
        public string LINKAUTONO { get; set; }
        public short? RECPROGSLNO { get; set; }
        public double? CONVQTYPUNIT { get; set; }
        public string NEGSTOCK { get; set; }
        public double? BALSTOCK { get; set; }
        public double PROGSLNO { get; set; }
        public string PROGITSTYLE { get; set; }
        public string PROGUOMNM { get; set; }
        public double PROGQNTY { get; set; }
    }
}