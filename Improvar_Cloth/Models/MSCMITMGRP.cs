using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Improvar.Models
{
    public class MSCMITMGRP
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
        [StringLength(6)]
        public string SCMITMGRPCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [StringLength(30)]
        public string SCMITMGRPNM { get; set; }

        [StringLength(4)]
        public string BRANDCD { get; set; }

        [StringLength(4)]
        public string SBRANDCD { get; set; }

        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(4)]
        public string COLLCD { get; set; }

        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }
        public bool Checked { get; set; }

        [StringLength(15)]
        public string BRANDNM { get; set; }
        [StringLength(15)]
        public string SBRANDNM { get; set; }
        [StringLength(30)]
        public string ITGRPNM { get; set; }
        [StringLength(30)]
        public string COLLNM { get; set; }
        [StringLength(60)]
        public string ITNM { get; set; }
        [StringLength(10)]
        public string SIZENM { get; set; }
        [StringLength(20)]
        public string COLRNM { get; set; }

        [StringLength(12)]
        public string STYLENO { get; set; }

        [StringLength(1)]
        public string SKIPCOND { get; set; }
        [StringLength(20)]
        public string PDESIGN { get; set; }

    }
}