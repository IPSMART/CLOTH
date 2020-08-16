using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSUBLEGSDDTL
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
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [StringLength(4)]
        public string TAXGRPCD { get; set; }

        [StringLength(8)]
        public string TRSLCD { get; set; }

        [StringLength(8)]
        public string COURCD { get; set; }

        public double? CRLIMIT { get; set; }

        public short? CRDAYS { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(8)]
        public string AGSLCD { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }
        public bool Checked { get; set; }
        public short SLNO { get; set; }
        [StringLength(40)]
        public string SLNM { get; set; }
        [StringLength(2)]
        public string STATECD { get; set; }
        [StringLength(15)]
        public string GSTNO { get; set; }
        [StringLength(500)]
        public string ADD1 { get; set; }
        [StringLength(30)]
        public string TAXGRPNM { get; set; }
        [StringLength(4)]
        public string PARTYGRP { get; set; }
        [StringLength(40)]
        public string TRSLNM { get; set; }
        [StringLength(40)]
        public string PRCNM { get; set; }

        public string AGSLNM { get; set; }
        public string COURNM { get; set; }
        public string LOCNM { get; set; }

    }
}