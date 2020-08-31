namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_SORD")]
    public partial class T_SORD
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

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(8)]
        public string CONSLCD { get; set; }

        [StringLength(8)]
        public string AGSLCD { get; set; }

        [StringLength(8)]
        public string SAGSLCD { get; set; }

        [StringLength(8)]
        public string TRSLCD { get; set; }

        [StringLength(8)]
        public string SLMSLCD { get; set; }

        [StringLength(30)]
        public string PREFNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PREFDT { get; set; }

        [StringLength(1)]
        public string RECMODE { get; set; }

        [StringLength(50)]
        public string ORDBY { get; set; }

        [StringLength(50)]
        public string SELBY { get; set; }

        [StringLength(1)]
        public string DOCTH { get; set; }

        [StringLength(200)]
        public string DELVINS { get; set; }

        public byte? PAYTRMS { get; set; }

        public byte? DUEDAYS { get; set; }

        [StringLength(1)]
        public string COD { get; set; }

         public double? APROXVAL { get; set; }

        [StringLength(300)]
        public string SPLNOTE { get; set; }

         public double? RATEPER { get; set; }

    }
}
