namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_NATBUSCODES")]
    public partial class MS_NATBUSCODES
    {
        [Key]
        [StringLength(2)]
        public string NATBUSCD { get; set; }

        [Required]
        [StringLength(50)]
        public string NATBUSNM { get; set; }
    }
}
