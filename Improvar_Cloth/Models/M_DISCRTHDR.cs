namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DISCRTHDR")]
    public partial class M_DISCRTHDR
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
        [Column(Order = 0)]
        [StringLength(4)]
        public string DISCRTCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [Required]
        [StringLength(1)]
        public string DISCCALCTYPE { get; set; }

        [StringLength(200)]
        public string REMARKS { get; set; }

        public long M_AUTONO { get; set; }
    }
}
