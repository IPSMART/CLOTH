namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_BUSNAT")]
    public partial class M_SUBLEG_BUSNAT
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(2)]
        public string NATBUSCD { get; set; }

        public virtual M_SUBLEG M_SUBLEG { get; set; }
    }
}
