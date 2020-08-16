using System;
using System.Collections.Generic;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class DocumentsUpload : Permission
    {
        public string MSG { get; set; }
        public List<DOCTYPE> Document { get; set; }
        public M_CNTRL_HDR_DOC M_CNTRL_HDR_DOC { get; set; }
        public List<MCNTRLHDRDOC> MCNTRLHDRDOC { get; set; }
    }
}