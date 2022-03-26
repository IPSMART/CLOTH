using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class SubLedgerSDdtlMasterEntry : Permission
    {
        public M_SUBLEG_COM M_SUBLEG_COM { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }
        public M_PRCLST M_PRCLST { get; set; }
        public M_AREACD M_AREACD { get; set; }
        public M_DISCRT M_DISCRT { get; set; }
        public List<MSUBLEGSDDTL> MSUBLEGSDDTL { get; set; }
        public List<MSUBLEGBRAND> MSUBLEGBRAND { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public string AGSLNM { get; set; }
        public bool Deactive { get; set; }
        public List<DocumentThrough> DocumentThrough { get; set; }
        public List<DropDown_list_DelvType> DropDown_list_DelvType { get; set; }
        public string GSTNO { get; set; }
        public string DISTRICT { get; set; }
        public string isPresentinLastYrSchema { get; set; }
    }
}