namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_JOBMSTSUB_STDRT")]
    public partial class M_JOBMSTSUB_STDRT
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
        [StringLength(5)]
        public string SJOBCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string JOBPRCCD { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime EFFDT { get; set; }

        public decimal JOBRT { get; set; }

        public long M_AUTONO { get; set; }
    }
}
