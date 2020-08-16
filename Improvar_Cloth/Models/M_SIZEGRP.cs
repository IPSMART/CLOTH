namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SIZEGRP")]
    public partial class M_SIZEGRP
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
        [StringLength(30)]
        public string SIZEGRPCD { get; set; }

        [StringLength(30)]
        public string SIZEGRPNM { get; set; }

        [StringLength(3)]
        public string PRINT_SEQ { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
