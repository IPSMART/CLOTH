using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TSORDDTL_CANC_SIZE
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

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(1)]
        public string STKDRCR { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [StringLength(1)]
        public string FREESTK { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        public double? QNTY { get; set; }

        public double? RATE { get; set; }
        public double? CANCQNTY { get; set; }


        [Required]
        public double SCMDISCAMT { get; set; }
        [Required]
        public double DISCAMT { get; set; }

        [Required]
        [StringLength(34)]
        public string ORDAUTOSLNO { get; set; }

        [StringLength(12)]
        public string ARTNO { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }
        public string UOM { get; set; }
        public bool Checked { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }

        [StringLength(20)]
        public string COLRNM { get; set; }
        public short ParentSerialNo { get; set; }
        public string ITCD_HIDDEN { get; set; }
        public string ITNM_HIDDEN { get; set; }
        public string ARTNO_HIDDEN { get; set; }
        public double PCS_HIDDEN { get; set; }
        public double? NOOFSETS_HIDDEN { get; set; }
        public string UOM_HIDDEN { get; set; }
        public string STKTYPE_HIDDEN { get; set; }
        public string FREESTK_HIDDEN { get; set; }
        public double? RATE_HIDDEN { get; set; }
        public double? QNTY_HIDDEN { get; set; }
        public double? TTL_PCS { get; set; }
        public double? TOTAL_BOX { get; set; }
        public double? AMT { get; set; }
        public double? PCSPERBOX { get; set; }
        public string ITCOLSIZE { get; set; }
        public string PRINT_SEQ { get; set; }
    }
}