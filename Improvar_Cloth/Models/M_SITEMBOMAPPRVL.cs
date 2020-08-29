namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEMBOMAPPRVL")]
    public partial class M_SITEMBOMAPPRVL
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BOMCD { get; set; }

        public DateTime? EFFDT { get; set; }

        public DateTime? APPRVDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(50)]
        public string REMARK { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
