using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class SalesOrderEntry : Permission
    {
        public M_PRCLST M_PRCLST { get; set; }
        public M_DOCTYPE M_DOCTYPE { get; set; }
        public M_JOBMST M_JOBMST { get; set; }
        public M_LINEMAST M_LINEMAST { get; set; }
        public M_SITEM M_SITEM { get; set; }
        public M_PARTS M_PARTS { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }
        public M_JOBMSTSUB M_JOBMSTSUB { get; set; }
        public M_FLRLOCA M_FLRLOCA { get; set; }
        public T_SORD T_SORD { get; set; }
        public List<DropDown_list_DelvType> DropDown_list_DelvType { get; set; }
        public T_SORD_CANC T_SORD_CANC { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<TSORDDTL> TSORDDTL { get; set; }
        public List<TSORDDTL_CANC> TSORDDTL_CANC { get; set; }
        public List<TSORDDTL_CANC_SIZE> TSORDDTL_CANC_SIZE { get; set; }
        public List<TSORDDTL_SIZE> TSORDDTL_SIZE { get; set; }
        public List<TSORDDTL_SEARCHPANEL> TSORDDTL_SEARCHPANEL { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DocumentThrough> DocumentThrough { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list4> DropDown_list4 { get; set; }
        public List<DropDown_list5> DropDown_list5 { get; set; }
        public List<CashOnDelivery> CashOnDelivery { get; set; }
        public List<SCHEME_DTL> SCHEME_DTL { get; set; }
        public string DOC_THRU { get; set; }
        public string DIS_THRU { get; set; }
        public string EFF_DATE_ID { get; set; }
        public string DISC_EFF_DATE_ID { get; set; }
        public string OTHRECMD { get; set; }
        public int SERIAL { get; set; }

        [StringLength(15)]
        public string BrandName { get; set; }

        [StringLength(45)]
        public string PartyName { get; set; }
        [StringLength(45)]
        public string AgentName { get; set; }

        [StringLength(45)]
        public string SalesManName { get; set; }

        [StringLength(30)]
        public string PriceName { get; set; }
        [StringLength(20)]
        public string OrdDocName { get; set; }
        [StringLength(45)]
        public string TransporterName { get; set; }

        [StringLength(45)]
        public string CourierName { get; set; }

        [StringLength(12)]
        public string PayTermName { get; set; }
        public double? TOTPCS { get; set; }
        
        public string DistrictName { get; set; }

        [StringLength(30)]
        public string DiscountName { get; set; }
        public string HIDDEN_SCHEME_DATA { get; set; }
        public string PARTYCD { get; set; }
        public string PARTYNM { get; set; }
        public string AREACD { get; set; }
        public string AREANM { get; set; }
        public string DISCD { get; set; }
        public string DISNM { get; set; }
        public string PRCCD { get; set; }
        public string PRCNM { get; set; }
        public string AGTCD { get; set; }
        public string AGTNM { get; set; }
        public string TRNSCD { get; set; }
        public string TRNSNM { get; set; }
        public string CURCD { get; set; }
        public string CURNM { get; set; }
        public string ORDNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDDT { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ORDASONDT { get; set; }
        public double? RATEPER { get; set; }
        [StringLength(45)]
        public string SaleBillMade { get; set; }
        public double TOTAL_NOOFSETS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
        public double TOTAL_QNTY { get; set; }
        public double TOTAL_BOXES { get; set; }
        public double TOTAL_SETS { get; set; }
        public double TOTAL_SIZE_QNTY { get; set; }
        public bool Edit_Tag { get; set; }
        public string PRTCD { get; set; }
        public string DOCNO { get; set; }
        public string FDT { get; set; }
        public string TDT { get; set; }
        public string CONSLNM { get; set; }
        public string SAGSLNM { get; set; }
    }
    public class SalesOrderEntry_MAINGRID
    {
        public string SerialNo { get; set; }        
        public string ChildData { get; set; }
        public string AMOUNT { get; set; }
        public string BOXES { get; set; }
        public string QNTY { get; set; }
        public string SETS { get; set; }        
        public string NOOFSETS { get; set; }
        public string ALL_SIZE { get; set; }
        public string RATE_DISPLAY { get; set; }
        public string MESSAGE { get; set; }
    }
}