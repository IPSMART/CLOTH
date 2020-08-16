using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MCNTRLHDRDOC
    {
        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long M_AUTONO { get; set; }

        [Required]
        [StringLength(50)]
        public string M_TBLNM { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte SLNO { get; set; }

        [Required]
        [StringLength(100)]
        public string DOC_FLNAME { get; set; }

        [Required]
        [StringLength(10)]
        public string DOC_EXTN { get; set; }

        [Required]
        [StringLength(15)]
        public string DOC_CTG { get; set; }

        [StringLength(300)]
        public string DOC_DESC { get; set; }
    }
}