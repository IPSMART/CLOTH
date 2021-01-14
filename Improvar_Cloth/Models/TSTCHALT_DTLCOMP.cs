using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Improvar.Models
{
    public class TSTCHALT_DTLCOMP
    {
        public int SLNO { get; set; }

        [StringLength(7)]
        public string FLDCD { get; set; }

        [Required]
        [StringLength(500)]
        public string FLDVAL { get; set; }

        [StringLength(100)]
        public string FLDREM { get; set; }

        [Required]
        [StringLength(1)]
        public string FLDTYPE { get; set; }
        public string STCHCD { get; set; }
        public short QNTY { get; set; }

        public double? RATE { get; set; }
        public string FLDDESC { get; set; }

    }
    public class TSTCHALT_MEASUREMENT
    {
        public int SLNO { get; set; }        
        public string FLDCD { get; set; }        
        public string FLDVAL { get; set; }
        public string STCHNM { get; set; }
        public string FLDTYPE { get; set; }
        public string STCHCD { get; set; }
        public short QNTY { get; set; }
        public double? RATE { get; set; }
        public List<TSTCHALT_DTLCOMP> TSTCHALT_DTLCOMP { get; set; }
    }
}