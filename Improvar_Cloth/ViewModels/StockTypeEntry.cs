using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class StockTypeEntry : Permission
    {
        public string MSG { get; set; }
        public List<GroupType> GroupType { get; set; }
        public M_STKTYPE M_STKTYPE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<IndexKey01> IndexKey01 { get; set; }
        public bool Checked { get; set; }
        public bool Deactive { get; set; }
       
    }}