namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_GLTBGRP")]
    public partial class M_GLTBGRP
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
        [StringLength(5)]
        public string GLTBGRPCD { get; set; }

        [Required]
        [StringLength(45)]
        public string GLTBGRPNM { get; set; }

        public long M_AUTONO { get; set; }
        [StringLength(5)]
        public string SEQNO { get; set; }
    }
}
