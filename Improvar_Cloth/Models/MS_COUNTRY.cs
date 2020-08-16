namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_COUNTRY")]
    public partial class MS_COUNTRY
    {
        [Key]
        [StringLength(2)]
        public string CNCD { get; set; }

        [StringLength(60)]
        public string CNAME { get; set; }

        public short? CISDCD { get; set; }

        [StringLength(3)]
        public string CN3CD { get; set; }

        public short? CNNUMCD { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_LIP { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_SIP { get; set; }

    }
}
