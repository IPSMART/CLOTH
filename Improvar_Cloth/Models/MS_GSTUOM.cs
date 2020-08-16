namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_GSTUOM")]
    public partial class MS_GSTUOM
    {
        [Key]
        [StringLength(3)]
        public string GUOMCD { get; set; }

        [Required]
        [StringLength(20)]
        public string GUOMNM { get; set; }
    }
}
