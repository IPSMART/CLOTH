namespace Improvar.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("T_GSTITC4_REC")]
    public partial class T_GSTITC4_REC
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

        [StringLength(1)]
        public string RECOACTION { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }

        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(15)]
        public string GSTNO { get; set; }

        [StringLength(50)]
        public string JOBNM { get; set; }

        [Required]
        [StringLength(16)]
        public string CHNO { get; set; }

        public DateTime? CHDT { get; set; }

        [Required]
        [StringLength(16)]
        public string ORGCHNO { get; set; }

        public DateTime? ORGCHDT { get; set; }

        [Required]
        [StringLength(8)]
        public string HSNCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string ITNM { get; set; }

        public double QNTY { get; set; }

        [Required]
        [StringLength(3)]
        public string UOMCD { get; set; }

        public double AMT { get; set; }

        [StringLength(16)]
        public string OTHJWCHNO { get; set; }

        public DateTime? OTHJWCHDT { get; set; }

        [StringLength(8)]
        public string OTHJWSLCD { get; set; }

        [StringLength(15)]
        public string OTHJWGSTNO { get; set; }

        [StringLength(16)]
        public string INVNO_DIR { get; set; }

        public DateTime? INVDT_DIR { get; set; }
        [StringLength(3)]
        public string SHORTUOMCD { get; set; }
        public double SHORTQNTY { get; set; }
        [Required]
        [StringLength(5)]
        public string SHEETNM { get; set; }
        [Required]
        [StringLength(30)]
        public string PROC_AUTONO { get; set; }
    }
}
