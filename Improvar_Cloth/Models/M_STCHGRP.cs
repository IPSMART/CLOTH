using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    [Table("M_STCHGRP")]
    public class M_STCHGRP
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
        [StringLength(3)]
        public string STCHCD { get; set; }

        [StringLength(1)]
        public string STCHALT { get; set; }
        [StringLength(30)]
        public string STCHNM { get; set; }
        [StringLength(5)]
        public string STCHUOM { get; set; }
        [StringLength(2)]
        public string JOBCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? STCHRATE { get; set; }
        [StringLength(1)]
        public string INCLTAX { get; set; }
        [StringLength(500)]
        public string STCHREM { get; set; }
        public long M_AUTONO { get; set; }
    }
}