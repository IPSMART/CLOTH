using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TSORDDTL_SIZE
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }  

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Required]
        public double SCMDISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
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
        public string UOM_HIDDEN { get; set; }
        public string STKTYPE_HIDDEN { get; set; }
        public string FREESTK_HIDDEN { get; set; }
        public double? QNTY_HIDDEN { get; set; }
        public double? TTL_PCS { get; set; }
        public double? TOTAL_BOX { get; set; }
        public double? AMT { get; set; }
        public double? PCSPERBOX { get; set; }
        public string ITCOLSIZE { get; set; }
        public string PRINT_SEQ { get; set; }
        public double PCSPERSET_HIDDEN { get; set; }
        public string MIXSIZE { get; set; }
    }
}