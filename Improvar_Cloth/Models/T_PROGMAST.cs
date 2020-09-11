namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_PROGMAST")]
    public partial class T_PROGMAST
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

        [StringLength(5)]
        public string LINECD { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [Required]
        [StringLength(25)]
        public string BARNO { get; set; }

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

        public decimal? NOS { get; set; }

        public decimal? QNTY { get; set; }

        [StringLength(50)]
        public string ITREMARK { get; set; }

        public decimal? RATE { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        [StringLength(15)]
        public string MILLNM { get; set; }

        public decimal? DIA { get; set; }

        public decimal? CUTLENGTH { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        [Required]
        [StringLength(15)]
        public string PROGUNIQNO { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

        [StringLength(30)]
        public string ORGBATCHAUTONO { get; set; }

        public short? ORGBATCHSLNO { get; set; }

        [StringLength(30)]
        public string RECAUTONO { get; set; }

        [StringLength(1)]
        public string SAMPLE { get; set; }
       
    }
}
