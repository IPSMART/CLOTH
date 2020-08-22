namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_COLOR")]
    public partial class M_COLOR
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
        [StringLength(4)]
        public string COLRCD { get; set; }

        [StringLength(20)]
        public string COLRNM { get; set; }

        public long M_AUTONO { get; set; }
        [Required]
        [StringLength(4)]
        public string CLRBARCODE { get; set; }

        [StringLength(20)]
        public string ALTCOLRNM { get; set; }
    }
}
