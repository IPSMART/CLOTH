namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_BANKIFSC")]
    public partial class MS_BANKIFSC
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(15)]
        public string IFSCCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string BANKNAME { get; set; }

        [StringLength(10)]
        public string MICRCODE { get; set; }

        [StringLength(25)]
        public string BRANCH { get; set; }

        [StringLength(100)]
        public string ADDRESS { get; set; }

        [StringLength(20)]
        public string CONTACT { get; set; }

        [StringLength(35)]
        public string CITY { get; set; }

        [StringLength(35)]
        public string DISCTRICT { get; set; }

        [StringLength(35)]
        public string STATE { get; set; }
    }
}
