using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class CollectionEntry : Permission
    {
        public string MSG { get; set; }
        public M_COLLECTION M_COLLECTION { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public bool Checked { get; set; }
    }
}