namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_GROUP")]
    public partial class M_GROUP
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
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(30)]
        public string ITGRPNM { get; set; }

        [StringLength(25)]
        public string GRPNM { get; set; }

        [StringLength(1)]
        public string ITGRPTYPE { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(4)]
        public string PRODGRPCD { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(1)]
        public string BARGENTYPE { get; set; }
    }
}
