using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Improvar.Models
{
    public class MSUBLEGIFSC
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

        public byte SLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(15)]
        public string IFSCCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string BANKNAME { get; set; }

        [Required]
        [StringLength(25)]
        public string BRANCH { get; set; }

        [Required]
        [StringLength(100)]
        public string ADDRESS { get; set; }

        [Required]
        [StringLength(20)]
        public string BANKACTNO { get; set; }

        [StringLength(10)]
        public string BANKACTTYPE { get; set; }

        [StringLength(1)]
        public string DEFLTBANK { get; set; }
        public bool Checked { get; set; }
        public bool DFLTBNK { get; set; }
    }
}