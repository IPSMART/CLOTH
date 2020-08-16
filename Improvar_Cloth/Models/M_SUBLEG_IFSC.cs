namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_IFSC")]
    public partial class M_SUBLEG_IFSC
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

        [StringLength(100)]
        public string ADDRESS { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [StringLength(20)]
        public string BANKACTNO { get; set; }

        [StringLength(10)]
        public string BANKACTTYPE { get; set; }

        [StringLength(1)]
        public string DEFLTBANK { get; set; }
    }
}
