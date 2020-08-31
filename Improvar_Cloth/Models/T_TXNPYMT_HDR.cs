namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNPYMT_HDR")]
    public partial class T_TXNPYMT_HDR
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
        public string RTDEBCD { get; set; }

        [Required]
        [StringLength(1)]
        public string DRCR { get; set; }

        [StringLength(2)]
        public string VCHRTYPE { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

         public double? AMTTORECD { get; set; }

         public double? RECDAMT { get; set; }

         public double? DUEAMT { get; set; }
    }
}
