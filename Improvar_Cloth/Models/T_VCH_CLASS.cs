namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_VCH_CLASS")]
    public partial class T_VCH_CLASS
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

        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(6)]
        public string DOCNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DSLNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }

        [StringLength(8)]
        public string REFCD { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(8)]
        public string CLASS2CD { get; set; }

        public double? AMT { get; set; }

        public double? CURR_AMT { get; set; }

        [StringLength(100)]
        public string REM1 { get; set; }

        [StringLength(2)]
        public string DATATAG { get; set; }
    }
}
