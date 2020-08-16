using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Improvar.Models
{
    public class MDOCAUTHLAVEL5
    {

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(2)]
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

        [StringLength(40)]
        public string AUTHNM { get; set; }

        public double? AMT_FR { get; set; }

        public double? AMT_TO { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long M_AUTONO { get; set; }

        public bool Checked { get; set; }
    }
}