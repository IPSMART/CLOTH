using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSCHEMESLCD
    {
        public short SLNO { get; set; }
        public bool Checked { get; set; }  
        public string SLCD { get; set; }
        public string SLNM { get; set; }
    }
    public class MSCHEMEAREA
    {
        public short SLNO { get; set; }
        public bool Checked { get; set; }
        public string AREACD { get; set; }
        public string AREANM { get; set; }
    }
    public class MSCHEMEAGENT
    {
        public short SLNO { get; set; }
        public bool Checked { get; set; }
        public string AGSLCD { get; set; }
        public string AGSLNM { get; set; }
    }
}