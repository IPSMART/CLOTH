namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_JBILLDTL")]
    public partial class T_JBILLDTL
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
        [StringLength(1)]
        public string DRCR { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(1)]
        public string QNTYCALCON { get; set; }

        public double? OTHQNTY { get; set; }

        public double? DNCNQNTY { get; set; }

        public double? PQNTY { get; set; }

        public double? BQNTY { get; set; }

        public double? RQNTY { get; set; }

        public double? RATE { get; set; }

        public double? AMT { get; set; }

        public double? DISCPER { get; set; }

        public double? DISCAMT { get; set; }

        public double? TAXABLEVAL { get; set; }

        public double? IGSTPER { get; set; }

        public double? IGSTAMT { get; set; }

        public double? CGSTPER { get; set; }

        public double? CGSTAMT { get; set; }

        public double? SGSTPER { get; set; }

        public double? SGSTAMT { get; set; }

        public double? CESSPER { get; set; }

        public double? CESSAMT { get; set; }

        [StringLength(30)]
        public string AGDOCAUTONO { get; set; }

        [StringLength(16)]
        public string AGDOCNO { get; set; }

        public DateTime? AGDOCDT { get; set; }

        [StringLength(4)]
        public string PRODGRPCD { get; set; }

        public DateTime? EFFDT { get; set; }

        public double? ADDLESSAMT { get; set; }
        public double? NOS { get; set; }
        [StringLength(100)]
        public string ITREM { get; set; }
    }
}
