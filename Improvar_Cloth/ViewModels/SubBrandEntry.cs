using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Improvar.ViewModels
{
    public class SubBrandEntry : Permission
    {
        public M_SUBBRAND M_SUBBRAND { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
    }
}