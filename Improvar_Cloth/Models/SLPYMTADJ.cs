using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class SLPYMTADJ
    {
        //public string VCHTYPE { get; set; }
        //public bool VChecked { get; set; }
        //public string VAUTONO { get; set; }
        //public int VSLNO { get; set; }
        //public int POPUPSL { get; set; }
        //public string VDOCNO { get; set; }
        //public string VDOCDT { get; set; }
        //public decimal VAMOUNT { get; set; }
        //public decimal VPRVADJAMT { get; set; }
        //public double VADJAMT { get; set; }
        //public decimal VBALAMT { get; set; }
        //public string VPYTREM { get; set; }
        //public string VBLREM { get; set; }
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
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }

        [StringLength(30)]
        public string I_AUTONO { get; set; }
        public int? I_SLNO { get; set; }

        [StringLength(30)]
        public string R_AUTONO { get; set; }
        public int? R_SLNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? I_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? PRE_ADJ_AMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? ADJ_AMT { get; set; }
        public string DOCNO { get; set; }
        
        public string DOCDT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? AMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? BAL_AMT { get; set; }
        public string BILLNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string BILLDT { get; set; }
        public bool Checked { get; set; }
        public int ParentSerialNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? ParentAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? MainParentAMT { get; set; }
        public string LOCCD { get; set; }
        public string CLASS1CD { get; set; }
        public string CLASS1NM { get; set; }
 
        [StringLength(8)]
        public string BALCLASS1CD { get; set; }

        [StringLength(40)]
        public string BALCLASS1NM { get; set; }
        public string DUE_DT { get; set; }
        public string CONSLCD { get; set; }
        [StringLength(75)]
        public string PYMTREM { get; set; }
    }
}