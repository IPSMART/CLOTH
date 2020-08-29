namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNTRANS")]
    public partial class T_TXNTRANS
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(8)]
        public string TRANSLCD { get; set; }

        [StringLength(2)]
        public string TRANSMODE { get; set; }

        [StringLength(16)]
        public string LORRYNO { get; set; }

        public DateTime? LRDT { get; set; }

        [StringLength(15)]
        public string LRNO { get; set; }

        [StringLength(8)]
        public string CRSLCD { get; set; }

        [StringLength(25)]
        public string DESTN { get; set; }

        [StringLength(20)]
        public string EWAYBILLNO { get; set; }

        public decimal? GRWT { get; set; }

        public decimal? TRWT { get; set; }

        public decimal? NTWT { get; set; }

        [StringLength(35)]
        public string RECVPERSON { get; set; }

        [StringLength(1)]
        public string VECHLTYPE { get; set; }

        [StringLength(30)]
        public string GATEENTNO { get; set; }
    }
}
