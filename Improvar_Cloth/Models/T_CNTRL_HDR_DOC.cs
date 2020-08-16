namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_CNTRL_HDR_DOC")]
    public partial class T_CNTRL_HDR_DOC
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

        [StringLength(100)]
        public string DOC_FLNAME { get; set; }

        [StringLength(10)]
        public string DOC_EXTN { get; set; }

        [StringLength(15)]
        public string DOC_CTG { get; set; }

        [StringLength(300)]
        public string DOC_DESC { get; set; }
    }
}
