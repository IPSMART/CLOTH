namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_VCH_DET")]
    public partial class T_VCH_DET
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
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(6)]
        public string DOCNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }

        [StringLength(1)]
        public string DRCR { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(8)]
        public string R_GLCD { get; set; }

        [StringLength(8)]
        public string R_SLCD { get; set; }

        [StringLength(200)]
        public string T_REM { get; set; }

        [StringLength(4)]
        public string SUBLOCCD { get; set; }

        public double? AMT { get; set; }

        public double? QTY { get; set; }

        [StringLength(10)]
        public string CHQNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CHQDT { get; set; }

        [StringLength(30)]
        public string BANK_NAME { get; set; }

        [StringLength(30)]
        public string RTGSNO { get; set; }

        [StringLength(8)]
        public string BANK_CODE { get; set; }

        public double? CURR_AMT { get; set; }

        public double? CURR_RATE { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? BANK_DT { get; set; }

        [StringLength(30)]
        public string BRSAUTONO { get; set; }

        public double? INTPER { get; set; }

        public int? INTDAY { get; set; }
        public int? OSLNO { get; set; }
        [StringLength(2)]
        public string OTHTRCD { get; set; }
        [StringLength(1)]
        public string INTTDS { get; set; }
    }
}
