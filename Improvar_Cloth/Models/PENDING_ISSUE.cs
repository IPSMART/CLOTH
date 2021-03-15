using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class PENDING_ISSUE
    {
        public bool Checked { get; set; }
        public short SLNO { get; set; }
        public string ISSUEAUTONO { get; set; }
        public string ISSUEDOCDT { get; set; }
        public string ISSUEDOCNO { get; set; }
    }
}