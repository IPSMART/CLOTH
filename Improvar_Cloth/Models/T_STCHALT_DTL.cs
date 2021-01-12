namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_STCHALT_DTL")]
    public partial class T_STCHALT_DTL
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
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SLNO { get; set; }

        [Required]
        [StringLength(3)]
        public string STCHCD { get; set; }

        public short QNTY { get; set; }

        public double? RATE { get; set; }
        
    }
}
