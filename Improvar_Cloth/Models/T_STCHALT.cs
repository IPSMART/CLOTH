namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_STCHALT")]
    public partial class T_STCHALT
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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? TRLDT { get; set; }

        [StringLength(5)]
        public string TRLTIME { get; set; }

        public DateTime? DELVDT { get; set; }

        [StringLength(5)]
        public string DELVTIME { get; set; }

        [StringLength(30)]
        public string OTHERREFNO { get; set; }

        [StringLength(16)]
        public string AGCMNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? AGCMDT { get; set; }

        [StringLength(1)]
        public string INC_RATE { get; set; }

        [StringLength(250)]
        public string REM { get; set; }
    }
}
