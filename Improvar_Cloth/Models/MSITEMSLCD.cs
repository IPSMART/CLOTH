using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMSLCD
    {
        public bool Checked { get; set; }
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string ITCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(30)]
        public string PDESIGN { get; set; }

         public double? JOBRT { get; set; }
        public string SRLNO { get; set; }
        public string SLNM { get; set; }
    }
}