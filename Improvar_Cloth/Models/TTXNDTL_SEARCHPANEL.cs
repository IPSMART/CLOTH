using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TSORDDTL_SEARCHPANEL
    {
        public short SLNO { get; set; }
        public string AUTONO { get; set; }
        public string DOCCD { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string SB_MADEDT { get; set; }
        public string SB_MADEBY_ID { get; set; }
        public string SB_MADEBY_NAME { get; set; }
        public bool Checked { get; set; }
    }
}