namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_PROGBOM")]
    public partial class T_PROGBOM
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RSLNO { get; set; }

        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public decimal? BOMQNTY { get; set; }

        public decimal? EXTRAQNTY { get; set; }

        public decimal? QNTY { get; set; }

        public decimal? PENDQNTY { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(100)]
        public string BREM { get; set; }

        [StringLength(1)]
        public string SAMPLE { get; set; }

        
    }
}
