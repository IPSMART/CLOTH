using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMBOM_PRINT
    {
        public byte SLNO { get; set; }
        public bool Checked { get; set; }
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
    }
}