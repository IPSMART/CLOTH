namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("V_SJOBMST_STDRT")]
    public partial class V_SJOBMST_STDRT
    {
        [StringLength(19)]
        public string ITPARTJOBCD { get; set; }

        [Key]
        [Column(Order = 0)]
        public DateTime BOMEFFDT { get; set; }

        public DateTime? PRICEEFFDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }

        [StringLength(12)]
        public string STYLENO { get; set; }

        public short? PCSPERBOX { get; set; }

        [StringLength(1)]
        public string MIXSIZE { get; set; }

        public short? PCSPERSET { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(15)]
        public string PARTNM { get; set; }

        public byte? SLNO { get; set; }

        [StringLength(5)]
        public string SJOBCD { get; set; }

        [StringLength(35)]
        public string SJOBNM { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        public double? JOBRT { get; set; }

        [StringLength(4)]
        public string JOBPRCCD { get; set; }

        public string SEQORDNO { get; set; }

        public double? FIXJOBRT { get; set; }
    }
}
