using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class PENDING_POPUP
    {
        public string PROGAUTONO { get; set; }
        public string PROGSLNO { get; set; }
        public string PROGAUTOSLNO { get; set; }        
        public string PROGUNIQNO { get; set; }
        public string ITGRPNM { get; set; }
        public string ITGRPCD { get; set; }
        public string FABITNM { get; set; }        
        public string ITCD { get; set; }
        public string UOMCD { get; set; }
        public string COLRNM { get; set; }
        public string STYLENO { get; set; }
        public string COLRCD { get; set; }
        public string SIZECD { get; set; }
        public string SHADE { get; set; }        
        public double? BALNOS  { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? BALQNTY { get; set; }
        public string ITREMARK { get; set; }
        public string SAMPLE { get; set; }
        public string COMMONUNIQBAR { get; set; }
        public double CUTLENGTH { get; set; }
        public string ITNM { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DOCDT { get; set; }
        public string DOCNO { get; set; }
        public bool Checked { get; set; }
        public string BARNO { get; set; }
        public string ORDDOCNO { get; set; }        
        public double? ORDSLNO { get; set; }
        public string MAKESTYLENO { get; set; }
    }
}