namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_EMPMAS")]
    public partial class M_EMPMAS
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(10)]
        public string EMPCD { get; set; }

        [StringLength(5)]
        public string TITLE { get; set; }

        [StringLength(75)]
        public string ENM { get; set; }

        [StringLength(25)]
        public string EFNM { get; set; }

        [StringLength(25)]
        public string EMNM { get; set; }

        [StringLength(25)]
        public string ELNM { get; set; }

        [StringLength(30)]
        public string ESHNM { get; set; }

        public DateTime? DOB { get; set; }

        [StringLength(20)]
        public string BPLACE { get; set; }

        public DateTime? DOJ { get; set; }

        public DateTime? DOL { get; set; }

        [StringLength(1)]
        public string SEX { get; set; }

        [StringLength(20)]
        public string RELGN { get; set; }

        [StringLength(20)]
        public string NATLTY { get; set; }

        [StringLength(30)]
        public string EGNM { get; set; }

        [StringLength(30)]
        public string EGRELAT { get; set; }

        [StringLength(1)]
        public string EMRTLSTATUS { get; set; }

        [StringLength(40)]
        public string QALIF { get; set; }

        public byte? EDEPNTNO { get; set; }

        [StringLength(15)]
        public string MOBILE1 { get; set; }

        [StringLength(15)]
        public string MOBILE2 { get; set; }

        [StringLength(15)]
        public string ETELNO { get; set; }

        [StringLength(4)]
        public string ETYPE { get; set; }

        [StringLength(40)]
        public string PADD1 { get; set; }

        [StringLength(40)]
        public string PADD2 { get; set; }

        [StringLength(5)]
        public string PDISTCD { get; set; }

        [StringLength(40)]
        public string PDISTRICT { get; set; }

        [StringLength(30)]
        public string PSTATE { get; set; }

        [StringLength(10)]
        public string PPIN { get; set; }

        [StringLength(35)]
        public string PCOUNTRY { get; set; }

        [StringLength(30)]
        public string PTELNO { get; set; }

        [StringLength(40)]
        public string CADD1 { get; set; }

        [StringLength(40)]
        public string CADD2 { get; set; }

        [StringLength(5)]
        public string CDISTCD { get; set; }

        [StringLength(40)]
        public string CDISTRICT { get; set; }

        [StringLength(30)]
        public string CSTATE { get; set; }

        [StringLength(10)]
        public string CPIN { get; set; }

        [StringLength(35)]
        public string CCOUNTRY { get; set; }

        [StringLength(30)]
        public string CTELNO { get; set; }

        [StringLength(40)]
        public string DADD1 { get; set; }

        [StringLength(40)]
        public string DADD2 { get; set; }

        [StringLength(5)]
        public string DDISTCD { get; set; }

        [StringLength(40)]
        public string DDISTRICT { get; set; }

        [StringLength(30)]
        public string DSTATE { get; set; }

        [StringLength(10)]
        public string DPIN { get; set; }

        [StringLength(35)]
        public string DCOUNTRY { get; set; }

        [StringLength(30)]
        public string DTELNO { get; set; }

        [StringLength(100)]
        public string EMAIL_B { get; set; }

        [StringLength(100)]
        public string EMAIL_P { get; set; }

        [StringLength(22)]
        public string PFNO { get; set; }

        [StringLength(20)]
        public string PENSNO { get; set; }

        [StringLength(20)]
        public string ESINO { get; set; }

        [StringLength(20)]
        public string PANNO { get; set; }

        [StringLength(20)]
        public string TNO { get; set; }

        [StringLength(20)]
        public string UAN { get; set; }

        [StringLength(20)]
        public string PASSNO { get; set; }

        public DateTime? PASSDT { get; set; }

        public DateTime? PASSVLDT { get; set; }

        [StringLength(30)]
        public string PASSPLACE { get; set; }

        [StringLength(20)]
        public string DRVNGLIC { get; set; }

        public DateTime? DRVNGEXP { get; set; }

        [StringLength(5)]
        public string BLOOD_GRP { get; set; }

        public DateTime? PFDT { get; set; }

        public DateTime? ESIDT { get; set; }

        public DateTime? GRTDT { get; set; }

        [StringLength(4)]
        public string LEAVREAS { get; set; }

        public DateTime? TRNSFDT { get; set; }

        [StringLength(30)]
        public string TRNSFPLACE { get; set; }

        [StringLength(8)]
        public string GLCD { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [StringLength(8)]
        public string CLASS1CD { get; set; }

        [StringLength(8)]
        public string CLASS2CD { get; set; }

        [StringLength(1)]
        public string PYT_MODE { get; set; }

        [StringLength(30)]
        public string BANKNM { get; set; }

        [StringLength(30)]
        public string BANKBRNCH { get; set; }

        [StringLength(30)]
        public string BANKACNO { get; set; }

        [StringLength(20)]
        public string BANKIFSC { get; set; }

        [StringLength(1)]
        public string BANKAC { get; set; }

        [StringLength(30)]
        public string IMPVR_LOGINID { get; set; }

        [StringLength(15)]
        public string LEAGACYCD { get; set; }

        [StringLength(50)]
        public string MCLAIM_INSCO { get; set; }

        [StringLength(20)]
        public string MCLAIM_POLNO { get; set; }

        public long? MCLAIM_POLAMT { get; set; }

        [StringLength(10)]
        public string LEGACYOTH { get; set; }

        public long? M_AUTONO { get; set; }
    }
}
