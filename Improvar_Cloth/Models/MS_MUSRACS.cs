namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_MUSRACS")]
    public partial class MS_MUSRACS
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string CLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(40)]
        public string USER_ID { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(40)]
        public string SCHEMA_NAME { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(15)]
        public string MODULE_CODE { get; set; }
    }
}
