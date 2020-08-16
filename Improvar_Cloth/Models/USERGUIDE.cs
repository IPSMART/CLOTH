using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class USERGUIDE
    {
        [Key]
        [StringLength(6)]
        public string QUESTIONID { get; set; }
        [Required]
        [StringLength(40)]
        public string QUESTIONBY { get; set; }
        [Required]
        [StringLength(2000)]
        public string QUESTION { get; set; }

        [StringLength(40)]
        public string ANSWERBY { get; set; }

        [StringLength(2000)]
        public string ANSWER { get; set; }

        [StringLength(100)]
        public string STATUS { get; set; }
        [Required]
        [StringLength(10)]
        public string MODULE_CODE { get; set; }
        public short SLNO { get; set; }
        public bool Checked { get; set; }
    }
}