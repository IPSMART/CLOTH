namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_JOBMST")]
    public partial class M_JOBMST
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

        [Required]
        [StringLength(4)]
        public string PRODGRPCD { get; set; }

        [Required]
        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(3)]
        public string UOMCD { get; set; }

        [StringLength(2)]
        public string RMTRLJOBCD { get; set; }

        [StringLength(20)]
        public string IMTRLJOBCD { get; set; }

        [StringLength(30)]
        public string ISSMTRLDESC { get; set; }

        public byte? JOBSEQ { get; set; }
    }
}
