namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_GODLINK")]
    public partial class M_GODLINK
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
        [StringLength(6)]
        public string GOCD { get; set; }

        [Required]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [Required]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }
    }
}
