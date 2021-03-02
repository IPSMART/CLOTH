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
        [StringLength(30)]
        public string I_AUTONO { get; set; }
        public int? I_SLNO { get; set; }        
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