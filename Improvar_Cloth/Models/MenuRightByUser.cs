using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class MenuRightByUser
    {
        public bool Active { get; set; }
        [StringLength(40)]
        public string MENU_NAME { get; set; }
        public byte MENU_INDEX { get; set; }

        [StringLength(20)]
        public string ParentID { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        public bool View { get; set; }
        public bool Check { get; set; }
        public short? A_DAY { get; set; }
        public short? E_DAY { get; set; }
        public short? D_DAY { get; set; }
        [StringLength(40)]
        public string USR_ID { get; set; }
        public string MENU_PROGCALL { get; set; }
        [StringLength(1)]
        public string MENU_type { get; set; }
        [StringLength(1)]
        public string MENU_date_option { get; set; }
        [StringLength(20)]
        public string MENU_ID { get; set; }
    }
}