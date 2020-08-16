namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_GENTRY")]
    public partial class T_GENTRY
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(6)]
        public string DOCNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(20)]
        public string CHLNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CHLDT { get; set; }

        [StringLength(1)]
        public string TRNSMOD { get; set; }

        [StringLength(8)]
        public string TSLCD { get; set; }

        [StringLength(15)]
        public string VEHLNO { get; set; }

        [StringLength(30)]
        public string PERSNM { get; set; }

        public double? QNTY { get; set; }

        [StringLength(10)]
        public string UOM { get; set; }

        [StringLength(50)]
        public string NOTE1 { get; set; }

        [StringLength(50)]
        public string NOTE2 { get; set; }        
        public DateTime? OUT_TIME { get; set; }
    }
}
