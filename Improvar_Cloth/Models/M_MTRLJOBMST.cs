namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_MTRLJOBMST")]
    public partial class M_MTRLJOBMST
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
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [Required]
        [StringLength(15)]
        public string MTRLJOBNM { get; set; }

        [StringLength(2)]
        public string MBATCHINI { get; set; }

        [StringLength(2)]
        public string RMTRLJOBCD { get; set; }

        [StringLength(30)]
        public string MTRLDESC { get; set; }

        [StringLength(1)]
        public string MTBARCODE { get; set; }

        public long M_AUTONO { get; set; }
       
    }
}
