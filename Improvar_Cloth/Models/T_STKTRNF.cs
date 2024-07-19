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
        [StringLength(6)]
        public string TOGOCD { get; set; }

        [Required]
        [StringLength(4)]
        public string TOLOCCD { get; set; }

        [Required]
        [StringLength(4)]
        public string TOCOMPCD { get; set; }

        [StringLength(30)]
        public string OTHAUTONO { get; set; }

        [StringLength(4)]
        public string SCOMPCD { get; set; }

        [StringLength(4)]
        public string SLOCCD { get; set; }

        [StringLength(6)]
        public string SGOCD { get; set; }

    }
}
