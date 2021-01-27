namespace Improvar.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("T_VCH_BL")]
    public partial class T_VCH_BL
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

        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(1)]
        public string DRCR { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(8)]
        public string CONSLCD { get; set; }

        [StringLength(8)]
        public string AGSLCD { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }

        public double? AMT { get; set; }

        [StringLength(20)]
        public string BLNO { get; set; }

        public DateTime? BLDT { get; set; }

        [StringLength(20)]
        public string REFNO { get; set; }

        public DateTime? DUEDT { get; set; }

        [StringLength(3)]
        public string VCHTYPE { get; set; }
        [StringLength(30)]
        public string ORDNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDDT { get; set; }
        public int? CRDAYS { get; set; }
        public double? ITAMT { get; set; }
        [StringLength(20)]
        public string LRNO { get; set; }
        public DateTime? LRDT { get; set; }
        [StringLength(50)]
        public string TRANSNM { get; set; }
        public double? BLAMT { get; set; }

        [StringLength(3)]
        public string FLAG { get; set; }
        [StringLength(8)]
        public string SAGSLCD { get; set; }
    }
}
