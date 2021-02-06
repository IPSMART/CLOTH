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

        public byte? BANKSLNO { get; set; }

        public long M_AUTONO { get; set; }

        public decimal? WPPER { get; set; }

        public decimal? RPPER { get; set; }
        [StringLength(100)]
        public string DESIGNPATH { get; set; }
        [StringLength(11)]
        public string PRICEINCODE { get; set; }
        [StringLength(8)]
        public string RTDEBCD { get; set; }
        [StringLength(1)]
        public string INC_RATE { get; set; }
        [StringLength(1)]
        public string MNTNSIZE { get; set; }
        [StringLength(1)]
        public string MNTNCOLOR { get; set; }
        [StringLength(1)]
        public string MNTNPART { get; set; }
        [StringLength(1)]
        public string MNTNFLAGMTR { get; set; }
        [StringLength(1)]
        public string MNTNLISTPRICE { get; set; }

        [StringLength(1)]
        public string MNTNDISC1 { get; set; }
        [StringLength(1)]
        public string MNTNDISC2 { get; set; }
        [StringLength(1)]
        public string MNTNSHADE { get; set; }
        [StringLength(1)]
        public string MNTNWPRPPER { get; set; }
        [StringLength(1)]
        public string MNTNOURDESIGN { get; set; }
        [StringLength(1)]
        public string MNTNBALE { get; set; }
        [StringLength(1)]
        public string MNTNPCSTYPE { get; set; }
        [StringLength(1)]
        public string MNTNBARNO { get; set; }
    }
}
