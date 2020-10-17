using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class PENDINGORDER
    {
        public string ORDDOCNO { get; set; }
        public string ORDDOCDT { get; set; }
        public string ITGRPNM { get; set; }
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        public string SIZECD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double ORDQTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double BALQTY { get; set; }
        public string ORDAUTONO { get; set; }
        public string ORDSLNO { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        public bool Ord_Checked { get; set; }
        public short SLNO { get; set; }
    }
}