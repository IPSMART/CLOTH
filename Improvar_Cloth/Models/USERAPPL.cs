//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Web;

//namespace Improvar.Models
//{
//    public class USERAPPL
//    {
//        public int SLNO { get; set; }
//        public bool Checked { get; set; }

//        [Key]
//        [StringLength(40)]
//        public string USER_ID { get; set; }

//        [Required]
//        [StringLength(12)]
//        public string MOBILE { get; set; }

//        [StringLength(100)]
//        public string EMAIL { get; set; }

//        [StringLength(8)]
//        public string EMPCD { get; set; }

//        public string EMPNM { get; set; }
//        public string ESHNM { get; set; }
//        public string EGNM { get; set; }       
//        public long M_AUTONO { get; set; }

//        [StringLength(200)]
//        public string LOGAPIID { get; set; }

//        [StringLength(500)]
//        public string LOGPUSHID { get; set; }

//        [StringLength(100)]
//        public string IMEINO { get; set; }

//        public bool FINACCESS { get; set; }

//        public DateTime? DEACT_FINACCESS { get; set; }


//        public string LAST_LOGGED { get; set; }

//        public string TIMES_LOOGED { get; set; }

//        public int? TIMES_PSSW_CHANGED { get; set; }

//        public DateTime? USER_CREATED_ON { get; set; }


//        public bool ACTIVE_TAG { get; set; }

//        [StringLength(100)]
//        public string REMARKS { get; set; }

//        public DateTime? LOCK_DATE { get; set; }

//        public DateTime? UNLOCK_DATE { get; set; }

//        [Required]
//        [StringLength(40)]
//        public string U_ID { get; set; }

//        public DateTime? U_ENTDATE { get; set; }

//        [StringLength(40)]
//        public string U_ID_NEW { get; set; }

//        public DateTime? U_ENTDATE_NEW { get; set; }

//        [Required]
//        [StringLength(100)]
//        public string OS_ID { get; set; }

//        [Required]
//        [StringLength(100)]
//        public string OS_SYS_ID { get; set; }

//        [StringLength(100)]
//        public string OS_ID_NEW { get; set; }

//        [StringLength(100)]
//        public string OS_SYS_ID_NEW { get; set; }

//        [StringLength(20)]
//        public string USER_IP { get; set; }

//        [StringLength(20)]
//        public string USER_IP_NEW { get; set; }

//        [StringLength(20)]
//        public string USER_STATIC_IP { get; set; }

//        [StringLength(20)]
//        public string USER_STATIC_IP_NEW { get; set; }


//        public bool INVACCESS { get; set; }

//        public DateTime? DEACT_INVACCESS { get; set; }


//        public bool PAYACCESS { get; set; }

//        public DateTime? DEACT_PAYACCESS { get; set; }


//        public bool SALEACCESS { get; set; }

//        public DateTime? DEACT_SALEACCESS { get; set; }


//        public bool OUTACCESS { get; set; }

//        public DateTime? DEACT_OUT { get; set; }


//        public bool MOBAPP1 { get; set; }

//        public DateTime? DEACT_MOBAPP1 { get; set; }

//        public bool MOBAPP2 { get; set; }

//        public DateTime? DEACT_MOBAPP2 { get; set; }

//        public bool MOBAPP3 { get; set; }

//        public DateTime? DEACT_MOBAPP3 { get; set; }

//        public bool CHECKED { get; set; }
//    }
//}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class USERAPPL
    {
        public int SLNO { get; set; }
        public bool Checked { get; set; }

        [Key]
        [StringLength(40)]
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }

        [Required]
        [StringLength(12)]
        public string MOBILE { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        [StringLength(8)]
        public string EMPCD { get; set; }

        public string EMPNM { get; set; }
        public string ESHNM { get; set; }
        public string EGNM { get; set; }
        public long M_AUTONO { get; set; }

    }
}