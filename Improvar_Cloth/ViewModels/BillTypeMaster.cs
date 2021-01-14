using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class BillTypeMaster :Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_BLTYPE M_BLTYPE { get; set; }
        public bool Deactive { get; set; }
    }
}