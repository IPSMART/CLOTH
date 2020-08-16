namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_DLYTARACH_DTL")]
    public partial class T_DLYTARACH_DTL
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
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        public short SLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string LINENO { get; set; }

        public byte? MPR_TAILOR { get; set; }

        public byte? MPR_TRIMMER { get; set; }

        public byte? MPR_HELPER { get; set; }

        public byte? MPR_CHECKER { get; set; }

        public double? WORKHRS { get; set; }

        public double? OTHRS { get; set; }

        [StringLength(50)]
        public string ITREMARK { get; set; }
    }
}
