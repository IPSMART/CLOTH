using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMMEASURE
    {
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
        public byte SLNO { get; set; }

        [Required]
        [StringLength(30)]
        public string MDESC { get; set; }

        public decimal MVAL { get; set; }

        [StringLength(30)]
        public string REM { get; set; }

        [StringLength(1)]
        public string INACTIVE_TAG { get; set; }
        public bool Checked { get; set; }
       
    }
}