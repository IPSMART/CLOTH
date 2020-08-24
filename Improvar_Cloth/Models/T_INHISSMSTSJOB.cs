namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_INHISSMSTSJOB")]
    public partial class T_INHISSMSTSJOB
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

        [Key]
        [Column(Order = 2)]
        [StringLength(5)]
        public string SJOBCD { get; set; }

        public decimal? JBRT { get; set; }

        public decimal? JBRTEXT { get; set; }

        [StringLength(3)]
        public string SEQORDNO { get; set; }

        public decimal? SAMPPC { get; set; }

       
    }
}
