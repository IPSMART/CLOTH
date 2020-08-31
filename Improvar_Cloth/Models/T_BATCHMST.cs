namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_BATCHMST")]
    public partial class T_BATCHMST
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
        [StringLength(25)]
        public string BARNO { get; set; }

        [Required]
        [StringLength(30)]
        public string AUTONO { get; set; }

        public short SLNO { get; set; }

        [StringLength(8)]
        public string SLCD { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }

        [Required]
        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public decimal? NOS { get; set; }

        public decimal? QNTY { get; set; }

        public decimal? RATE { get; set; }

        public decimal? AMT { get; set; }

        public decimal? FLAGMTR { get; set; }

        public decimal? MTRL_COST { get; set; }

        public decimal? OTH_COST { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }

        [StringLength(30)]
        public string PDESIGN { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [StringLength(30)]
        public string ORGBATCHAUTONO { get; set; }

        public short? ORGBATCHSLNO { get; set; }

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
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

       
    }
}
