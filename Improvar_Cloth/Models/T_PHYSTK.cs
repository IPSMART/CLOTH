namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_PHYSTK")]
    public partial class T_PHYSTK
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(25)]
        public string BARNO { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        public double? NOS { get; set; }

        public double? QNTY { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }

        public double? RATE { get; set; }

        public double? DIA { get; set; }

        public double? CUTLENGTH { get; set; }

        [StringLength(10)]
        public string LOCABIN { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        [StringLength(4)]
        public string BALEYR { get; set; }

        [StringLength(30)]
        public string BALENO { get; set; }
    }
}
