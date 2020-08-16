namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SIZE")]
    public partial class M_SIZE
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
        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }

        [StringLength(10)]
        public string ALTSIZENM { get; set; }

        [StringLength(3)]
        public string PRINT_SEQ { get; set; }

        [StringLength(3)]
        public string SZBARCODE { get; set; }

        public long M_AUTONO { get; set; }

    }
}
