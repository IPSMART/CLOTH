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
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string CMNO { get; set; }
        public string AUTONO { get; set; }
        public string MOBILE { get; set; }
        public string RTDEBNM { get; set; }
        public string ADDR { get; set; }
        public string RTMOBILE { get; set; }
        public string STCHNM { get; set; }
        


    }
}