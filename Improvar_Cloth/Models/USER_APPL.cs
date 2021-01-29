namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USER_APPL")]
    public partial class USER_APPL
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(40)]
        public string USER_ID { get; set; }

        [Required]
        [StringLength(500)]
        public string PSWRD { get; set; }

        [Required]
        [StringLength(50)]
        public string USER_NAME { get; set; }

        [Required]
        [StringLength(1)]
        public string USER_TYPE { get; set; }

        public DateTime? LAST_LOGGED { get; set; }

        public int? TIMES_LOOGED { get; set; }

        public int? TIMES_PSSW_CHANGED { get; set; }

        public DateTime? USER_CREATED_ON { get; set; }

        [StringLength(1)]
        public string ACTIVE_TAG { get; set; }

        [StringLength(100)]
        public string REMARKS { get; set; }

        public DateTime? LOCK_DATE { get; set; }

        public DateTime? UNLOCK_DATE { get; set; }

        [Required]
        [StringLength(40)]
        public string U_ID { get; set; }

        public DateTime? U_ENTDATE { get; set; }

        [StringLength(40)]
        public string U_ID_NEW { get; set; }

        public DateTime? U_ENTDATE_NEW { get; set; }

        [Required]
        [StringLength(100)]
        public string OS_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string OS_SYS_ID { get; set; }

        [StringLength(100)]
        public string OS_ID_NEW { get; set; }

        [StringLength(100)]
        public string OS_SYS_ID_NEW { get; set; }

        [StringLength(20)]
        public string USER_IP { get; set; }

        [StringLength(20)]
        public string USER_IP_NEW { get; set; }

        [StringLength(20)]
        public string USER_STATIC_IP { get; set; }

        [StringLength(20)]
        public string USER_STATIC_IP_NEW { get; set; }

        [Required]
        [StringLength(12)]
        public string MOBILE { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        [StringLength(200)]
        public string LOGAPIID { get; set; }

        [StringLength(500)]
        public string LOGPUSHID { get; set; }

        [StringLength(100)]
        public string IMEINO { get; set; }

        [StringLength(8)]
        public string EMPCD { get; set; }

        [StringLength(1)]
        public string FINACCESS { get; set; }

        public DateTime? DEACT_FINACCESS { get; set; }

        [StringLength(1)]
        public string INVACCESS { get; set; }

        public DateTime? DEACT_INVACCESS { get; set; }

        [StringLength(1)]
        public string PAYACCESS { get; set; }

        public DateTime? DEACT_PAYACCESS { get; set; }

        [StringLength(1)]
        public string SALEACCESS { get; set; }

        public DateTime? DEACT_SALEACCESS { get; set; }

        [StringLength(1)]
        public string OUTACCESS { get; set; }

        public DateTime? DEACT_OUTACCESS { get; set; }

        [StringLength(1)]
        public string MOBAPP1 { get; set; }

        public DateTime? DEACT_MOBAPP1 { get; set; }

        [StringLength(1)]
        public string MOBAPP2 { get; set; }

        public DateTime? DEACT_MOBAPP2 { get; set; }

        [StringLength(1)]
        public string MOBAPP3 { get; set; }

        public DateTime? DEACT_MOBAPP3 { get; set; }
        public DateTime? LASTPWDCHANGED { get; set; }
        public DateTime? LASTFORGOTUSED { get; set; }
        public int? TIMESFORGOTUSED { get; set; }
    }
}
