using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TSORDDTL
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

        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }

        //[DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        //public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? AMOUNT { get; set; }


        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Required]
        public double DISCAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Required]
        public double SCMDISCAMT { get; set; }

        [StringLength(12)]
        public string ARTNO { get; set; }

        [StringLength(60)]
        public string ITNM { get; set; }
        public string UOM { get; set; }
        public double? TOTAL_PCS { get; set; }
        public string ALL_SIZE { get; set; }
        public string RATE_DISPLAY { get; set; }
        public bool Checked { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }

        [StringLength(20)]
        public string COLRNM { get; set; }
        public double? NOOFSETS { get; set; }
        public double? PCSPERSET { get; set; }
        public double? BOXES { get; set; }
        public double? SETS { get; set; }
        [StringLength(4)]
        public string ITGRPCD { get; set; }
        [StringLength(30)]
        public string ITGRPNM { get; set; }
        [StringLength(8)]
        public string FABITCD { get; set; }
        [StringLength(60)]
        public string FABITNM { get; set; }
        public string GRPNM { get; set; }
        public string STYLENO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DELVDT { get; set; }

        [StringLength(500)]
        public string ITREM { get; set; }
        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

        [Required]
        [StringLength(4)]
        public string PRCCD { get; set; }

        public DateTime? PRCEFFDT { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        public string PDESIGN { get; set; }
        
    }
}