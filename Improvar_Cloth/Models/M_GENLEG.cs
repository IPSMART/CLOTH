namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_GENLEG")]
    public partial class M_GENLEG
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
        [StringLength(8)]
        public string GLCD { get; set; }

        [Required]
        [StringLength(45)]
        public string GLNM { get; set; }

        [Required]
        [StringLength(3)]
        public string GLHDGRPCD { get; set; }

        [StringLength(5)]
        public string GLTBGRPCD { get; set; }

        [StringLength(5)]
        public string GLSUBCTGCD { get; set; }

        [StringLength(1)]
        public string SLCDMUST { get; set; }

        [StringLength(1)]
        public string REFCDMUST { get; set; }

        [StringLength(1)]
        public string CLASS1CDMUST { get; set; }

        [StringLength(1)]
        public string CLASS2CDMUST { get; set; }

        [StringLength(12)]
        public string STATCTGNM { get; set; }

        [StringLength(10)]
        public string HSNCODE { get; set; }

        [StringLength(1)]
        public string LINKCD { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(3)]
        public string CHQFMTCD { get; set; }

        [StringLength(20)]
        public string ACNO { get; set; }

        [StringLength(30)]
        public string BRNCHNM { get; set; }

        [StringLength(20)]
        public string IFSCCD { get; set; }

        [StringLength(20)]
        public string MICRCD { get; set; }

        [StringLength(1)]
        public string BLWS { get; set; }

        [StringLength(1)]
        public string EMPLNAC { get; set; }

        [StringLength(50)]
        public string BRANCHADD1 { get; set; }

        [StringLength(50)]
        public string BRANCHADD2 { get; set; }

        [StringLength(8)]
        public string RPLGLCD { get; set; }
        [StringLength(60)]
        public string FULLNAME { get; set; }
    }
}
