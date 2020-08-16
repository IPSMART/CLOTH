namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SCHEME_HDR")]
    public partial class M_SCHEME_HDR
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
        [StringLength(6)]
        public string SCMCD { get; set; }

        public DateTime? SCMDT { get; set; }

        [Required]
        [StringLength(30)]
        public string SCMNM { get; set; }

        [StringLength(200)]
        public string SCMREM { get; set; }

        [Required]
        [StringLength(1)]
        public string SCMTYPE { get; set; }

        [Required]
        [StringLength(1)]
        public string SCMBASIS { get; set; }

        public DateTime? FRDT { get; set; }

        public DateTime? TODT { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
