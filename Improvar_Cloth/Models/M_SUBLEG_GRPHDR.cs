namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_GRPHDR")]
    public partial class M_SUBLEG_GRPHDR
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
        [StringLength(2)]
        public string GRPCD { get; set; }

        [Required]
        [StringLength(20)]
        public string GRPNM { get; set; }

        public long M_AUTONO { get; set; }
    }
}
