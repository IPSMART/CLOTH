using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Improvar.Models
{
    public class Prepare_JSON
    {
        public string AUTONO { get; set; }
        public string Supply_Type { get; set; }
        public string SubSupply_Type { get; set; }
        public string Doctype { get; set; }
        public string blno { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime bldt { get; set; }
        public string Transaction_Type { get; set; }


        [StringLength(100)]
        public string compnm { get; set; }
        public string frmgstno { get; set; }

        [StringLength(100)]
        public string frmadd1 { get; set; }

        [StringLength(100)]
        public string frmadd2 { get; set; }
        public string frmdistrict { get; set; }
        public string frmpin { get; set; }
        public string frmstatecd { get; set; }
        public string disptchfrmstatecd { get; set; }
        public string disptchfrmstnm { get; set; }
        public string slnm { get; set; }
        public string togstno { get; set; }
        public string toadd1 { get; set; }
        [StringLength(100)]
        public string toadd2 { get; set; }
        [StringLength(30)]
        public string todistrict { get; set; }
        public string shiptopin { get; set; }
        public string billtostcd { get; set; }
        public string shiptostcd { get; set; }
        public string itnm { get; set; }
        public string itdscp { get; set; }
        public string hsncode { get; set; }
        public string guomcd { get; set; }
        public string guomnmw { get; set; }
        public double qnty { get; set; }
        public double amt { get; set; }
        public string TtlTax { get; set; }
        public double cgstamt { get; set; }
        public double cgstper { get; set; }
        public double sgstper { get; set; }
        public double igstper { get; set; }
        public double cessper { get; set; }
        public double sgstamt { get; set; }
        public double igstamt { get; set; }
        public double cessamt { get; set; }
        public double cess_non_advol { get; set; }
        public double othramt { get; set; }
        public double invamt { get; set; }
        public string transMode { get; set; }
        public int distance { get; set; }
        public string trslnm { get; set; }
        public string trgst { get; set; }
        public string lrno { get; set; }
        public string lrdt { get; set; }
        public string Vehicle_No { get; set; }
        public string Vehile_Type { get; set; }
        public bool Checked { get; set; }
        public short SLNO { get; set; }
        public string message { get; set; }
    }
    public class EWB_SUPPLY_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_SUB_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_DOC_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_TRANSACTION_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_BlFrmSt_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_DisFrmSt_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_BlTo_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_ShipTo_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_Vehicle_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class EWB_Unit_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class EWB_Transmode_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class AdqrEWBItemList
    {
        public string productName { get; set; }
        public string productDesc { get; set; }
        public int hsnCode { get; set; }
        public double quantity { get; set; }
        public string qtyUnit { get; set; }
        public double cgstRate { get; set; }
        public double sgstRate { get; set; }
        public double igstRate { get; set; }
        public double cessRate { get; set; }
        public double cessNonAdvol { get; set; }
        public double taxableAmount { get; set; }
    }

    public class AdqrGenEWAYBILL
    {
        public string supplyType { get; set; }
        public string subSupplyType { get; set; }
        public string subSupplyDesc { get; set; }
        public string docType { get; set; }
        public string docNo { get; set; }
        public string docDate { get; set; }
        public string fromGstin { get; set; }
        public string fromTrdName { get; set; }
        public string fromAddr1 { get; set; }
        public string fromAddr2 { get; set; }
        public string fromPlace { get; set; }
        public int fromPincode { get; set; }
        public int actFromStateCode { get; set; }
        public int fromStateCode { get; set; }
        public string toGstin { get; set; }
        public string toTrdName { get; set; }
        public string toAddr1 { get; set; }
        public string toAddr2 { get; set; }
        public string toPlace { get; set; }
        public int toPincode { get; set; }
        public int actToStateCode { get; set; }
        public int toStateCode { get; set; }
        public int transactionType { get; set; }
        public string dispatchFromGSTIN { get; set; }
        public string dispatchFromTradeName { get; set; }
        public string shipToGSTIN { get; set; }
        public string shipToTradeName { get; set; }
        public double otherValue { get; set; }
        public double totalValue { get; set; }
        public double cgstValue { get; set; }
        public double sgstValue { get; set; }
        public double igstValue { get; set; }
        public double cessValue { get; set; }
        public double cessNonAdvolValue { get; set; }
        public double totInvValue { get; set; }
        public string transporterId { get; set; }
        public string transporterName { get; set; }
        public string transDocNo { get; set; }
        public string transMode { get; set; }
        public string transDistance { get; set; }
        public string transDocDate { get; set; }
        public string vehicleNo { get; set; }
        public string vehicleType { get; set; }
        public List<AdqrEWBItemList> itemList { get; set; }
    }

    public class AdqrRsltGENEWAYBILL
    {
        public string ewayBillDate { get; set; }
        public long ewayBillNo { get; set; }
        public string alert { get; set; }
        public string validUpto { get; set; }
    }

    public class AdqrRespGENEWAYBILL
    {
        public bool success { get; set; }
        public AdqrRsltGENEWAYBILL result { get; set; }
        public string message { get; set; }
    }
}