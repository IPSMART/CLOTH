using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMBOMMTRL
    {
        public short? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BOMCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        public int SLNO { get; set; }

        [Required]
        [StringLength(12)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

         public double? QNTY { get; set; }

         public double? MTRLRT { get; set; }

        [StringLength(50)]
        public string REMARK { get; set; }

        [StringLength(1)]
        public string SIZE_LNK { get; set; }

        [Key]
        [Column(Order = 3)]
        public byte PSLNO { get; set; }

        [StringLength(50)]
        public string ITNM { get; set; }

        [StringLength(15)]
        public string PARTNM { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }

        [StringLength(20)]
        public string COLRNM { get; set; }
        public bool Checked { get; set; }

        [StringLength(3)]
        public string UOMCD { get; set; }

        [StringLength(5)]
        public string UOMNM { get; set; }
        public string ChildData { get; set; }
        public int ParentSerialNo { get; set; }

        [StringLength(8)]
        public string SLCD1 { get; set; }

        [StringLength(45)]
        public string SLNM1 { get; set; }

        [StringLength(8)]
        public string SLCD2 { get; set; }

        [StringLength(45)]
        public string SLNM2 { get; set; }

        [StringLength(8)]
        public string SLCD3 { get; set; }

        [StringLength(45)]
        public string SLNM3 { get; set; }

        [StringLength(12)]
        public string STYLENO { get; set; }
        public string ITGRP_TYPE { get; set; }
        public short? PCSPERBOX { get; set; }


    }
}