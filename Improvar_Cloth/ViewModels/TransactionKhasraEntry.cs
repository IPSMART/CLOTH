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
        public T_BALE_HDR T_BALE_HDR { get; set; }
        public T_TXNTRANS T_TXNTRANS { get; set; }
        public T_TXN T_TXN { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<TBILTYKHASRA> TBILTYKHASRA { get; set; }
        public List<TBILTYKHASRA_POPUP> TBILTYKHASRA_POPUP { get; set; }
        public List<DocumentType> DocumentType { get; set; }    
        public string SLNM { get; set; }
        public string REGMOBILE { get; set; }
        public string STRTNO { get; set; }
        public string GONM { get; set; }
        public string TRANSLNM { get; set; }
        public List<VECHLTYPE> VECHLTYPE { get; set; }
        public List<TRANSMODE> TRANSMODE { get; set; }
        public string SLNM1 { get; set; }
        public string BALENO { get; set; }
        public string GONM1 { get; set; }

        public string BALECOUNT { get; set; }
        public string M_SLIP_NO { get; set; }
        public string ISSUEDT { get; set; }
    }
}