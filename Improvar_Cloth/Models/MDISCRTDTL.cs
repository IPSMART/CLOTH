namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    public class MDISCRTDTL
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
        [Column(Order = 0)]
        [StringLength(4)]
        public string DISCRTCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(6)]
        public string SCMITMGRPCD { get; set; }

        public double DISCPER { get; set; }

        public double DISCRATE { get; set; }
        public byte SLNO { get; set; }
    
        public bool Checked { get; set; }

        [StringLength(30)]
        public string SCMITMGRPNM { get; set; }
    }
}
