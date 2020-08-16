namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DOC_BRAND")]
    public partial class M_DOC_BRAND
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string BRANDCD { get; set; }

        [StringLength(6)]
        public string GOCD { get; set; }
        
    }
}
