namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_STATE")]
    public partial class MS_STATE
    {
        [Key]
        [StringLength(2)]
        public string STATECD { get; set; }

        [Required]
        [StringLength(30)]
        public string STATENM { get; set; }

        [StringLength(2)]
        public string TDSSTATECD { get; set; }
        [StringLength(2)]
        public string VECHCD { get; set; }
    }
}
