namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNDTL")]
    public partial class T_TXNDTL
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

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

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

        [StringLength(4)]
        public string BALEYR { get; set; }

        [StringLength(30)]
        public string BALENO { get; set; }

        [Required]
        [StringLength(6)]
        public string GOCD { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        public decimal? NOS { get; set; }

        public decimal? QNTY { get; set; }

        public decimal? BLQNTY { get; set; }

        public decimal? RATE { get; set; }

        public decimal? AMT { get; set; }

        public decimal? FLAGMTR { get; set; }

        public decimal? TOTDISCAMT { get; set; }

        public decimal? TXBLVAL { get; set; }

        public decimal? IGSTAMT { get; set; }

        public decimal? IGSTPER { get; set; }

        public decimal? CGSTAMT { get; set; }

        public decimal? CGSTPER { get; set; }

        public decimal? SGSTAMT { get; set; }

        public decimal? SGSTPER { get; set; }

        public decimal? CESSAMT { get; set; }

        public decimal? CESSPER { get; set; }

        public decimal? DUTYAMT { get; set; }

        public decimal? DUTYPER { get; set; }

        public decimal? NETAMT { get; set; }

        public decimal? OTHRAMT { get; set; }

        [StringLength(16)]
        public string AGDOCNO { get; set; }

        public DateTime? AGDOCDT { get; set; }

        public decimal? SHORTQNTY { get; set; }

        public decimal? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }

        public decimal? DISCAMT { get; set; }

        public decimal? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }

        public decimal? SCMDISCAMT { get; set; }

        public decimal? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }

        public decimal? TDDISCAMT { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        public DateTime? PRCEFFDT { get; set; }

        [StringLength(25)]
        public string BARNO { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

      
    }
}
