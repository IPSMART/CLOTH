using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Improvar.Models
{
    public class MSUBLEGTAX
    {

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }
        public string COMPNM { get; set; }

        public double? TDSLMT { get; set; }

        public double? LWRRT { get; set; }

        public double? INTPER { get; set; }

        public bool Checked { get; set; }
        public int SLNO { get; set; }


    }
}