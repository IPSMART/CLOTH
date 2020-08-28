namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_STKTYPE")]
    public partial class M_STKTYPE
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
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [Required]
        [StringLength(15)]
        public string STKNAME { get; set; }

        [StringLength(15)]
        public string SHORTNM { get; set; }

        [StringLength(5)]
        public string FLAG1 { get; set; }

        [StringLength(1)]
        public string ISDEFAULT { get; set; }

        public long M_AUTONO { get; set; }
    }
}
