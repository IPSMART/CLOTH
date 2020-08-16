namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_EMPMAS_DET")]
    public partial class M_EMPMAS_DET
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
        [StringLength(10)]
        public string EMPCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EDATE { get; set; }

        [StringLength(4)]
        public string DEPTCD { get; set; }

        [StringLength(4)]
        public string GRADECD { get; set; }

        [StringLength(5)]
        public string DESIGCD { get; set; }

        [StringLength(4)]
        public string LOCCD { get; set; }

        [StringLength(4)]
        public string COMPCD { get; set; }

        public long? M_AUTONO { get; set; }
    }
}
