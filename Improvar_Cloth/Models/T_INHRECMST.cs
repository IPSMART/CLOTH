namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_INHRECMST")]
    public partial class T_INHRECMST
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

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(6)]
        public string DOCNO { get; set; }

        public DateTime? DOCDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [StringLength(5)]
        public string LINECD { get; set; }

        [StringLength(5)]
        public string DLINECD { get; set; }

        [StringLength(16)]
        public string BATCHNO { get; set; }

        [Required]
        [StringLength(8)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(10)]
        public string EMPCD { get; set; }

        [StringLength(5)]
        public string SJOBCD { get; set; }

         public double? CUTLENGTH { get; set; }

        [StringLength(1)]
        public string QNTYIN { get; set; }

         public double? RATE { get; set; }

         public double? AMT { get; set; }

         public double? JOBEXTRATE { get; set; }

         public double? JOBEXTAMT { get; set; }

        [StringLength(50)]
        public string ITREMARK { get; set; }
    }
}
