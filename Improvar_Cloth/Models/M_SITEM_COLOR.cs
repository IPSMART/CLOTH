namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEM_COLOR")]
    public partial class M_SITEM_COLOR
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
        [StringLength(8)]
        public string ITCD { get; set; }

        public byte? SLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COLRCD { get; set; }

        [StringLength(1)]
        public string INACTIVE_TAG { get; set; }

    }
}
