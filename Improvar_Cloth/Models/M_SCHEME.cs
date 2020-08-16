namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SCHEME")]
    public partial class M_SCHEME
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
        [Column(Order = 0)]
        [StringLength(6)]
        public string SCMCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(6)]
        public string SCMITMGRPCD { get; set; }

        public decimal SCMQNTY { get; set; }

        [Required]
        [StringLength(1)]
        public string SCMBOXPCS { get; set; }

        [StringLength(1)]
        public string INCRMNTL { get; set; }
        
    }
}
