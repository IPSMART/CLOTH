﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TBILTY
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(1)]
        public string DRCR { get; set; }

        [Required]
        [StringLength(30)]
        public string BLAUTONO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string LRDT { get; set; }

        [Required]
        [StringLength(15)]
        public string LRNO { get; set; }

        [Required]
        [StringLength(4)]
        public string BALEYR { get; set; }

        [Required]
        [StringLength(30)]
        public string BALENO { get; set; }
        public bool Checked { get; set; }
        public string PREFNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string PREFDT { get; set; }
        public string ChildData { get; set; }

    }
}