using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMBOMSJOB
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BOMCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime EFFDT { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(15)]
        public string PARTNM { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte SLNO { get; set; }

        [Required]
        [StringLength(2)]
        public string JOBCD { get; set; }

        [StringLength(15)]
        public string JOBNM { get; set; }

        [Required]
        [StringLength(5)]
        public string SJOBCD { get; set; }


        [StringLength(15)]
        public string SJOBNM { get; set; }
        public decimal? JOBRT { get; set; }

        [StringLength(3)]
        public string SEQORDNO { get; set; }

        [StringLength(50)]
        public string REMARK { get; set; }
        public decimal? LENCM { get; set; }
        public decimal? SMV { get; set; }
        public decimal? TRATIO { get; set; }
        [StringLength(6)]
        public string MCCD { get; set; }
        public string MCNM { get; set; }
        public bool Checked { get; set; }
        public decimal? STD_SJ_JOBRT { get; set; }
        [StringLength(20)]
        public string SJOBSTYLE { get; set; }

        [StringLength(20)]
        public string SCATE { get; set; }
    }
}