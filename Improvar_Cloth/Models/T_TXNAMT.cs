namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNAMT")]
    public partial class T_TXNAMT
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

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(4)]
        public string AMTCD { get; set; }

        [StringLength(50)]
        public string AMTDESC { get; set; }

        public decimal AMTRATE { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        public decimal? AMT { get; set; }

        public decimal? CURR_AMT { get; set; }

        public decimal? IGSTAMT { get; set; }

        public decimal? IGSTPER { get; set; }

        public decimal? CGSTAMT { get; set; }

        public decimal? CGSTPER { get; set; }

        public decimal? SGSTAMT { get; set; }

        public decimal? SGSTPER { get; set; }

        public decimal? CESSAMT { get; set; }

        public decimal? CESSPER { get; set; }

        public decimal? DUTYAMT { get; set; }

        public decimal? DUTYPER { get; set; }
    }
}
