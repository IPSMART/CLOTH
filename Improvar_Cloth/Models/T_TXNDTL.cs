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

         public double? NOS { get; set; }

         public double? QNTY { get; set; }

         public double? BLQNTY { get; set; }

         public double? RATE { get; set; }

         public double? AMT { get; set; }

         public double? FLAGMTR { get; set; }

         public double? TOTDISCAMT { get; set; }

         public double? TXBLVAL { get; set; }

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

         public double? NETAMT { get; set; }

         public double? OTHRAMT { get; set; }

        [StringLength(16)]
        public string AGDOCNO { get; set; }

        public DateTime? AGDOCDT { get; set; }

         public double? SHORTQNTY { get; set; }

         public double? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }

         public double? DISCAMT { get; set; }

         public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }

         public double? SCMDISCAMT { get; set; }

         public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }

         public double? TDDISCAMT { get; set; }

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
