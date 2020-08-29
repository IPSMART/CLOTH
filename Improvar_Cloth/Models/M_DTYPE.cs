namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_DTYPE")]
    public partial class M_DTYPE
    {
        [Key]
        [StringLength(10)]
        public string DCD { get; set; }

        [Required]
        [StringLength(40)]
        public string DNAME { get; set; }

        [Required]
        [StringLength(1)]
        public string FIN { get; set; }

        [StringLength(20)]
        public string MENU_PROGCALL { get; set; }

        [StringLength(10)]
        public string MENU_PARA { get; set; }

        [StringLength(5)]
        public string FLAG1 { get; set; }
    }
}
