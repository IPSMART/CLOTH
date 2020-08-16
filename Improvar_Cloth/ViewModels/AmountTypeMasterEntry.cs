using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class AmountTypeMasterEntry : Permission
    {
        public M_AMTTYPE M_AMTTYPE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_GENLEG M_GENLEG { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public bool Checked { get; set; }
        public string CALCTYPE { get; set; }
        public string TAXCODE { get; set; }
        public string SALPUR { get; set; }
        public string ADDLESS { get; set; }
    }
}