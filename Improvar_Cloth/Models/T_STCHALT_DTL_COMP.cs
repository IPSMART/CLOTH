namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_STCHALT_DTL_COMP")]
    public partial class T_STCHALT_DTL_COMP
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
        public byte SLNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(7)]
        public string FLDCD { get; set; }

        [Required]
        [StringLength(500)]
        public string FLDVAL { get; set; }

        [StringLength(100)]
        public string FLDREM { get; set; }

        [Required]
        [StringLength(1)]
        public string FLDTYPE { get; set; }
        
    }
}
