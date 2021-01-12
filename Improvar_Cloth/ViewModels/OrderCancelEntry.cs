using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class OrderCancelEntry : Permission
    {
        
        public M_DOCTYPE M_DOCTYPE { get; set; }       
        public M_SITEM M_SITEM { get; set; }      
        public T_SORD_CANC T_SORD_CANC { get; set; }
        public T_DO_CANC T_DO_CANC { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<TSORDDTL_CANC> TSORDDTL_CANC { get; set; }
        public List<TSORDDTL_CANC_SIZE> TSORDDTL_CANC_SIZE { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list4> DropDown_list4 { get; set; }
        public string DOC_ID { get; set; }
        public int SERIAL { get; set; }
      
        [StringLength(45)]
        public string PartyName { get; set; }       
        public string ORDNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDDT { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDASONDT { get; set; }
    }
}