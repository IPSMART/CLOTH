using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSITEMBOMPART
    {
        public short? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(10)]
        public string BOMCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(15)]
        public string PARTNM { get; set; }

        [Key]
        [Column(Order = 2)]
        public int SLNO { get; set; }

        public decimal? MTRLCOST { get; set; }

        [StringLength(15)]
        public string SJOBNM { get; set; }

        [StringLength(15)]
        public string JOBNM { get; set; }

        [StringLength(2)]
        public string JOBCD { get; set; }
        public decimal? JOBRT { get; set; }

        [StringLength(50)]
        public string REMARK { get; set; }
        public bool Checked { get; set; }
        public string ChildData { get; set; }
        public string ChildData_RMPM { get; set; }
        public string MTRDTL { get; set; }
        public string RMPMDTL { get; set; }

        [StringLength(8)]
        public string SLCD1 { get; set; }

        [StringLength(45)]
        public string SLNM1 { get; set; }

        [StringLength(8)]
        public string SLCD2 { get; set; }

        [StringLength(45)]
        public string SLNM2 { get; set; }

        [StringLength(8)]
        public string SLCD3 { get; set; }

        [StringLength(45)]
        public string SLNM3 { get; set; }
        public decimal? STD_SJ_JOBRT { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }
    }
}