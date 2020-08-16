namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DISTRICT")]
    public partial class M_DISTRICT
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(5)]
        public string DISTCD { get; set; }

        [StringLength(40)]
        public string DISTNM { get; set; }

        public long? M_AUTONO { get; set; }
        [StringLength(4)]
        public string CLCD { get; set; }
    }
}
