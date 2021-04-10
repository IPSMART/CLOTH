namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNSLSMN")]
    public partial class T_TXNSLSMN
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
        [StringLength(8)]
        public string SLMSLCD { get; set; }

        public double? PER { get; set; }

        public double? ITAMT { get; set; }

        public double? BLAMT { get; set; }
        public byte? SLNO { get; set; }
    }
}
