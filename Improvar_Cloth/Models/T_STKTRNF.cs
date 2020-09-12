namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_STKTRNF")]
    public partial class T_STKTRNF
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
        [StringLength(4)]
        public string SLOCCD { get; set; }

        [Required]
        [StringLength(4)]
        public string TLOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string SGOCD { get; set; }

        [Required]
        [StringLength(6)]
        public string TGOCD { get; set; }

        [Required]
        [StringLength(30)]
        public string SAUTONO { get; set; }

        [Required]
        [StringLength(30)]
        public string TAUTONO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? RECDT { get; set; }

        [StringLength(100)]
        public string RECREM { get; set; }
    }
}
