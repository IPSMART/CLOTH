namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_POST")]
    public partial class M_POST
    {
        public int? EMD_NO { get; set; }

      
      
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [StringLength(8)]
        public string ROGL { get; set; }

        [StringLength(8)]
        public string TCSGL { get; set; }

        [StringLength(8)]
        public string IGST_S { get; set; }

        [StringLength(8)]
        public string CGST_S { get; set; }

        [StringLength(8)]
        public string SGST_S { get; set; }

        [StringLength(8)]
        public string CESS_S { get; set; }

        [StringLength(8)]
        public string DUTY_S { get; set; }

        [StringLength(8)]
        public string IGST_P { get; set; }

        [StringLength(8)]
        public string CGST_P { get; set; }

        [StringLength(8)]
        public string SGST_P { get; set; }

        [StringLength(8)]
        public string CESS_P { get; set; }

        [StringLength(8)]
        public string DUTY_P { get; set; }

        [StringLength(8)]
        public string IGST_RVI { get; set; }

        [StringLength(8)]
        public string CGST_RVI { get; set; }

        [StringLength(8)]
        public string SGST_RVI { get; set; }

        [StringLength(8)]
        public string CESS_RVI { get; set; }

        [StringLength(8)]
        public string IGST_RVO { get; set; }

        [StringLength(8)]
        public string CGST_RVO { get; set; }

        [StringLength(8)]
        public string SGST_RVO { get; set; }

        [StringLength(8)]
        public string CESS_RVO { get; set; }

        [StringLength(8)]
        public string INPTNTCLM_P { get; set; }

        [StringLength(8)]
        public string DISC_GLCD { get; set; }

        [StringLength(8)]
        public string PL_INC_GLCD { get; set; }

        [StringLength(8)]
        public string PL_CAP_GLCD { get; set; }

        [Key]
      
        public long M_AUTONO { get; set; }

        [StringLength(1)]
        public string TDSROUND { get; set; }
    }
}
