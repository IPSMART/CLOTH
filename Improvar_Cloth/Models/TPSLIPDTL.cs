using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TPSLIPDTL
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

        public double DISCAMT { get; set; }

        public double SCMDISCAMT { get; set; }

        public double TAXAMT { get; set; }

        [StringLength(30)]
        public string DOAUTONO { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }
        public bool Checked { get; set; }
        public double? PACKQNTY { get; set; }

        [StringLength(12)]
        public string ARTNO { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }
        public string UOM { get; set; }
        public double? TOTAL_PCS { get; set; }
        public string ALL_SIZE { get; set; }
        public string ChildData { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }

        [StringLength(20)]
        public string COLRNM { get; set; }
        public string STKTYP_HIDDEN { get; set; }
        public string FREESTK_HIDDEN { get; set; }
        public string RATEIN { get; set; }
    }
}