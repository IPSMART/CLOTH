namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_MACHINE")]
    public partial class M_MACHINE
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
        public string MCCD { get; set; }

        [Required]
        [StringLength(40)]
        public string MCNM { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
