namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_COMP")]
    public partial class M_COMP
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Required]
        [StringLength(60)]
        public string COMPNM { get; set; }

        [StringLength(60)]
        public string ADD1 { get; set; }

        [StringLength(60)]
        public string ADD2 { get; set; }

        [StringLength(60)]
        public string ADD3 { get; set; }

        [StringLength(60)]
        public string ADD4 { get; set; }

        [StringLength(60)]
        public string ADD5 { get; set; }

        [StringLength(60)]
        public string ADD6 { get; set; }

        [StringLength(60)]
        public string ADD7 { get; set; }

        [StringLength(80)]
        public string BLDGNO { get; set; }

        [StringLength(80)]
        public string PREMISES { get; set; }

        [StringLength(80)]
        public string FLOORNO { get; set; }

        [StringLength(80)]
        public string ROADNAME { get; set; }

        [StringLength(60)]
        public string EXTADDR { get; set; }

        [StringLength(80)]
        public string LOCALITY { get; set; }

        [StringLength(80)]
        public string LANDMARK { get; set; }

        [Required]
        [StringLength(35)]
        public string STATE { get; set; }

        [StringLength(35)]
        public string DISTRICT { get; set; }

        [StringLength(10)]
        public string PIN { get; set; }

        [Required]
        [StringLength(35)]
        public string COUNTRY { get; set; }

        [StringLength(15)]
        public string SHORTNM { get; set; }

        [StringLength(16)]
        public string ADHAARNO { get; set; }

        [StringLength(15)]
        public string PANNO { get; set; }

        [StringLength(25)]
        public string CINNO { get; set; }

        [StringLength(2)]
        public string CMCOMPTYPE { get; set; }

        [StringLength(60)]
        public string PROPNAME { get; set; }

        public long M_AUTONO { get; set; }
    }
}
