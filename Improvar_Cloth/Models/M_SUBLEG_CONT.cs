namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SUBLEG_CONT")]
    public partial class M_SUBLEG_CONT
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
        [Column(Order = 0)]
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte SLNO { get; set; }
        
        [StringLength(60)]
        public string CPERSON { get; set; }

        [StringLength(45)]
        public string DESIG { get; set; }

        public int? EXTENSION { get; set; }

        public byte? MOBILE1PREFIX { get; set; }

        public long? MOBILE1 { get; set; }

        public byte? MOBILE2PREFIX { get; set; }

        public long? MOBILE2 { get; set; }

        public int? PHNO1STD { get; set; }

        public long? PHNO1 { get; set; }

        [StringLength(60)]
        public string PERSEMAIL { get; set; }

        public DateTime? PERSDOB { get; set; }

        public DateTime? PERSDOA { get; set; }

        [StringLength(1)]
        public string DEPT { get; set; }
       
    }
}
