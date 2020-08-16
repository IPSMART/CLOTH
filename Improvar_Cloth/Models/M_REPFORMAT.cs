namespace Improvar.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("M_REPFORMAT")]
    public partial class M_REPFORMAT
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(15)]
        public string REPTYPE { get; set; }

        [Required]
        [Key]
        [Column(Order = 2)]
        [StringLength(15)]
        public string REPDESC { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string FORMNAME { get; set; }
        [StringLength(1)]
        public string REPDEFAULT { get; set; }
        [StringLength(4)]
        public string COMPCD { get; set; }
        [StringLength(4)]
        public string ITGRPCD { get; set; }
    }
}
