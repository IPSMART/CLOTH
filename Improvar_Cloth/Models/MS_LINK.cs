namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_LINK")]
    public partial class MS_LINK
    {

        [Key]
        [StringLength(1)]
        public string LINKCD { get; set; }

        [Required]
        [StringLength(20)]
        public string LINKNM { get; set; }

        
    }
}
