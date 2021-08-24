namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_VCH_BL_REM")]
    public partial class T_VCH_BL_REM
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
        [StringLength(30)]
        [Column(Order = 0)]
        public string AUTONO { get; set; }


        [Key]
        [Column(Order = 1)]
        public int SLNO { get; set; }

        
        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(30)]
        public string BL_AUTONO { get; set; }

        public int BL_SLNO { get; set; }

        [Required]
        [StringLength(12)]
        public string CTG { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DUEDT { get; set; }

        [StringLength(50)]
        public string REM { get; set; }
        public double? AMT { get; set; }
    }
}
