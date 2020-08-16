namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_ITEM_UN_CNV")]
    public partial class M_ITEM_UN_CNV
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
        [Column(Order = 0)]
        [StringLength(8)]
        public string ITCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string UOM { get; set; }

        public double? QNTY { get; set; }

        public double? CONV_FACT { get; set; }
        
    }
}
