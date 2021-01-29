namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USER_ACTIVITY")]
    public partial class USER_ACTIVITY
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(40)]
        public string USER_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SESSION_NO { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string MENU_ID { get; set; }

        [Key]
        [Column(Order = 5)]
        public byte MENU_INDEX { get; set; }

        [Key]
        [Column(Order = 6)]
        public byte MENU_SESSION { get; set; }

        [StringLength(10)]
        public string MODULE_CODE { get; set; }

        public DateTime? IN_DATE { get; set; }

        public DateTime? OUT_DATE { get; set; }
    }
}
