namespace Improvar.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("T_JBILL")]
    public partial class T_JBILL
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(8)]
        public string CONSLCD { get; set; }

        [StringLength(16)]
        public string PBLNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PBLDT { get; set; }

        public byte? CRDAYS { get; set; }

        [StringLength(1)]
        public string DUECALCON { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DUEDT { get; set; }

        [StringLength(8)]
        public string BROKSLCD { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        [Required]
        [StringLength(2)]
        public string DOCTAG { get; set; }

        [StringLength(8)]
        public string CREGLCD { get; set; }

        [StringLength(8)]
        public string EXPGLCD { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(3)]
        public string UOMCD { get; set; }

        public double? TAXABLEVAL { get; set; }

        [StringLength(1)]
        public string ROYN { get; set; }

        public double? ROAMT { get; set; }

        public double? BLAMT { get; set; }

        [StringLength(3)]
        public string TDSHD { get; set; }

        public double? TDSON { get; set; }

        public double? TDSPER { get; set; }

        public double? TDSAMT { get; set; }

        [StringLength(1)]
        public string LOWTDS { get; set; }

        public double? ADVAMT { get; set; }

        [StringLength(3)]
        public string CURR_CD { get; set; }

        public double? CURRRT { get; set; }

        [StringLength(1)]
        public string REVCHG { get; set; }

        [StringLength(500)]
        public string REM { get; set; }

        [StringLength(30)]
        public string OTHAUTONO { get; set; }

        public decimal? DNCNAMT { get; set; }

        [StringLength(4)]
        public string TAXGRPCD { get; set; }
        [StringLength(30)]
        public string OTHAUTONO1 { get; set; }

        public decimal? DNCNAMT1 { get; set; }
    }
}
