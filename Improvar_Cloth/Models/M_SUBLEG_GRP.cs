namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_GRP")]
    public partial class M_SUBLEG_GRP
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(2)]
        public string GRPCD { get; set; }

        [StringLength(6)]
        public string SLCDGRPCD { get; set; }

        [Required]
        [StringLength(40)]
        public string SLCDGRPNM { get; set; }

        [Key]
        [StringLength(200)]
        public string GRPCDFULL { get; set; }

        [StringLength(6)]
        public string PARENTCD { get; set; }

        [StringLength(40)]
        public string USER_ID { get; set; }

        public int? GRPSLNO { get; set; }

        [StringLength(1000)]
        public string SLCDGRPNMDESC { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        public long M_AUTONO { get; set; }
    }
}
