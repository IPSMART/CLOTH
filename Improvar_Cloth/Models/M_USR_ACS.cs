namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_USR_ACS")]
    public partial class M_USR_ACS
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(40)]
        public string USER_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string MENU_NAME { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte MENU_INDEX { get; set; }

        [StringLength(5)]
        public string USER_RIGHT { get; set; }

        public short? A_DAY { get; set; }

        public short? E_DAY { get; set; }

        public short? D_DAY { get; set; }

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

        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [StringLength(15)]
        public string USR_LIP { get; set; }

        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }
        [StringLength(4)]
        public string CLCD { get; set; }
    }
}
