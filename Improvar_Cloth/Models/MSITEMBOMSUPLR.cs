using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMBOMSUPLR
    {
        public int? EMD_NO { get; set; }

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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte SLNO { get; set; }

        [Key]
        [Column(Order = 3)]
        public byte RSLNO { get; set; }

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

        [Key]
        [Column(Order = 4)]
        public byte PSLNO { get; set; }
    }
}