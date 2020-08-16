using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Improvar.Models
{
    public class MSIGNAUTH
    {
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
        public bool Checked { get; set; }
      
        public short SLNO { get; set; }
    }
}