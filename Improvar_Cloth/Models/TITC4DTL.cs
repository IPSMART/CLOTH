using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TITC4DTL
    {
        public short SLNO { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        [StringLength(20)]
        public string JOBNM { get; set; }
        public string AUTONO { get; set; }
        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }
        public string DOCNM { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public decimal QNTY { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public decimal SHORTQNTY { get; set; }
        public string UOM { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public decimal RATE { get; set; }
        public bool Checked { get; set; }
        public double? JOBSEQ { get; set; }
        public string FLAG1 { get; set; }
    }
}