namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_UOM")]
    public partial class M_UOM
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [StringLength(3)]
        public string UOMCD { get; set; }

        [StringLength(5)]
        public string UOMNM { get; set; }

        public byte? DECIMALS { get; set; }

        public double? TAREWT { get; set; }

        public long? M_AUTONO { get; set; }

        [Required]
        [StringLength(3)]
        public string GST_UOMCD { get; set; }

        public double? GST_QNTYCONV { get; set; }
    }
}
