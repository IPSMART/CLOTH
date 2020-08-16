namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_DLY_ATN")]
    public partial class T_DLY_ATN
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
        public DateTime ATNDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string EMPCD { get; set; }

        [StringLength(2)]
        public string SHIFTCD { get; set; }

        [StringLength(5)]
        public string INTIME { get; set; }

        [StringLength(5)]
        public string OUTTIME { get; set; }

        [StringLength(5)]
        public string LUNCHIN { get; set; }

        [StringLength(5)]
        public string LUNCHOUT { get; set; }

        public double? WRKHRS { get; set; }

        public double? WTHPAY { get; set; }

        public double? OTHRS { get; set; }

        public double? ATTND { get; set; }

        public double? ABSNT { get; set; }

        public double? WOFF { get; set; }

        [StringLength(4)]
        public string ATNCD { get; set; }

        public double? ATNDAY { get; set; }

        [StringLength(1)]
        public string FDUTY { get; set; }

        [StringLength(50)]
        public string REMKS { get; set; }

        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(6)]
        public string DOCNO { get; set; }

        [StringLength(1)]
        public string LATEATN { get; set; }

        public double? PDHOL { get; set; }
    }
}
