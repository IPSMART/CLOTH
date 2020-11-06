namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_PRCLST")]
    public partial class M_PRCLST
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
        [StringLength(4)]
        public string PRCCD { get; set; }

        [StringLength(30)]
        public string PRCNM { get; set; }

        [StringLength(5)]
        public string PRCGRP { get; set; }

        public long? M_AUTONO { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }
        public byte? SEQNO { get; set; }
        
    }
}
