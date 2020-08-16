namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_LOCKDATA")]
    public partial class M_LOCKDATA
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
        [StringLength(40)]
        public string MENU_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte MENU_INDEX { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(40)]
        public string SCHEMA_NAME { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        public DateTime? LOCKDT { get; set; }

        [StringLength(1)]
        public string BACKDATE { get; set; }

        public long M_AUTONO { get; set; }
        
    }
}
