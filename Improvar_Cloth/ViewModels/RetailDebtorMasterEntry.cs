using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class RetailDebtorMasterEntry : Permission
    {
        public string DOBYEAR { get; set; }
        public List<GroupType> GroupType { get; set; }
        public M_RETDEB M_RETDEB { get; set; }
        public MS_STATE MS_STATE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public bool Checked { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string REFRTDEBNM { get; set; }
        public string REFSLNM { get; set; }
        public string GSTNO { get; set; }
        public string isPresentinLastYrSchema { get; set; }
    }
}