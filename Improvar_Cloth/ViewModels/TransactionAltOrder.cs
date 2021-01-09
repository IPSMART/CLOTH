using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TransactionAltOrder :Permission
    {
        public T_STCHALT T_STCHALT { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_STCHALT_DTL T_STCHALT_DTL { get; set; }
       public T_STCHALT_DTL_COMP T_STCHALT_DTL_COMP { get; set; }
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        
        public string CMNO { get; set; }
        public string AUTONO { get; set; }
        public string MOBILE { get; set; }
        public string RTDEBNM { get; set; }
        public string ADDR { get; set; }
        public string RTMOBILE { get; set; }
        public string STCHCD { get; set; }
        public string FLDDESC { get; set; }
        public short? FLDLEN { get; set; }
        public string FLDDATACOMBO { get; set; }
        public bool INC_RATE { get; set; }
        public string FLDTYPE { get; set; }
        


    }
    //public class TransactionAltOrdertttttt
    //{


    //}

}