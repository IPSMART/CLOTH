using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;


namespace Improvar.ViewModels
{
    public class JobMasterEntry : Permission
    {
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_JOBMST M_JOBMST { get; set; }
        public M_GENLEG M_GENLEG { get; set; }
        public M_GENLEG M_GENLEG1 { get; set; }
        public M_PRODGRP M_PRODGRP { get; set; }
        public M_SITEM M_SITEM { get; set; }
        public M_UOM M_UOM { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public bool Deactive { get; set; }
        public M_MTRLJOBMST M_MTRLJOBMST { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }

    }
}