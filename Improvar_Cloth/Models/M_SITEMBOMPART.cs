namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEMBOMPART")]
    public partial class M_SITEMBOMPART
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

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte SLNO { get; set; }

        public decimal? MTRLCOST { get; set; }

        [Required]
        [StringLength(2)]
        public string JOBCD { get; set; }

        public decimal? JOBRT { get; set; }

        [StringLength(50)]
        public string REMARK { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }
        
    }
}
