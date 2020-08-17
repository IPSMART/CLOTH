namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_ITEMPLISTDTL")]
    public partial class M_ITEMPLISTDTL
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
        [StringLength(4)]
        public string PRCCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public decimal RATE { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(8)]
        public string SIZECOLCD { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
