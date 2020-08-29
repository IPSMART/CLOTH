namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_BATCHDTL")]
    public partial class T_BATCHDTL
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
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        public short TXNSLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(25)]
        public string BARNO { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string STKDRCR { get; set; }

        public decimal? NOS { get; set; }

        public decimal? QNTY { get; set; }

        public decimal? BLQNTY { get; set; }

        public decimal? FLAGMTR { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }

        public decimal? RATE { get; set; }

        public decimal? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }

        public decimal? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }

        public decimal? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

        public decimal? DIA { get; set; }

        public decimal? CUTLENGTH { get; set; }

        [StringLength(10)]
        public string LOCABIN { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        [StringLength(15)]
        public string MILLNM { get; set; }

        [StringLength(40)]
        public string BATCHNO { get; set; }

        [StringLength(30)]
        public string BALENO { get; set; }

        [StringLength(30)]
        public string RECPROGAUTONO { get; set; }

        [StringLength(15)]
        public string RECPROGLOTNO { get; set; }

        public short? RECPROGSLNO { get; set; }

       
    }
}
