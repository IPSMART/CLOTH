using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MMGROUPSPL
    {
        
        [StringLength(4)]
        public string ITGRPCD { get; set; }
        
        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(200)]
        public string DEALSIN { get; set; }

        [StringLength(200)]
        public string INSPOLDESC { get; set; }

        [StringLength(2000)]
        public string BLTERMS { get; set; }

        [StringLength(1)]
        public string DUEDATECALCON { get; set; }
        public bool DUEDATECALCONChecked { get; set; }
        public int? BANKSLNO { get; set; }
        public int SLNO { get; set; }
        public string COMPNM { get; set; }
    }
}