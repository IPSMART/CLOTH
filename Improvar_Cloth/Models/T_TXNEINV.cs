namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNEINV")]
    public partial class T_TXNEINV
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

        [StringLength(30)]
        public string ACKNO { get; set; }

        public DateTime? ACKDT { get; set; }

        [StringLength(100)]
        public string IRNNO { get; set; }

        [StringLength(4000)]
        public string SIGNQRCODE { get; set; }

        [StringLength(1)]
        public string CANCELRSN { get; set; }

        [StringLength(100)]
        public string CANCELREM { get; set; }

        public DateTime? CANCELDT { get; set; }
    }
}
