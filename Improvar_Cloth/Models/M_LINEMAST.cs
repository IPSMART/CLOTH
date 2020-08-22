namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_LINEMAST")]
    public partial class M_LINEMAST
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
        [StringLength(5)]
        public string LINECD { get; set; }

        [Required]
        [StringLength(10)]
        public string LINENM { get; set; }

        [Required]
        [StringLength(3)]
        public string FLRCD { get; set; }

        public long M_AUTONO { get; set; }

        
    }
}
