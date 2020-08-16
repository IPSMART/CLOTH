using System.Collections.Generic;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class ItemSizeEntry : Permission
    {
        public string MSG { get; set; }
        public M_SIZE M_SIZE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        //public List<IndexKey01> IndexKey01 { get; set; }
        public bool Checked { get; set; }
    }
}
