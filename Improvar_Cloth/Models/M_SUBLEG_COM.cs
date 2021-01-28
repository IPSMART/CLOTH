namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_COM")]
    public partial class M_SUBLEG_COM
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
        [Column(Order = 0)]
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(8)]
        public string AGSLCD { get; set; }

        public double? CRLIMIT { get; set; }

        public short? CRDAYS { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        [StringLength(6)]
        public string AREACD { get; set; }

        [StringLength(1)]
        public string COD { get; set; }

        [StringLength(1)]
        public string DOCTH { get; set; }

        [StringLength(4)]
        public string DISCRTCD { get; set; }
        public long M_AUTONO { get; set; }

        [StringLength(20)]
        public string SAPCODE { get; set; }
        
    }
}
