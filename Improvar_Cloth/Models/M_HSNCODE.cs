namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_HSNCODE")]
    public partial class M_HSNCODE
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
        [StringLength(12)]
        public string HSNCODE { get; set; }

        [StringLength(1000)]
        public string HSNDESCN { get; set; }

        [StringLength(1)]
        public string HSNTYPE { get; set; }

        [StringLength(1)]
        public string RCHGS { get; set; }

        public long M_AUTONO { get; set; }
    }
}
