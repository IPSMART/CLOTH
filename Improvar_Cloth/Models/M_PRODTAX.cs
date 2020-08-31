namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_PRODTAX")]
    public partial class M_PRODTAX
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
        [StringLength(4)]
        public string PRODGRPCD { get; set; }

        [Key]
        [Column(Order = 1)]

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string TAXGRPCD { get; set; }

        public double? IGSTPER { get; set; }

        public double? CGSTPER { get; set; }

        public double? SGSTPER { get; set; }

        public double? CESSPER { get; set; }

        [Key]
        [Column(Order = 3)]
         public double FROMRT { get; set; }

         public double TORT { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
