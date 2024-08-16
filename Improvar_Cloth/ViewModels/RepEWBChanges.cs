using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System;

namespace Improvar.ViewModels
{
    public class RepEWBChanges :Permission
    {
        public string ActionName { get; set; }
        public string CaptionName { get; set; }
        public string OtherPara { get; set; }
        public string RepType { get; set; }
        public string TEXTBOX1 { get; set; }
        public string TEXTBOX2 { get; set; }
        public bool Checkbox1 { get; set; }
        public bool Checkbox2 { get; set; }
        public string DOCNO { get; set; }
        public string FDOCNO { get; set; }
        public string TDOCNO { get; set; }
        public string DOCCD { get; set; }
        public bool AskSlCd { get; set; }
        public string FDT { get; set; }
        public string BLDT { get; set; }
        public string AUTONO { get; set; }
        public string BLNO { get; set; }
        public string SLNM { get; set; }
        public string REASONREM { get; set; }
        public string REASONCD { get; set; }
        public string LORRYNO1 { get; set; }
        public string LORRYNO { get; set; }
        public string LRDT1 { get; set; }
        public string LRDT { get; set; }
        public string LRNO1 { get; set; }
        public string LRNO { get; set; }
        public string TRSLNM1 { get; set; }
        public string TRSLCD1 { get; set; }
        public string TRSLNM { get; set; }
        public string TRSLCD { get; set; }
        public string EWAYBILLDT { get; set; }
        public string EWAYBILLNO { get; set; }
        public string GSTNO { get; set; }
        public string PDESIGN { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double BLAMT { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public string AGSLCD { get; set; }
        public string AGSLNM { get; set; }
        public string AGSLCD1 { get; set; }
        public string AGSLNM1 { get; set; }
        public string CARRIAGEAMT { get; set; }
    }
}