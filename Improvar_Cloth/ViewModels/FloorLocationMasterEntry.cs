using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class FloorLocationMasterEntry:Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_FLRLOCA M_FLRLOCA { get; set; }
        public M_JOBPRCCD M_JOBPRCCD { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }

        public bool Deactive { get; set; }
    }
}