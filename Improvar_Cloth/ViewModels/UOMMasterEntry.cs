using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class UOMMasterEntry : Permission
    {
        public string MSG { get; set; }
        public M_UOM M_UOM { get; set; }
        public MS_GSTUOM MS_GSTUOM { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
    }
}