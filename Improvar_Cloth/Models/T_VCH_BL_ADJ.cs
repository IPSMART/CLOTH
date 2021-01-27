namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_VCH_BL_ADJ")]
    public partial class T_VCH_BL_ADJ
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }

        [StringLength(30)]
        public string I_AUTONO { get; set; }

        public int? I_SLNO { get; set; }

        [StringLength(30)]
        public string R_AUTONO { get; set; }

        public int? R_SLNO { get; set; }

        public double? I_AMT { get; set; }

        public double? R_AMT { get; set; }

        public double? ADJ_AMT { get; set; }
        [StringLength(3)]
        public string FLAG { get; set; }
        [StringLength(75)]
        public string PYMTREM { get; set; }
    }
}
