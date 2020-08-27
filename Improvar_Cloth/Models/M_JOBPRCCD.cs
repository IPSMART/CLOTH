namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_JOBPRCCD")]
    public partial class M_JOBPRCCD
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
        [StringLength(4)]
        public string JOBPRCCD { get; set; }

        [Required]
        [StringLength(15)]
        public string JOBPRCNM { get; set; }

        public long M_AUTONO { get; set; }
    }
}
