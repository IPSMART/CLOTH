namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEM")]
    public partial class M_SITEM
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
        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }

        [StringLength(8)]
        public string FABITCD { get; set; }

        [StringLength(40)]
        public string QUALITY { get; set; }

        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(30)]
        public string STYLENO { get; set; }

        [StringLength(4)]
        public string BRANDCD { get; set; }

        [StringLength(4)]
        public string SBRANDCD { get; set; }

        [StringLength(10)]
        public string GENDER { get; set; }

        [StringLength(4)]
        public string COLLCD { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public short? PCSPERSET { get; set; }

        [StringLength(3)]
        public string UOMCD { get; set; }

        [StringLength(20)]
        public string FEATURE { get; set; }

        [StringLength(20)]
        public string DMNSN { get; set; }

        [StringLength(1)]
        public string SZWISEDTL { get; set; }

        [StringLength(1)]
        public string COLRWISEDTL { get; set; }

        public decimal? MINPURQTY { get; set; }

        public long M_AUTONO { get; set; }

        public short? COLRPERSET { get; set; }

        [StringLength(8)]
        public string LINKITCD { get; set; }

        public virtual M_COLOR M_COLOR { get; set; }

        public virtual M_GROUP M_GROUP { get; set; }

      
    }
}
