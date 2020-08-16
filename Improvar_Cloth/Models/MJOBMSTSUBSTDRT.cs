using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Improvar.Models
{
    public class MJOBMSTSUBSTDRT
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(5)]
        public string SJOBCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string JOBPRCCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        public decimal JOBRT { get; set; }

        public long M_AUTONO { get; set; }




        //extra         public M_JOBPRCCD M_JOBMST { get; set; }
        [StringLength(2)]
        public string JOBCD { get; set; }

        [Required]
        [StringLength(15)]
        public string JOBNM { get; set; }

        [StringLength(15)]
        public string JOBPRCNM { get; set; }
        public bool Checked { get; set; }
        public int SLNO { get; set; }
        [StringLength(20)]
        public string SJOBNM { get; set; }

        [StringLength(20)]
        public string SJOBSTYLE { get; set; }

        [StringLength(10)]
        public string SJOBMCHN { get; set; }
        public decimal PREVIOUS_JOBRT { get; set; }
    }
}