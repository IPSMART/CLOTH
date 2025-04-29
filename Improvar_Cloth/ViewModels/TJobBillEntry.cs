using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TJobBillEntry : Permission
    {
        public string MSG { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_JBILL T_JBILL { get; set; }
        public List<T_JBILLDTL> T_JBILLDTL { get; set; }
        public List<T_TXN_LINKNO> T_TXN_LINKNO { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DocumentType> Reverse_Charge { get; set; }
        public List<DocumentType> LowerTDSType { get; set; }
        public List<DocumentType> DueCal { get; set; }
        public T_CNTRL_HDR_DOC T_CNTRL_HDR_DOC { get; set; }
        public T_CNTRL_HDR_DOC_DTL T_CNTRL_HDR_DOC_DTL { get; set; }
        public List<Pending_Challan_SLIP> Pending_SLIP { get; set; }
        public List<PendingChallanItemDetails> ItemDetails { get; set; }
        public List<PendingChallanDr_Cr_NoteDetails> DRCRDetails { get; set; }
        public List<PendingChallan_SBill_Sortage> SBillSortage { get; set; }
        public List<TTXNAMT> TTXNAMT { get; set; }
        public string JOBNM { get; set; }
        public string PRODGRPCD { get; set; }
        public string SLNM { get; set; }
        public string BrokerSLNM { get; set; }
        public string CURRNM { get; set; }
        public string UOMNM { get; set; }
        public string TAXGRPNM { get; set; }
        public string TDSNM { get; set; }
        public string LOW_TDS_DESC { get; set; }
        public double TOTAL_P_QNTY { get; set; }
        public double TOTAL_P_NOS { get; set; }
        public bool Roundoff_Item { get; set; }
        public double RoundoffAMT_Item { get; set; }
        public bool Roundoff_DCNote { get; set; }
        public double RoundoffAMT_DCNote { get; set; }
        public double BackupIGSTPER { get; set; }
        public double BackupCGSTPER { get; set; }
        public double BackupSGSTPER { get; set; }
        public double BackupCESSTPER { get; set; }
        public double TOTALBILLAMT_ITEM { get; set; }
        public double TOTALBILLAMT_DRCRNOTE { get; set; }
        public double NETPAYAMT { get; set; }
        public double TAXABLEVAL_ITEM { get; set; }
        public double TAXABLEVAL_DNCNNOTE { get; set; }
        public string ChildData_PendingSlip { get; set; }
        public string CREGLNM { get; set; }
        public string EXPGLNM { get; set; }
        public string EXPGLCD1 { get; set; }
        public string EXPGLNM1 { get; set; }
        public bool Roundoff_sbill { get; set; }
        public double RoundoffAMT_sbill { get; set; }
        public double TOTALBILLAMT_sbill { get; set; }
        public double TAXABLEVAL_sbill { get; set; }
        public string BackupTable { get; set; }
        public string TDSROUND { get; set; }
        public double TOTALBILLQNTY_ITEM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_CURR { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_AMOUNT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_IGST { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_SGST { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_CGST { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_CESS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_DUTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? A_T_NET { get; set; }
        public double T_RECQNTY { get; set; }
        public double T_PASSQNTY { get; set; }
        public double T_BillQNTY { get; set; }
        public double T_AMOUNT { get; set; }
        public double T_DISCAMT { get; set; }
        public double T_TAXABLE { get; set; }
        public double T_cgstamt { get; set; }
        public double T_sgstamt { get; set; }
        public double T_cessamt { get; set; }
        public double T_NETAMOUNT { get; set; }
        public double T_igstamt { get; set; }
        public string M_SLIP_NO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_RECEVEQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_BillQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_PASSQNTY { get; set; }
        public double Total_NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_AMOUNT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_ShortQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_DISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_addless { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_TAXABLE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_igstamt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_cgstamt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_sgstamt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_cessamt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Total_NETAMOUNT { get; set; }
        public string Item_UomTotal { get; set; }
    }
    public class PendingChallanItemDetails
    {
        public string PRODGRPCD { get; set; }
        public short SLNO { get; set; }
        public string AUTONO { get; set; }
        public string ITCD { get; set; }
        public string STYLENO { get; set; }
        public string ITNM { get; set; }
        public string ITREM { get; set; }
        public List<DocumentType> QNTY_UNIT_PC { get; set; }
        public string qtncalcon { get; set; }       
        public double RECQNTY { get; set; }
        public double RECQNTY_DISPLAY { get; set; }
        public double BillQNTY { get; set; }        
        public double PASSQNTY { get; set; }       
        public double ShortQNTY { get; set; }
        public double ShortQNTY_DISPLAY { get; set; }
        public double RATE { get; set; }
        public double AMOUNT { get; set; }
        public double DISCPER { get; set; }
        public double DISCAMT { get; set; }
        public double TAXABLE { get; set; }
        public double igstper { get; set; }
        public double igstper_hidden { get; set; }
        public double igstamt { get; set; }
        public double cgstper { get; set; }
        public double cgstper_hidden { get; set; }
        public double cgstamt { get; set; }
        public double sgstper { get; set; }
        public double sgstper_hidden { get; set; }
        public double sgstamt { get; set; }
        public double cessper { get; set; }
        public double cessper_hidden { get; set; }
        public double cessamt { get; set; }
        public double NETAMOUNT { get; set; }
        public string PARTCD { get; set; }
        public DateTime EFFDT { get; set; }
        public bool Checked { get; set; }
        public string HSNSACCD { get; set; }
        public double addless { get; set; }
        public double? NOS { get; set; }
        public string UOM { get; set; }
    }
    public class PendingChallanDr_Cr_NoteDetails
    {
        public List<DocumentType> DocumentCode { get; set; }
        public string DcodeDRCR { get; set; }
        public string PRODGRPCD { get; set; }
        public string AUTONO { get; set; }
        public short SLNO { get; set; }
        public string ADOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ADOCDT { get; set; }
        public string ITCD { get; set; }
        public string STYLENO { get; set; }
        public string ITNM { get; set; }
        public int PCSPERBOX { get; set; }
        public List<DocumentType> QNTY_UNIT_DNCN { get; set; }
        public string qtncalcon { get; set; }       
        public double QUAN { get; set; }       
        public double RATE { get; set; }
        public double AMOUNT { get; set; }
        public double TAXABLE { get; set; }
        public double igstper { get; set; }
        public double igstamt { get; set; }
        public double cgstper { get; set; }
        public double cgstamt { get; set; }
        public double sgstper { get; set; }
        public double sgstamt { get; set; }
        public double cessper { get; set; }
        public double cessamt { get; set; }
        public double NETAMOUNT { get; set; }
        public DateTime EFFDT { get; set; }
        public string PARTCD { get; set; }
        public bool Checked { get; set; }
        public bool RelWithItem { get; set; }
        public string ADOCNO_AUTONO { get; set; }
    }
    public class Pending_Challan_SLIP
    {
        public short SLNO { get; set; }
        public string AUTONO { get; set; }
        public string ITCD { get; set; }
        public string STYLENO { get; set; }
        public string ITNM { get; set; }
        public double QNTY { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string SLCD { get; set; }
        public double SHORTQNTY { get; set; }
        public bool Checked { get; set; }
        public string PARTCD { get; set; }
        public string ISSAUTONO { get; set; }
        public string ISSDOCNO { get; set; }
        public string ISSDOCDT { get; set; }
        public string PREFNO { get; set; }
        public string PREFDT { get; set; }
        public string HSNSACCD { get; set; }
        public double RATE { get; set; }
        public double? NOS { get; set; }
    }
    public static class Misc_Function
    {
        public static string convertDateTimeToString(DateTime? DT)
        {
            return DT.Value.ToString("dd/MM/yyyy");
        } 
    }
    public class PendingChallan_SBill_Sortage
    {
        public string PRODGRPCD { get; set; }
        public string AUTONO { get; set; }
        public short SLNO { get; set; }
        public string ITCD { get; set; }
        public string STYLENO { get; set; }
        public string ITNM { get; set; }
        public List<DocumentType> QNTY_UNIT_DNCN { get; set; }
        public string qtncalcon { get; set; }
        public double QUAN { get; set; }
        public double RATE { get; set; }
        public double AMOUNT { get; set; }
        public double TAXABLE { get; set; }
        public double igstper { get; set; }
        public double igstamt { get; set; }
        public double cgstper { get; set; }
        public double cgstamt { get; set; }
        public double sgstper { get; set; }
        public double sgstamt { get; set; }
        public double cessper { get; set; }
        public double cessamt { get; set; }
        public double NETAMOUNT { get; set; }
        public DateTime EFFDT { get; set; }
        public string PARTCD { get; set; }
        public bool Checked { get; set; }
        public bool RelWithItem { get; set; }
        public string ADOCNO_AUTONO { get; set; }
        public string HSNSACCD { get; set; }
        public string UOMCD { get; set; }
    }
}