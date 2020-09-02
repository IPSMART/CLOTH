namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_KARDTL")]
    public partial class T_KARDTL
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

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RSLNO { get; set; }

        [StringLength(30)]
        public string PROGAUTONO { get; set; }

        public short? PROGSLNO { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(1)]
        public string STKDRCR { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [Required]
        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public short? NOS { get; set; }

        public decimal? CUTLENGTH { get; set; }

        public decimal? QNTY { get; set; }

        public decimal? STKQNTY { get; set; }

        public decimal? RATE { get; set; }

        [StringLength(25)]
        public string BARNO { get; set; }

        [StringLength(1)]
        public string SAMPLE { get; set; }

       
    }
}
