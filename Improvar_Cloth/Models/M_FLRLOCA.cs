namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_FLRLOCA")]
    public partial class M_FLRLOCA
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
        [StringLength(3)]
        public string FLRCD { get; set; }

        [Required]
        [StringLength(15)]
        public string FLRNM { get; set; }

        [Required]
        [StringLength(4)]
        public string JOBPRCCD { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }

        public long M_AUTONO { get; set; }

       
    }
}
