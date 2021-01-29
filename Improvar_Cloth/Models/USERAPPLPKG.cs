using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class USERAPPLPKG
    {
        public int SLNO { get; set; }    
        public string USER_ID { get; set; }       
        public string EMPCD { get; set; }        
        public string EMPNM { get; set; }        
        public bool FINACCESS { get; set; }
        public string LAST_LOGGED { get; set; }
        public string TIMES_LOOGED { get; set; }
        public bool ACTIVE_TAG { get; set; }
        public bool INVACCESS { get; set; }
        public bool PAYACCESS { get; set; }
        public bool SALEACCESS { get; set; }
        public bool OUTACCESS { get; set; }
        public bool MOBAPP1 { get; set; }
        public bool MOBAPP2 { get; set; }      
        public bool MOBAPP3 { get; set; }
       public bool CHECKED { get; set; }
    }
}