namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SCMITMGRP_HDR")]
    public partial class M_SCMITMGRP_HDR
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
        [StringLength(6)]
        public string SCMITMGRPCD { get; set; }

        [Required]
        [StringLength(30)]
        public string SCMITMGRPNM { get; set; }

        public long M_AUTONO { get; set; }
    }
}
