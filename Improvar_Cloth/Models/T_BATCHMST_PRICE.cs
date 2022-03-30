namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_BATCHMST_PRICE")]
    public partial class T_BATCHMST_PRICE
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
        [StringLength(25)]
        public string BARNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string PRCCD { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime EFFDT { get; set; }

        [StringLength(30)]
        public string AUTONO { get; set; }

        public double? RATE { get; set; }
    }
}
