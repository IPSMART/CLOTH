namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_AMTTYPE")]
    public partial class M_AMTTYPE
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
        [StringLength(4)]
        public string AMTCD { get; set; }

        [Required]
        [StringLength(40)]
        public string AMTNM { get; set; }

        [Required]
        [StringLength(1)]
        public string ADDLESS { get; set; }

        [Required]
        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string CALCCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string CALCTYPE { get; set; }

        [StringLength(2)]
        public string TAXCODE { get; set; }

        [StringLength(100)]
        public string CALCFORMULA { get; set; }

        [Required]
        [StringLength(1)]
        public string SALPUR { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
