using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class GodownMasterEntry:Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_GODOWN M_GODOWN { get; set; }
        public List<CompanyLocationName> CompanyLocationName { get; set; }
        public bool Deactive { get; set; }
       public string LOCNM { get; set; }
        //public List<Attendance_type> Attendance_type { get; set; }
        //public List<Attendance_day> Attendance_day { get; set; }

    }
}