using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TransactionKhasraEntry : Permission
    {
        public T_BILTY_HDR T_BILTY_HDR { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_BILTY T_BILTY { get; set; }
        public List<TBILTYKHASRA> TBILTYKHASRA { get; set; }
        public List<TBILTYKHASRA_POPUP> TBILTYKHASRA_POPUP { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string DRCR { get; set; }
        public string SLNM { get; set; }
        public string REGMOBILE { get; set; }
        public string STRTNO { get; set; }
    }
}