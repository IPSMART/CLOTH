namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_COMPTYPE")]
    public partial class MS_COMPTYPE
    {
        [Key]
        [StringLength(2)]
        public string COMPTYCD { get; set; }

        [Required]
        [StringLength(25)]
        public string COMPTYNM { get; set; }

        [StringLength(1)]
        public string LTDIND { get; set; }
        [StringLength(1)]
        public string DEDTYPE { get; set; }
    }
}
