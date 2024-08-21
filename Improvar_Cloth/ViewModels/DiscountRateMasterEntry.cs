using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class DiscountRateMasterEntry : Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_DISCRTHDR M_DISCRTHDR { get; set; }
        public M_DISCRT M_DISCRT { get; set; }
        public M_SCMITMGRP_HDR M_SCMITMGRP_HDR { get; set; }
        public List<MDISCRTDTL> MDISCRTDTL { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public bool Deactive { get; set; }


    }
}