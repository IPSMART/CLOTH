namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNEWB")]
    public partial class T_TXNEWB
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(8)]
        public string TRANSLCD { get; set; }

        [StringLength(100)]
        public string LRNO { get; set; }

        public DateTime? LRDT { get; set; }

        [StringLength(16)]
        public string LORRYNO { get; set; }

        [StringLength(2)]
        public string TRANSMODE { get; set; }

        [StringLength(1)]
        public string VECHLTYPE { get; set; }

        public decimal? DISTANCE { get; set; }

        [StringLength(20)]
        public string EWAYBILLNO { get; set; }

        public DateTime? EWAYBILLDT { get; set; }

        public DateTime? EWAYBILLVALID { get; set; }

        [StringLength(6)]
        public string GOCD { get; set; }
        [StringLength(4)]
        public string REASONCD { get; set; }

        [StringLength(100)]
        public string REASONREM { get; set; }

        [StringLength(40)]
        public string CHNG_BY { get; set; }

        public DateTime? CHNG_TIME { get; set; }
    }
}
