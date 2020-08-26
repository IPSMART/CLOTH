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

        public double AMTRATE { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        public double? AMT { get; set; }

        public double? CURR_AMT { get; set; }

        public double? IGSTAMT { get; set; }

        public double? IGSTPER { get; set; }

        public double? CGSTAMT { get; set; }

        public double? CGSTPER { get; set; }

        public double? SGSTAMT { get; set; }

        public double? SGSTPER { get; set; }

        public double? CESSAMT { get; set; }

        public double? CESSPER { get; set; }

        public double? DUTYAMT { get; set; }

        public double? DUTYPER { get; set; }
    }
}
