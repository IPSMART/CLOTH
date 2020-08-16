using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Improvar.ViewModels
{
    public class SubBrandEntry : Permission
    {
        public string MSG { get; set; }
        public M_SUBBRAND M_SUBBRAND { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public List<IndexKey1> IndexKey1 { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
    }
}