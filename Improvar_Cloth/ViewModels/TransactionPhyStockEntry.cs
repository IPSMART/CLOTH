using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Improvar.ViewModels
{
    public class TransactionPhyStockEntry : Permission
    {
        public T_PHYSTK_HDR T_PHYSTK_HDR { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_PHYSTK T_PHYSTK { get; set; }
        public M_SYSCNFG M_SYSCNFG { get; set; }
        public List<TPHYSTK> TPHYSTK { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string DRCR { get; set; }
        public string GONM { get; set; }
        public string PRCNM { get; set; }
        public string PRCCD { get; set; }
        public string BARCODE { get; set; }
        public string SHADE { get; set; }
        public string STYLENO { get; set; }
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        public string PRTBARCODE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? CUTLENGTH { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        public string STKTYPE { get; set; }
        public List<DropDown_list_StkType> DropDown_list_StkType { get; set; }
        public string ITREM { get; set; }
        public string BALENO { get; set; }
        public string BALEYR { get; set; }
        public string LOCABIN { get; set; }
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        public string MTBARCODE { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        public double B_T_NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double B_T_QNTY { get; set; }
        public string TAXGRPCD { get; set; }
        public short SLNO { get; set; }
        public string Last_DOCDT { get; set; }
        public string Last_BARCODE { get; set; }
        public string Last_STYLENO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? EFFDT { get; set; }
        public double RPPERMANUAL { get; set; }
        public double WPPERMANUAL { get; set; }
    }
}