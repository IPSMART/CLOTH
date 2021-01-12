using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TPSLIPDTL_SIZE
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
        public int SLNO { get; set; }

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

        public double DISCAMT { get; set; }

        public double SCMDISCAMT { get; set; }

        public double TAXAMT { get; set; }

        [StringLength(30)]
        public string DOAUTONO { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }
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

        [StringLength(10)]
        public string SIZENM { get; set; }
        [StringLength(20)]
        public string COLRNM { get; set; }
        public double? PACKQNTY { get; set; }
        public string RATEIN_HIDDEN { get; set; }
    }
}