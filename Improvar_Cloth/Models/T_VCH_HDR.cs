namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_VCH_HDR")]
    public partial class T_VCH_HDR
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

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? VL_DT { get; set; }

        [StringLength(10)]
        public string PAY_BY { get; set; }

        [StringLength(100)]
        public string PAID_TO { get; set; }

        [StringLength(3)]
        public string CURRCD { get; set; }

        [StringLength(8)]
        public string BANK_CODE { get; set; }

        public double? CURRRT { get; set; }

        [StringLength(1)]
        public string AUTOGEN { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(8)]
        public string CLASS2CD { get; set; }

        [StringLength(1)]
        public string REVCHG { get; set; }

        [StringLength(1)]
        public string INPTCLAIM { get; set; }

        [StringLength(2)]
        public string TRCD { get; set; }

        [StringLength(30)]
        public string PASSEDBY { get; set; }
    }
}
