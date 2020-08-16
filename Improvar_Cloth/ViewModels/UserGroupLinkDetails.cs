using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class UserGroupLinkDetails : Permission
    {
        public M_USR_ACS_GRPDTL M_USR_ACS_GRPDTL { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public USER_APPL USER_APPL { get; set; }
        public List<MUSRACSGRPDTL> MUSRACSGRPDTL { get; set; }
    }
}