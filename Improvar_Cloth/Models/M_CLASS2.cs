namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_CLASS2")]
    public partial class M_CLASS2
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
        [StringLength(8)]
        public string CLASS2CD { get; set; }

        [Required]
        [StringLength(40)]
        public string CLASS2NM { get; set; }

        public long M_AUTONO { get; set; }
    }
}
