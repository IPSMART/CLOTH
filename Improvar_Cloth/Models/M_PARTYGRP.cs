namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_PARTYGRP")]
    public partial class M_PARTYGRP
    {
        [Key]
        [StringLength(4)]
        public string PARTYCD { get; set; }

        [Required]
        [StringLength(35)]
        public string PARTYNM { get; set; }
    }
}
