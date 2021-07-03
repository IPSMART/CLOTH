namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG")]
    public partial class M_SUBLEG
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
        [StringLength(8)]
        public string SLCD { get; set; }

        [Required]
        [StringLength(45)]
        public string SLNM { get; set; }

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

        [StringLength(40)]
        public string DISTRICT { get; set; }

        [StringLength(10)]
        public string PIN { get; set; }

        [Required]
        [StringLength(35)]
        public string COUNTRY { get; set; }

        [StringLength(15)]
        public string SHORTNM { get; set; }

        [StringLength(80)]
        public string FULLNAME { get; set; }

        [StringLength(30)]
        public string GROUPNM { get; set; }

        [StringLength(16)]
        public string ADHAARNO { get; set; }

        [StringLength(15)]
        public string PANNO { get; set; }

        [StringLength(15)]
        public string TANNO { get; set; }

        [StringLength(15)]
        public string GSTNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? GSTDT { get; set; }

        [StringLength(25)]
        public string CINNO { get; set; }

        [StringLength(2)]
        public string SLCOMPTYPE { get; set; }

        [StringLength(100)]
        public string PROPNAME { get; set; }

        [StringLength(20)]
        public string STATNO_1 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
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

        public byte? PREFRTGSSLNO { get; set; }

        public double? GPSLAT { get; set; }

        public double? GPSLOT { get; set; }

        [StringLength(100)]
        public string WEBADDR { get; set; }

        [StringLength(60)]
        public string REGEMAILID { get; set; }

        public byte? REGMOBILEPREFIX { get; set; }

        public long? REGMOBILE { get; set; }

        public int? PHNO1STD { get; set; }

        public long? PHNO1 { get; set; }

        public int? PHNO2STD { get; set; }

        public long? PHNO2 { get; set; }

        public int? PHNO3STD { get; set; }

        public long? PHNO3 { get; set; }

        [StringLength(100)]
        public string FACEBOOK_ID { get; set; }

        [StringLength(100)]
        public string TWITTER_ID { get; set; }

        public long? WHATSAPP_NO { get; set; }

        [StringLength(2)]
        public string STATECD { get; set; }

        public byte? PREFBANKSLNO { get; set; }

        public long M_AUTONO { get; set; }

        [StringLength(4)]
        public string PARTYGRP { get; set; }

        [StringLength(50)]
        public string OFCEMAIL { get; set; }

        [StringLength(1)]
        public string ECOMM { get; set; }

        [StringLength(1)]
        public string REGNTYPE { get; set; }

        [StringLength(8)]
        public string PSLCD { get; set; }

        [StringLength(6)]
        public string AREACD { get; set; }

        [StringLength(100)]
        public string NMONCHQ { get; set; }

        [StringLength(1)]
        public string CMPNONCMP { get; set; }

        [StringLength(50)]
        public string OTHADD1 { get; set; }

        [StringLength(50)]
        public string OTHADD2 { get; set; }

        [StringLength(50)]
        public string OTHADD3 { get; set; }
        [StringLength(50)]
        public string OTHADD4 { get; set; }

        [StringLength(7)]
        public string OTHADDPIN { get; set; }

        [StringLength(50)]
        public string OTHADDEMAIL { get; set; }

        [StringLength(100)]
        public string OTHADDREM { get; set; }
        [StringLength(30)]
        public string SLAREA { get; set; }
        [StringLength(30)]
        public string SLPHNO { get; set; }
        [StringLength(100)]
        public string ACTNAMEOF { get; set; }
        [StringLength(1)]
        public string TCSAPPL { get; set; }
        [StringLength(4)]
        public string PARTYCD { get; set; }
        [StringLength(15)]
        public string CENNO { get; set; }
        [StringLength(40)]
        public string SUBDISTRICT { get; set; }
        [StringLength(1)]
        public string TOT194Q { get; set; }
        [StringLength(1)]
        public string PAN_206AB_CCA { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PANDT { get; set; }
    }
}
