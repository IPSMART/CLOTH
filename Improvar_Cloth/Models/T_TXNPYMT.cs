namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNPYMT")]
    public partial class T_TXNPYMT
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
        [StringLength(2)]
        public string PYMTCD { get; set; }

        [Required]
        [StringLength(8)]
        public string GLCD { get; set; }

         public double AMT { get; set; }

        [StringLength(16)]
        public string CARDNO { get; set; }

        [StringLength(10)]
        public string INSTNO { get; set; }

        public DateTime? INSTDT { get; set; }

        [StringLength(100)]
        public string PYMTREM { get; set; }
    }
}
