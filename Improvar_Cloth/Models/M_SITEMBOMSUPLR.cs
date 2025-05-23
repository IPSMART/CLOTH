namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEMBOMSUPLR")]
    public partial class M_SITEMBOMSUPLR
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
        [StringLength(10)]
        public string BOMCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte SLNO { get; set; }

        [Key]
        [Column(Order = 3)]
        public byte RSLNO { get; set; }

        [Key]
        [Column(Order = 4)]
        public byte PSLNO { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }
    }
}
