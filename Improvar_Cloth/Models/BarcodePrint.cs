using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class BarcodePrint
    {
        public short? EMD_NO { get; set; }
        public string CLCD { get; set; }
        public string DTAG { get; set; }
        public string TTAG { get; set; }
        public string AUTONO { get; set; }
        public string TAXSLNO { get; set; }
        public string BARNO { get; set; }
        public string ITGRPNM { get; set; }
        public string FABITNM { get; set; }
        public string STYLENO { get; set; }
        public string NOS { get; set; }
        public string WPRATE { get; set; }
        public string CPRATE { get; set; }
        public string MTR { get; set; }
        public bool Checked { get; set; }
    }
}