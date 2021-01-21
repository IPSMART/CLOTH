namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_GODOWN")]
    public partial class M_GODOWN
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
        [StringLength(6)]
        public string GOCD { get; set; }

        [Required]
        [StringLength(30)]
        public string GONM { get; set; }

        [StringLength(40)]
        public string DISTRICT { get; set; }

        [StringLength(40)]
        public string GOADD1 { get; set; }

        [StringLength(40)]
        public string GOADD2 { get; set; }

        [StringLength(40)]
        public string GOADD3 { get; set; }

        [StringLength(7)]
        public string PIN { get; set; }

        [StringLength(50)]
        public string GOEMAIL { get; set; }

        [StringLength(30)]
        public string GOPHNO { get; set; }

        [StringLength(25)]
        public string FSSAILICNO { get; set; }

        public long M_AUTONO { get; set; }
        [StringLength(4)]
        public string LOCCD { get; set; }
        [StringLength(10)]
        public string FLAG1 { get; set; }
        


    }
}
