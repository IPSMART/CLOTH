namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_PAYMENT")]
    public partial class M_PAYMENT
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
        public string PYMTCD { get; set; }

        [Required]
        [StringLength(20)]
        public string PYMTNM { get; set; }

        [Required]
        [StringLength(8)]
        public string GLCD { get; set; }

        public long M_AUTONO { get; set; }
        [StringLength(1)]
        public string PYMTTYPE { get; set; }
        
    }
}
