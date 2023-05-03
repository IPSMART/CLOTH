namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_CNTRL_HDR")]
    public partial class T_CNTRL_HDR
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Required]
        [StringLength(4)]
        public string YR_CD { get; set; }

        [Required]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Required]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [StringLength(1)]
        public string MODCD { get; set; }

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(6)]
        public string DOCONLYNO { get; set; }

        public int VCHRNO { get; set; }

        [StringLength(1)]
        public string VCHRSUFFIX { get; set; }

        [Required]
        [StringLength(4)]
        public string MNTHCD { get; set; }

        [StringLength(30)]
        public string DOCNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DOCAMT { get; set; }

        [StringLength(1)]
        public string CALAUTO { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [StringLength(15)]
        public string USR_LIP { get; set; }

        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }

        [StringLength(40)]
        public string LM_USR_ID { get; set; }

        public DateTime? LM_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string LM_USR_LIP { get; set; }

        [StringLength(15)]
        public string LM_USR_SIP { get; set; }

        [StringLength(50)]
        public string LM_USR_OS { get; set; }

        [StringLength(50)]
        public string LM_USR_MNM { get; set; }

        [StringLength(40)]
        public string DEL_USR_ID { get; set; }

        public DateTime? DEL_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string DEL_USR_LIP { get; set; }

        [StringLength(15)]
        public string DEL_USR_SIP { get; set; }

        [StringLength(50)]
        public string DEL_USR_OS { get; set; }

        [StringLength(50)]
        public string DEL_USR_MNM { get; set; }

        [StringLength(1)]
        public string CANCEL { get; set; }

        [StringLength(100)]
        public string CANC_REM { get; set; }

        [StringLength(40)]
        public string CANC_USR_ID { get; set; }

        public DateTime? CANC_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string CANC_USR_LIP { get; set; }

        [StringLength(15)]
        public string CANC_USR_SIP { get; set; }

        [StringLength(50)]
        public string CANC_USR_OS { get; set; }

        [StringLength(50)]
        public string CANC_USR_MNM { get; set; }
        [StringLength(100)]
        public string DEL_REM { get; set; }

        [StringLength(100)]
        public string LM_REM { get; set; }

    }
}
