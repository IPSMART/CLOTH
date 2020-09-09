namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_BALE_HDR")]
    public partial class T_BALE_HDR
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
        public string MUTSLCD { get; set; }

        [StringLength(100)]
        public string TREM { get; set; }

        public short? STARTNO { get; set; }

        [StringLength(2)]
        public string TXTAG { get; set; }
    }
}
