namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_GRPCLASS1")]
    public partial class M_SUBLEG_GRPCLASS1
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
        [StringLength(2)]
        public string GRPCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(8)]
        public string CLASS1CD { get; set; }
    }
}
