using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Improvar.Models
{
    public class MDEPTHOD
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string DEPTCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SRLNO { get; set; }

        [StringLength(5)]
        public string HODCD { get; set; }

        [StringLength(40)]
        public string HODNM { get; set; }

        public long? AMT_FR { get; set; }

        public long? AMT_TO { get; set; }
    }
}