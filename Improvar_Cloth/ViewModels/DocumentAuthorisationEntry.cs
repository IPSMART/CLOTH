using System.Collections.Generic;
using Improvar.Models;
using System;


namespace Improvar.ViewModels
{
    public class DocumentAuthorisationEntry : Permission
    {
        public List<TCNTRLAUTH> TCNTRLAUTH { get; set; }
        public T_CNTRL_DOC_PASS T_CNTRL_DOC_PASS { get; set; }
        public string FROMDT { get; set; }
        public string TODT { get; set; }
        public string SHOW_RECORD { get; set; }
        public string DOCTYPE { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
    }
}