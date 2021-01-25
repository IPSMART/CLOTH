using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Improvar.ViewModels
{
    public class TransactionPhyStockEntry :Permission
    {
        public T_PHYSTK_HDR T_PHYSTK_HDR { get; set; }  
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_PHYSTK T_PHYSTK { get; set; }
        public List<TBILTY> TBILTY { get; set; }
        public List<TBILTY_POPUP> TBILTY_POPUP { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string DRCR { get; set; }
        public string GONM { get; set; }
        public string PRCNM { get; set; }
        public string PRCCD { get; set; }
        
    }
}