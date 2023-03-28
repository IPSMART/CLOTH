using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TransactionStkConvMobEntry : Permission
    {
        public T_MOBDTL T_MOBDTL { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public string SLNM { get; set; }
        public string ITGRPCD { get; set; }
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list_GODOWN> DropDown_list_GODOWN { get; set; }

        public List<TMOBDTL> TMOBDTL { get; set; }

    }
}