namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_DOCCTG")]
    public partial class MS_DOCCTG
    {

        [Key]
        [Required]
        [StringLength(15)]
        public string DOC_CTG { get; set; }

    }
}
