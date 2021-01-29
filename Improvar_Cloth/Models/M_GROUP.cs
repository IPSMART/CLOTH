namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_GROUP")]
    public partial class M_GROUP
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
        [StringLength(4)]
        public string ITGRPCD { get; set; }

        [StringLength(30)]
        public string ITGRPNM { get; set; }

        [StringLength(25)]
        public string GRPNM { get; set; }

        [StringLength(1)]
        public string ITGRPTYPE { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(4)]
        public string PRODGRPCD { get; set; }

        [StringLength(1)]
        public string BARGENTYPE { get; set; }

        [StringLength(25)]
        public string SHORTNM { get; set; }

        [StringLength(8)]
        public string SALGLCD { get; set; }

        [StringLength(8)]
        public string PURGLCD { get; set; }

        [StringLength(8)]
        public string SALRETGLCD { get; set; }

        [StringLength(8)]
        public string PURRETGLCD { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(1)]
        public string NEGSTOCK { get; set; }

        [StringLength(5)]
        public string WPPRICEGEN { get; set; }

        [StringLength(5)]
        public string RPPRICEGEN { get; set; }

        [StringLength(2)]
        public string GRPBARCODE { get; set; }
        [StringLength(20)]
        public string SAPCODE { get; set; }

    }
}
