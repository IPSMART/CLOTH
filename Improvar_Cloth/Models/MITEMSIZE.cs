using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Improvar.Models
{
    public class MSITEMSIZE
    {
        public short? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(2)]
        public string SRLNO { get; set; }

        

        [StringLength(15)]
        public string BARCODE { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(12)]
        public string ITCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }

        public bool Checked { get; set; }
        public bool IChecked { get; set; }
    }
}