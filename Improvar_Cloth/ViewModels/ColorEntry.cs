using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class ColorEntry: Permission
    {
        public string MSG { get; set; }
        public M_COLOR M_COLOR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_CNTRL_HDR_DOC M_CNTRL_HDR_DOC { get; set; }
        public List<MCNTRLHDRDOC> MCNTRLHDRDOC { get; set; }
        public bool Checked { get; set; }

    }
}