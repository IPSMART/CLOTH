namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_SORDDTL")]
    public partial class T_SORDDTL
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
        [StringLength(1)]
        public string STKDRCR { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [StringLength(1)]
        public string FREESTK { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public double? QNTY { get; set; }

        public double? RATE { get; set; }

        public double DISCAMT { get; set; }

        public double SCMDISCAMT { get; set; }

        public double TAXAMT { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

        [Required]
        [StringLength(4)]
        public string PRCCD { get; set; }

        public DateTime? PRCEFFDT { get; set; }

        public DateTime? DELVDT { get; set; }

        [StringLength(500)]
        public string ITREM { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        [StringLength(30)]
        public string PDESIGN { get; set; }
        


    }
}
