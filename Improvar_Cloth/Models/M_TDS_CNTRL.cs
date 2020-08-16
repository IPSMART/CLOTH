namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_TDS_CNTRL")]
    public partial class M_TDS_CNTRL
    {
    
        public int? EMD_NO { get; set; }

        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [StringLength(3)]
        public string TDSCODE { get; set; }

        [StringLength(30)]
        public string TDSNM { get; set; }

        [StringLength(10)]
        public string SECNO { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        public long? M_AUTONO { get; set; }

       
    }
}
