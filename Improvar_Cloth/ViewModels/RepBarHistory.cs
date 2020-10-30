using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class RepBarHistory : Permission
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
        
        public string TDT { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public string DOCNM { get; set; }
        public string TEXTBOX6 { get; set; }
    }
}