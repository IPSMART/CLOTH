namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DOC_AUTH")]
    public partial class M_DOC_AUTH
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
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte LVL { get; set; }

        [Key]
        [Column(Order = 2)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFF_DT { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [StringLength(5)]
        public string AUTHCD { get; set; }

        public double? AMT_FR { get; set; }

        public double? AMT_TO { get; set; }

        public long M_AUTONO { get; set; }
    }
}
