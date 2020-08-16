namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APPL_MENU")]
    public partial class APPL_MENU
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string MENU_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string MENU_NAME { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte MENU_INDEX { get; set; }

        [StringLength(20)]
        public string MENU_PARENT_ID { get; set; }

        public byte? MENU_IMAGE { get; set; }

        [StringLength(1)]
        public string MENU_TYPE { get; set; }

        [StringLength(10)]
        public string MENU_ORDER_CODE { get; set; }

        [StringLength(1)]
        public string MENU_DATE_OPTION { get; set; }

        [StringLength(10)]
        public string MODULE_CODE { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(20)]
        public string MENU_PROGCALL { get; set; }

        [StringLength(20)]
        public string MENU_FIND_ID { get; set; }

        [StringLength(20)]
        public string MENU_PARA { get; set; }

        [StringLength(100)]
        public string MENU_DOCCD { get; set; }
    }
}
