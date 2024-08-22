using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class SchmItemGroupMaster : Permission
    {
        public M_SCMITMGRP_HDR M_SCMITMGRP_HDR { get; set; }
        public M_SCMITMGRP M_SCMITMGRP { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public M_SUBBRAND M_SUBBRAND { get; set; }
        public M_GROUP M_GROUP { get; set; }
        public M_COLLECTION M_COLLECTION { get; set; }
        public M_SITEM M_SITEM { get; set; }
        public M_SIZE M_SIZE { get; set; }
        public M_COLOR M_COLOR { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_CNTRL_HDR_DOC M_CNTRL_HDR_DOC { get; set; }
        public List<MSCMITMGRP> MSCMITMGRP { get; set; }
        public List<MSCMITEMGRPDATA> MSCMITEMGRPDATA { get; set; }
        public bool Deactive { get; set; }
        public string PDESIGN { get; set; }
        // public List<CompanyLocationName> CompanyLocationName { get; set; }

    }
}