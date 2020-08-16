namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_CNTRL_DOC_PASS")]
    public partial class T_CNTRL_DOC_PASS
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte SLNO { get; set; }

        public byte? PASS_LEVEL { get; set; }

        [StringLength(5)]
        public string AUTHCD { get; set; }
        [StringLength(1000)]
        public string AUTHREM { get; set; }
    }
}
