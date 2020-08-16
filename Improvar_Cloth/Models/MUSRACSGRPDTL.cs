using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
namespace Improvar.Models
{
    public class MUSRACSGRPDTL
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
        public string LINKUSER_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string USER_ID { get; set; }

        public long M_AUTONO { get; set; }
        public bool Checked { get; set; }        
        public int SLNO { get; set; }
        [StringLength(50)]
        public string USER_NAME { get; set; }
    }
}