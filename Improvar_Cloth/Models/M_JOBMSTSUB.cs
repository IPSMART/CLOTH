namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_JOBMSTSUB")]
    public partial class M_JOBMSTSUB
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
        [StringLength(5)]
        public string SJOBCD { get; set; }

        [Required]
        [StringLength(35)]
        public string SJOBNM { get; set; }

        [StringLength(20)]
        public string SJOBSTYLE { get; set; }

        [StringLength(10)]
        public string SJOBMCHN { get; set; }

        [Required]
        [StringLength(2)]
        public string JOBCD { get; set; }

        [StringLength(20)]
        public string SCATE { get; set; }

        [StringLength(10)]
        public string SJOBSIZE { get; set; }

        [StringLength(15)]
        public string SJOBATCH { get; set; }

        public decimal? SJOBSAM { get; set; }

        public long M_AUTONO { get; set; }
    }
}
