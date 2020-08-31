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
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string CLASS1NM{ get; set; }
    }
}