using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TBILTY_POPUP
    {
        public short SLNO { get; set; }
        public string BLAUTONO { get; set; }
        public string LRNO { get; set; }
        public string LRDT { get; set; }
        public string BALENO { get; set; }
        public bool Checked { get; set; }
        public string PREFNO { get; set; }
        public string PREFDT { get; set; }
    }
}