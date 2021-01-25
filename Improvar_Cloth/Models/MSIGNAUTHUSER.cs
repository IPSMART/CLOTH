using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSIGNAUTHUSER
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
        [StringLength(5)]
        public string AUTHCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string USRID { get; set; }
        public string USER_NAME { get; set; }

        [StringLength(20)]
        public string SIGN_IMG { get; set; }

        [StringLength(10)]
        public string EMPID { get; set; }

        [StringLength(5)]
        public string DESIGCD { get; set; }
        public string DESIGNM { get; set; }
        [StringLength(75)]
        public string ENM { get; set; }
        public bool CHECKED { get; set; }
        public int SLNO { get; set; }
    }
}