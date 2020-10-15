using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class PENDINGORDER
    {
        public string ORDNO { get; set; }
        public string ORDDT { get; set; }
        public string ITGRPNM { get; set; }
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        public string SIZECD { get; set; }
        public double ORDQTY { get; set; }
        public double BALQTY { get; set; }
        public string ORDAUTONO { get; set; }
        public string ORDSLNO { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        public bool Ord_Checked { get; set; }
        public short SLNO { get; set; }
    }
}