using System.Collections.Generic;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class DocumentAuthorizationEntry : Permission
    {
        public List<DocumentType> DocumentType { get; set; }
       // public List<MDOCAUTH> MDOCAUTH { get; set; }
        public List<MDOCAUTHLAVEL1> MDOCAUTHLEVEL1 { get; set; }
        public List<MDOCAUTHLAVEL2> MDOCAUTHLEVEL2 { get; set; }
        public List<MDOCAUTHLAVEL3> MDOCAUTHLEVEL3 { get; set; }
        public List<MDOCAUTHLAVEL4> MDOCAUTHLEVEL4 { get; set; }
        public List<MDOCAUTHLAVEL5> MDOCAUTHLEVEL5 { get; set; }
        public M_DOC_AUTH M_DOC_AUTH { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_SIGN_AUTH M_SIGN_AUTH { get; set; }
        public List<MSIGNAUTH> MSIGNAUTH { get; set; }
        public List<MDEPTHOD> MDEPTHOD { get; set; }
        public List<CompanyLocationName> CompanyLocationName { get; set; }
        public List<SignAuthority> SignAuthority { get; set; }
        public bool Checked { get; set; }
    }
}
