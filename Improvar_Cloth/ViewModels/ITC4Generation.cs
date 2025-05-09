using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Improvar.ViewModels
{
    public class ITC4Generation : Permission
    {
        public T_ITC4_HDR T_ITC4_HDR { get; set; }
        public T_ITC4_DTL T_ITC4_DTL { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<TITC4DTL> TITC4DTL { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
    }
}