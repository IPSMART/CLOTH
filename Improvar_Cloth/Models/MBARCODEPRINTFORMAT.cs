using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MBARCODEPRINTFORMAT
    {
        public bool Checked { get; set; }
        public short? EMD_NO { get; set; }

        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        public byte? SLNO { get; set; }

        [StringLength(100)]
        public string FMTTEXT { get; set; }

        [StringLength(1)]
        public string ALIGN { get; set; }

        public byte? FONTSZ { get; set; }

        [StringLength(1)]
        public string MARGINE { get; set; }
        
        public long M_AUTONO { get; set; }
    }
}