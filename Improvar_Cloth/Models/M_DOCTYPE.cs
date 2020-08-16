namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DOCTYPE")]
    public partial class M_DOCTYPE
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
        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(35)]
        public string DOCNM { get; set; }

        public DateTime? FRDT { get; set; }

        public DateTime? TODT { get; set; }

        [StringLength(4)]
        public string DOCPRFX { get; set; }

        [StringLength(10)]
        public string DOCTYPE { get; set; }

        [StringLength(1)]
        public string DOCJNRL { get; set; }

        [StringLength(100)]
        public string DOCFOOT { get; set; }

        [StringLength(1)]
        public string DOCPRN { get; set; }

        [StringLength(100)]
        public string DOCNOPATTERN { get; set; }

        public byte? DOCNOMAXLENGTH { get; set; }

        [StringLength(1)]
        public string DOCNOWOZERO { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(1)]
        public string PRO { get; set; }

        [StringLength(1)]
        public string FDATE { get; set; }

        [StringLength(1)]
        public string BACKDATE { get; set; }

        [StringLength(5)]
        public string MAINDOCCD { get; set; }

        [StringLength(20)]
        public string FLAG1 { get; set; }
        
    }
}
