using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MUSRACSDOCCD
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(40)]
        public string USER_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(40)]
        public string SCHEMA_NAME { get; set; }

        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(4)]
        public string LOCCD { get; set; }

        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [StringLength(15)]
        public string USR_LIP { get; set; }

        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }

        [StringLength(40)]
        public string DNAME { get; set; }

        [StringLength(35)]
        public string DOCNM { get; set; }
        public bool Checked { get; set; }


    }
}