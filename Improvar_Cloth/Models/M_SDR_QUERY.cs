namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SDR_QUERY")]
    public partial class M_SDR_QUERY
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [StringLength(10)]
        public string QUERYID { get; set; }

        [Required]
        [StringLength(1)]
        public string MODNM { get; set; }

        [Required]
        [StringLength(40)]
        public string QUERYNM { get; set; }

        [Required]
        [StringLength(200)]
        public string REPHEADING { get; set; }

        [Required]
        [StringLength(200)]
        public string REPDESIGN { get; set; }

        [Required]
        [StringLength(20)]
        public string FILENAME { get; set; }

        [Required]
        [StringLength(4000)]
        public string QUERYVAL { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
