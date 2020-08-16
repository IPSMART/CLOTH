using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMCOLOR
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
        [StringLength(12)]
        public string ITCD { get; set; }
        public byte? SLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COLRCD { get; set; }

        [StringLength(20)]
        public string COLRNM { get; set; }
        
        public bool Checked { get; set; }
        public bool IChecked { get; set; }
    }
}