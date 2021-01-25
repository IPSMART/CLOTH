using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class RateHistory
    {
        public List<RateHistoryGrid> RateHistoryGrid { get; set; }
    }
}
namespace Improvar.Models
{
    public class RateHistoryGrid
    {
        public string SLNO { get; set; }
        public string SLNM { get; set; }
        public string SLCD { get; set; }
        public string CITY { get; set; }
        public string AUTONO { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string QNTY { get; set; }
        public string RATE { get; set; }
        public double? SCMDISCRATE { get; set; }
    }
}