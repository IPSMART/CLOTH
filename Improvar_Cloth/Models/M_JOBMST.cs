namespace Improvar.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("M_JOBMST")]
    public partial class M_JOBMST
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(2)]
        public string JOBCD { get; set; }

        [Required]
        [StringLength(15)]
        public string JOBNM { get; set; }

        [StringLength(8)]
        public string EXPGLCD { get; set; }

        [StringLength(8)]
        public string SCGLCD { get; set; }

        [StringLength(1)]
        public string JBATCHNINI { get; set; }
        public long M_AUTONO { get; set; }
        [StringLength(4)]
        public string PRODGRPCD { get; set; }
        [StringLength(8)]
        public string HSNSACCD { get; set; }
        [StringLength(3)]
        public string UOMCD { get; set; }
        [StringLength(2)]
        public string RMTRLJOBCD { get; set; }
        [StringLength(20)]
        public string IMTRLJOBCD { get; set; }
        [StringLength(30)]
        public string ISSMTRLDESC { get; set; }
        public double? JOBSEQ { get; set; }
    }
}
