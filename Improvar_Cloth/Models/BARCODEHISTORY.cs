using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class BARCODEHISTORY
    {
        public string AUTONO { get; set; }
        public short SLNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? OUTQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? INQNTY { get; set; }        
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public string DISCPER { get; set; }
        public string PREFNO { get; set; }
        public string SLNM { get; set; }
        public string SLCD { get; set; }
        public string DISTRICT { get; set; }
        public string LOCNM { get; set; }
        public string DOCDT { get; set; }
        public string DOCNO { get; set; }
        public string DOCNM { get; set; }
        public bool Checked { get; set; }
        public string STKDRCR { get; set; }
        public double? QNTY { get; set; }
        public string itstyleno { get; set; }
        public string itremarks { get; set; }
        public string colornm { get; set; }
        public string UOM { get; set; }
        public string GONM { get; set; }

    }
}