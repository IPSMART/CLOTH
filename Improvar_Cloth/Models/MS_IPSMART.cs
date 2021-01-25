namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_IPSMART")]
    public partial class MS_IPSMART
    {
        public short? EMD_NO { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [StringLength(4)]
        public string CLIENT_CODE { get; set; }

        [StringLength(6)]
        public string APICODE { get; set; }

        [StringLength(30)]
        public string GSPCLIENTAPP { get; set; }
        [Key]
        [Column(Order = 1)]
        [StringLength(200)]
        public string GSPAPPID { get; set; }

        [StringLength(200)]
        public string GSPAPPSECRET { get; set; }

        [StringLength(4000)]
        public string GSPACCESSTKN { get; set; }

        [StringLength(30)]
        public string GSPTKNTYPE { get; set; }

        public DateTime? GSPTKNEXPIN { get; set; }

        public decimal? GSPTKNEXPIN_NO { get; set; }

        [StringLength(20)]
        public string GSPSCOPE { get; set; }

        [StringLength(100)]
        public string GSPJTI { get; set; }
        
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
        public string LIVE_STAGING { get; set; }

        [StringLength(1)]
        public string GSPENABLE { get; set; }
    }
}
