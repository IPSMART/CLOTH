namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNOTH")]
    public partial class T_TXNOTH
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(2)]
        public string DNCNCD { get; set; }

        [StringLength(1)]
        public string DNSALPUR { get; set; }

        [StringLength(100)]
        public string DOCREM { get; set; }

        [StringLength(1)]
        public string ECOMM { get; set; }

        [StringLength(2)]
        public string EXPCD { get; set; }

        [StringLength(15)]
        public string GSTNO { get; set; }

        [StringLength(50)]
        public string PNM { get; set; }

        [StringLength(2)]
        public string POS { get; set; }

        [StringLength(1)]
        public string DOCTH { get; set; }

        [StringLength(1)]
        public string COD { get; set; }

        [StringLength(8)]
        public string AGSLCD { get; set; }

        [StringLength(8)]
        public string SAGSLCD { get; set; }

        [StringLength(5)]
        public string BLTYPE { get; set; }

        [StringLength(30)]
        public string DESTN { get; set; }

        [StringLength(30)]
        public string PLSUPPLY { get; set; }

        [StringLength(50)]
        public string OTHADD1 { get; set; }

        [StringLength(50)]
        public string OTHADD2 { get; set; }

        [StringLength(50)]
        public string OTHADD3 { get; set; }

        [StringLength(50)]
        public string OTHADD4 { get; set; }

        [StringLength(1)]
        public string INSBY { get; set; }

        [StringLength(30)]
        public string PAYTERMS { get; set; }

        [StringLength(500)]
        public string CASENOS { get; set; }

        public byte? NOOFCASES { get; set; }

        [StringLength(4)]
        public string PRCCD { get; set; }

        [StringLength(50)]
        public string OTHNM { get; set; }

        [StringLength(30)]
        public string POREFNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? POREFDT { get; set; }

        [StringLength(50)]
        public string PACKBY { get; set; }

        [StringLength(50)]
        public string SELBY { get; set; }

        [StringLength(50)]
        public string DEALBY { get; set; }

        [StringLength(50)]
        public string DESPBY { get; set; }

        [StringLength(4)]
        public string TAXGRPCD { get; set; }

        [StringLength(3)]
        public string TDSHD { get; set; }

         public double? TDSON { get; set; }

         public double? TDSPER { get; set; }

         public double? TDSAMT { get; set; }
    }
}
