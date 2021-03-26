namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_MGROUP_SPL")]
    public partial class M_MGROUP_SPL
    {
        [Key]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(200)]
        public string DEALSIN { get; set; }

        [StringLength(200)]
        public string INSPOLDESC { get; set; }

        [StringLength(2000)]
        public string BLTERMS { get; set; }

        [StringLength(1)]
        public string DUEDATECALCON { get; set; }

        public int? BANKSLNO { get; set; }
    }
}
