namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DESIGNATION")]
    public partial class M_DESIGNATION
    {
        [Key]
        [StringLength(5)]
        public string DESIGCD { get; set; }

        [Required]
        [StringLength(35)]
        public string DESIGNM { get; set; }
    }
}
