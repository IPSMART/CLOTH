namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SYSCNFG")]
    public partial class M_SYSCNFG
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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [StringLength(8)]
        public string RETDEBSLCD { get; set; }

        [StringLength(5)]
        public string WPPRICEGEN { get; set; }

        [StringLength(5)]
        public string RPPRICEGEN { get; set; }

        [Required]
        [StringLength(8)]
        public string SALDEBGLCD { get; set; }

        [Required]
        [StringLength(8)]
        public string PURDEBGLCD { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(4)]
        public string COMPCD { get; set; }

        [StringLength(100)]
        public string INSPOLDESC { get; set; }

        [StringLength(100)]
        public string DEALSIN { get; set; }

        [StringLength(1000)]
        public string BLTERMS { get; set; }

        [StringLength(1)]
        public string DUEDATECALCON { get; set; }

        public byte? BANLSLNO { get; set; }

        public long M_AUTONO { get; set; }

        public decimal? WPPER { get; set; }

        public decimal? RPPER { get; set; }
        [StringLength(11)]
        public string PRICEINCODE { get; set; }
        [StringLength(8)]
        public string RTDEBCD { get; set; }
        [StringLength(1)]
        public string INC_RATE { get; set; }
    }
}
