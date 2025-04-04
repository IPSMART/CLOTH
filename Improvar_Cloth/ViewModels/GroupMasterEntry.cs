﻿using Improvar.Models;
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
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public string CLASS1NM{ get; set; }
        public string SALGLNM { get; set; }
        public string PURGLNM { get; set; }
        public string SALRETGLNM { get; set; }
        public string PURRETGLNM { get; set; }
        public bool NEGSTOCK { get; set; }
        public string WPPRICEGENCD { get; set; }
        public string WPPRICEGENAMT { get; set; }
        public string RPPRICEGENCD { get; set; }
        public string RPPRICEGENAMT { get; set; }


    }
}