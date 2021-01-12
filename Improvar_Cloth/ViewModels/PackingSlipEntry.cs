using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class PackingSlipEntry : Permission
    {
        public M_DOCTYPE M_DOCTYPE { get; set; }
        public M_SITEM M_SITEM { get; set; }
        public T_PSLIP T_PSLIP { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<TPSLIPDTL> TPSLIPDTL { get; set; }
        public List<TPSLIPDTL_SIZE> TPSLIPDTL_SIZE { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list4> DropDown_list4 { get; set; }
        public List<PENDING_DO> PENDING_DO { get; set; }
        public string DOC_ID { get; set; }
        public int SERIAL { get; set; }

        [StringLength(45)]
        public string PartyName { get; set; }
        public string ORDNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDDT { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDASONDT { get; set; }

        [StringLength(15)]
        public string BrandName { get; set; }
        public string GodownName { get; set; }
        public double? LAYER1 { get; set; }
        public double? LAYER2 { get; set; }
        public double? LAYER3 { get; set; }
        public string BAR_CODE { get; set; }
        public double? TOTBOX { get; set; }
        public double? TOTPCS { get; set; }
        public double? SIZE_T_QNTY { get; set; }
        public int? M_SLIP_NO { get; set; }
        public string PRCCD { get; set; }
        public string PRCNM { get; set; }
    }
}