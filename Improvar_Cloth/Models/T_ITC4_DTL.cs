namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_ITC4_DTL")]
    public partial class T_ITC4_DTL
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

        [StringLength(2)]
        public string JOBCD { get; set; }

        [Required]
        [StringLength(5)]
        public string DOCCD { get; set; }

        [Required]
        [StringLength(3)]
        public string UOMCD { get; set; }
        public decimal QNTY { get; set; }

        public decimal SHORTQNTY { get; set; }

        public decimal RATE { get; set; }
    }
}
