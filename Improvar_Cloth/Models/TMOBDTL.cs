using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TMOBDTL
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        public short TXNSLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(6)]
        public string GOCD { get; set; }

        [Required]
        [StringLength(25)]
        public string BARNO { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string STKDRCR { get; set; }

        public decimal? NOS { get; set; }

        public double? QNTY { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }

        public decimal? CUTLENGTH { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        public decimal? RATE { get; set; }

        [StringLength(30)]
        public string ADJAUTONO { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        public double CNTBARNO { get; set; }
        public bool Checked { get; set; }
        public string FAVCOLR { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        public string UOM { get; set; }
        public string MTRLJOBNM { get; set; }

    }
}