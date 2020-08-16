using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class GroupMasterEntry : Permission
    {
        public string MSG { get; set; }
        public List<GroupType> GroupType { get; set; }
        public M_GROUP M_GROUP { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public M_PRODGRP M_PRODGRP { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public bool Checked { get; set; }
    }
}