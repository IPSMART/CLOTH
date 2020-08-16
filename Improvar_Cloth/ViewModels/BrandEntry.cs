using System.Collections.Generic;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class BrandEntry : Permission
    {
        public string MSG { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public M_CNTRL_HDR_DOC M_CNTRL_HDR_DOC { get; set; }
        public List<MCNTRLHDRDOC> MCNTRLHDRDOC { get; set; }
        public bool Checked { get; set; }



    }
}