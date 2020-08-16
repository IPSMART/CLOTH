using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class MISC_BILL_ENTRY : Permission
    {
        public MS_CURRENCY MS_CURRENCY { get; set; }
        public M_GENLEG M_GENLEG { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }
        public M_CLASS1 M_CLASS1 { get; set; }
        public M_CLASS2 M_CLASS2 { get; set; }
        public M_TDS_CNTRL M_TDS_CNTRL { get; set; }
        public T_MBILL_HDR T_MBILL_HDR { get; set; }
        public T_VCH_HDR T_VCH_HDR { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<TMBILLCLASS> TMBILLCLASS { get; set; }
        public List<TMBILLDET> TMBILLDET { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DebitCreditType> DebitCreditType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public bool Checked { get; set; }
        public bool R_Checked { get; set; }
        public bool L_Checked { get; set; }
        public int SERIAL { get; set; }
        public string REF_CODE { get; set; }
        public string CLASS1_CODE { get; set; }
        public string CLASS2_CODE { get; set; }
        public string REFNM { get; set; }
        public string LOW_TDS { get; set; }
        public string LOW_TDS_DESC { get; set; }
        public double? NETPAYAMT { get; set; }

        [StringLength(16)]
        public string AGREFNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? AGREFDT { get; set; }

        [StringLength(10)]
        public string AGENT_CODE { get; set; }

        [StringLength(45)]
        public string AGENT_NAME { get; set; }
    }
}