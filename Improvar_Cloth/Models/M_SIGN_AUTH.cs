namespace Improvar.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("M_SIGN_AUTH")]
    public partial class M_SIGN_AUTH
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(5)]
        public string AUTHCD { get; set; }

        [StringLength(40)]
        public string AUTHNM { get; set; }

        [StringLength(40)]
        public string USRID { get; set; }

        [StringLength(20)]
        public string SIGN_IMG { get; set; }

        [StringLength(10)]
        public string EMPID { get; set; }

        [StringLength(5)]
        public string DESIGCD { get; set; }

        public long? M_AUTONO { get; set; }
        [StringLength(4)]
        public string CLCD { get; set; }
    }
}
