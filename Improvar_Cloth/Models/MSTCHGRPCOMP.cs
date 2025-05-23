﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class MSTCHGRPCOMP
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [StringLength(3)]
        public string STCHCD { get; set; }
        [Key]
        [StringLength(7)]
        public string FLDCD { get; set; }
        public byte? SLNO { get; set; }
        [StringLength(100)]
        public string FLDDESC { get; set; }
        [StringLength(20)]
        public string FLDNM { get; set; }
        [StringLength(1)]
        public string FLDTYPE { get; set; }
        public short? FLDLEN { get; set; }
        public byte? FLDDEC { get; set; }
        [StringLength(1)]
        public string FLDMANDT { get; set; }
        [StringLength(1)]
        public string FLDDATACOMBO { get; set; }
        public double? FLDMAX { get; set; }
        public double? FLDMIN { get; set; }
        [StringLength(15)]
        public string FLDFLAG1 { get; set; }
        [StringLength(1)]
        public string DEACTIVATE { get; set; }
        public bool Checked_Comp { get; set; }
        public bool Checked_FLDMANDT { get; set; }
        public bool Checked_FLDREMIND { get; set; }
        public bool Checked_FLDDATACOMBO { get; set; }
        public bool Checked_DEACTIVATE { get; set; }
        public string Checked_Disable { get; set; }
    }
}