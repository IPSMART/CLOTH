using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
namespace Improvar.ViewModels
{
    public class PriceListCodeMasterEntry : Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_PRCLST M_PRCLST { get; set; }
        
        public List<DropDown_list> DropDown_list { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }
        public bool Deactive { get; set; }

    }
}