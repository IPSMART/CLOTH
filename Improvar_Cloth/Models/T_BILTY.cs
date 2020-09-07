namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_BILTY")]
    public partial class T_BILTY
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
        [StringLength(1)]
        public string DRCR { get; set; }

        [Required]
        [StringLength(30)]
        public string BLAUTONO { get; set; }

        public DateTime? LRDT { get; set; }

        [Required]
        [StringLength(15)]
        public string LRNO { get; set; }

        [Required]
        [StringLength(4)]
        public string BALEYR { get; set; }

        [Required]
        [StringLength(30)]
        public string BALENO { get; set; }

       
    }
}
