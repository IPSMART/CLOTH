namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_LOCA")]
    public partial class M_LOCA
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
        public string LOCCD { get; set; }

        [StringLength(6)]
        public string LOCA_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Required]
        [StringLength(60)]
        public string LOCNM { get; set; }

        [StringLength(60)]
        public string ADD1 { get; set; }

        [StringLength(60)]
        public string ADD2 { get; set; }

        [StringLength(60)]
        public string ADD3 { get; set; }

        [StringLength(60)]
        public string ADD4 { get; set; }

        [StringLength(60)]
        public string ADD5 { get; set; }

        [StringLength(60)]
        public string ADD6 { get; set; }

        [StringLength(60)]
        public string ADD7 { get; set; }

        [StringLength(80)]
        public string BLDGNO { get; set; }

        [StringLength(80)]
        public string PREMISES { get; set; }

        [StringLength(80)]
        public string FLOORNO { get; set; }

        [StringLength(80)]
        public string ROADNAME { get; set; }

        [StringLength(60)]
        public string EXTADDR { get; set; }

        [StringLength(80)]
        public string LOCALITY { get; set; }

        [StringLength(80)]
        public string LANDMARK { get; set; }

        [Required]
        [StringLength(35)]
        public string STATE { get; set; }

        [StringLength(35)]
        public string DISTRICT { get; set; }

        [StringLength(10)]
        public string PIN { get; set; }

        [Required]
        [StringLength(35)]
        public string COUNTRY { get; set; }

        [StringLength(15)]
        public string SHORTNM { get; set; }

        [StringLength(2)]
        public string STATECD { get; set; }

        [StringLength(15)]
        public string TANNO { get; set; }

        [StringLength(15)]
        public string GSTNO { get; set; }

        public DateTime? GSTDT { get; set; }

        [StringLength(20)]
        public string STATNO_1 { get; set; }

        public DateTime? STATDT_1 { get; set; }

        [StringLength(20)]
        public string STATNO_2 { get; set; }

        public DateTime? STATDT_2 { get; set; }

        [StringLength(20)]
        public string STATNO_3 { get; set; }

        public DateTime? STATDT_3 { get; set; }

        [StringLength(20)]
        public string STATNO_4 { get; set; }

        public DateTime? STATDT_4 { get; set; }

        [StringLength(20)]
        public string STATNO_5 { get; set; }

        public DateTime? STATDT_5 { get; set; }

        [StringLength(40)]
        public string TDS_COMPNM { get; set; }

        [StringLength(40)]
        public string TDS_SIG_NM { get; set; }

        [StringLength(30)]
        public string TDS_SIG_DESIG { get; set; }

        [StringLength(20)]
        public string TDS_SIG_PAN { get; set; }

        [StringLength(60)]
        public string TDS_SIG_ADD1 { get; set; }

        [StringLength(60)]
        public string TDS_SIG_ADD2 { get; set; }

        [StringLength(60)]
        public string TDS_SIG_ADD3 { get; set; }

        [StringLength(7)]
        public string TDS_SIG_PIN { get; set; }

        public long? TDS_SIG_MOB { get; set; }

        [StringLength(12)]
        public string TDS_SIG_PHNO { get; set; }

        [StringLength(2)]
        public string TDS_SIG_STATECD { get; set; }

        public byte? PREFRTGSSLNO { get; set; }

        public double? GPSLAT { get; set; }

        public double? GPSLOT { get; set; }

        [StringLength(100)]
        public string WEBADDR { get; set; }

        [StringLength(60)]
        public string REGEMAILID { get; set; }

        public byte? REGMOBILEPREFIX { get; set; }

        public long? REGMOBILE { get; set; }

        public short? PHNO1STD { get; set; }

        public long? PHNO1 { get; set; }

        public short? PHNO2STD { get; set; }

        public long? PHNO2 { get; set; }

        public short? PHNO3STD { get; set; }

        public long? PHNO3 { get; set; }

        [StringLength(100)]
        public string FACEBOOK_ID { get; set; }

        [StringLength(100)]
        public string TWITTER_ID { get; set; }

        public long? WHATSAPP_NO { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(1)]
        public string SILENT { get; set; }

        [StringLength(4)]
        public string SUBLOCCD { get; set; }

        [StringLength(2)]
        public string LBATCHINI { get; set; }
    }
}
