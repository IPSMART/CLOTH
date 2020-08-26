namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNMEMO")]
    public partial class T_TXNMEMO
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

        [StringLength(50)]
        public string NM { get; set; }

        [StringLength(12)]
        public string MOBILE { get; set; }

        [StringLength(30)]
        public string CITY { get; set; }

        [StringLength(200)]
        public string ADDR { get; set; }
    }
}
