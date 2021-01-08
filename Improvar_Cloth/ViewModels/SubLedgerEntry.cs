using System.Collections.Generic;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class SubLedgerEntry : Permission
    {
        public List<DOCTYPE> Document { get; set; }
        public List<PartyGroup> PartyGroup { get; set; }
        public List<CompanyType> CompanyType { get; set; }
        public List<LinkType> LinkType { get; set; }
        public List<RegistrationType> RegistrationType { get; set; }
        public List<GeneralLedgerDetails> GeneralLedgerDetails { get; set; }
        public List<CompanyLocationName> CompanyLocationName { get; set; }
        public List<MSUBLEGLOCOTH> MSUBLEGLOCOTH { get; set; }
        public List<BusinessActivity> BusinessActivity { get; set; }
        public List<LedgerType> LedgerType { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public MS_STATE MS_STATE { get; set; }
        public MS_COUNTRY MS_COUNTRY { get; set; }
        public M_DISTRICT M_DISTRICT { get; set; }        
        public List<MSUBLEGCONT> MSUBLEGCONT { get; set; }
        public List<MSUBLEGTAX> MSUBLEGTAX { get; set; }
        public List<MSUBLEGIFSC> MSUBLEGIFSC { get; set; }
        public bool Checked { get; set; }
        [StringLength(45)]
        public string PSLNM { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<Database_Combo3> Database_Combo3 { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public string isPresentinLastYrSchema { get; set; }
        public bool TCSAPPL { get; set; }
        public string PARTYNM { get; set; }
        public bool IsAPIEnabled { get; set; }


    }
}