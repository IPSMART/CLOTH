using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class SysCnfgMasterEntry : Permission
    {
        public M_SYSCNFG M_SYSCNFG { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public bool Checked { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }
        public string CLASS1NM { get; set; }
        public string RETDEBSLNM { get; set; }
        public string SALDEBGLNM { get; set; }
        public string PURDEBGLNM { get; set; }
        public string SEARCH_AUTONO { get; set; }
        public bool INC_RATE { get; set; }
        public string RTDBNM { get; set; }
        public string WPPRICEGENCD { get; set; }
        public string WPPRICEGENAMT { get; set; }
        public string RPPRICEGENCD { get; set; }
        public string RPPRICEGENAMT { get; set; }
        public bool MNTNSIZE { get; set; }
        public bool MNTNCOLOR { get; set; }
        public bool MNTNPART { get; set; }
        public bool MNTNFLAGMTR { get; set; }
        public bool MNTNLISTPRCE { get; set; }
        

    }
}