using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MITEMPLISTADD
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
        [StringLength(4)]
        public string PRCCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public double RATE { get; set; }
        public double? OLDRATE { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(8)]
        public string SIZECOLCD { get; set; }

        public long M_AUTONO { get; set; }

        public long? APPRV_MAUTONO { get; set; }

        [StringLength(200)]
        public string REMARKS { get; set; }
        public int SLNO { get; set; }
        public bool Checked { get; set; }
        public string ChildData { get; set; }

        [StringLength(12)]
        public string ARTNO { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }
        public string SIZE_RATE_DTL { get; set; }

    }
}