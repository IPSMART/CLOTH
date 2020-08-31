namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEM_PARTS")]
    public partial class M_SITEM_PARTS
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

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string PARTCD { get; set; }

         public double? JOBRT { get; set; }

    }
}
