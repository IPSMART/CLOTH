using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TransactionPackingSlipEntry : Permission
    {
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_TXN T_TXN { get; set; }
        public T_TXN_LINKNO T_TXN_LINKNO { get; set; }
        public T_TXNACK T_TXNACK { get; set; }
        public T_TXNAMT T_TXNAMT { get; set; }
        public T_TXNDTL T_TXNDTL { get; set; }
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_TXNPYMT T_TXNPYMT { get; set; }
        public T_TXNSTATUS T_TXNSTATUS { get; set; }
        public T_TXNTRANS T_TXNTRANS { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public string SLNM { get; set; }
        public string CONSLNM { get; set; }
        public string AGSLNM { get; set; }
        public string SAGSLNM { get; set; }
        public double TOTNOS { get; set; }
        public double TOTQNTY { get; set; }
        public double TOTTAX { get; set; }
        public bool RoundOff { get; set; }
    }
}