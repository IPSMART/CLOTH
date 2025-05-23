﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class AdaequareIRN
    {
        public string Version { get { return "1.1"; } }
        public TranDtls TranDtls { get; set; }
        public DocDtls DocDtls { get; set; }
        public SellerDtls SellerDtls { get; set; }
        public BuyerDtls BuyerDtls { get; set; }
        public DispDtls DispDtls { get; set; }
        public ShipDtls ShipDtls { get; set; }
        public List<ItemList> ItemList { get; set; }
        public ValDtls ValDtls { get; set; }
        public PayDtls PayDtls { get; set; }
        public RefDtls RefDtls { get; set; }
        public List<AddlDocDtl> AddlDocDtls { get; set; }
        public ExpDtls ExpDtls { get; set; }
        public EwbDtls EwbDtls { get; set; }
    }

    public class TranDtls
    {
        public string TaxSch { get { return "GST"; } }
        public string SupTyp { get; set; }
        public string RegRev { get; set; }
        public object EcmGstin { get; set; }
        public string IgstOndoublera { get { return "N"; } }
    }

    public class DocDtls
    {
        public string Typ { get; set; }
        public string No { get; set; }
        public string Dt { get; set; }
    }

    public class SellerDtls
    {
        public string Gstin { get; set; }
        public string LglNm { get; set; }
        public string TrdNm { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Loc { get; set; }
        public int Pin { get; set; }
        public string Stcd { get; set; }
        public string Ph { get; set; }
        public string Em { get; set; }
    }

    public class BuyerDtls
    {
        public string Gstin { get; set; }
        public string LglNm { get; set; }
        public string TrdNm { get; set; }
        public string Pos { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Loc { get; set; }
        public int Pin { get; set; }
        public string Stcd { get; set; }
        public string Ph { get; set; }
        public string Em { get; set; }
    }

    public class DispDtls
    {
        public string Nm { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Loc { get; set; }
        public int Pin { get; set; }
        public string Stcd { get; set; }
    }

    public class ShipDtls
    {
        public string Gstin { get; set; }
        public string LglNm { get; set; }
        public string TrdNm { get; set; }
        public string Addr1 { get; set; }
        public string Addr2 { get; set; }
        public string Loc { get; set; }
        public int Pin { get; set; }
        public string Stcd { get; set; }
    }

    public class BchDtls
    {
        public string Nm { get; set; }
        public string Expdt { get; set; }
        public string wrDt { get; set; }
    }

    public class AttribDtl
    {
        public string Nm { get; set; }
        public string Val { get; set; }
    }

    public class ItemList
    {
        public string SlNo { get; set; }
        [Required]
        public string PrdDesc { get; set; }
        public string IsServc { get; set; }
        [Required]
        public string HsnCd { get; set; }
        public string Barcde { get; set; }
        public double Qty { get; set; }
        public double FreeQty { get; set; }
        public string Unit { get; set; }
        public double UnitPrice { get; set; }
        public double TotAmt { get; set; }
        public double Discount { get; set; }
        public double PreTaxVal { get; set; }
        public double AssAmt { get; set; }
        public double GstRt { get; set; }
        public double IgstAmt { get; set; }
        public double CgstAmt { get; set; }
        public double SgstAmt { get; set; }
        public double CesRt { get; set; }
        public double CesAmt { get; set; }
        public double CesNonAdvlAmt { get; set; }
        public double StateCesRt { get; set; }
        public double StateCesAmt { get; set; }
        public double StateCesNonAdvlAmt { get; set; }
        public double OthChrg { get; set; }
        public double TotItemVal { get; set; }
        public string OrdLineRef { get; set; }
        public string OrgCntry { get; set; }
        public string PrdSlNo { get; set; }
        public BchDtls BchDtls { get; set; }
        public List<AttribDtl> AttribDtls { get; set; }
    }

    public class ValDtls
    {
        public double AssVal { get; set; }
        public double CgstVal { get; set; }
        public double SgstVal { get; set; }
        public double IgstVal { get; set; }
        public double CesVal { get; set; }
        public double StCesVal { get; set; }
        public double Discount { get; set; }
        public double OthChrg { get; set; }
        public double RndOffAmt { get; set; }
        public double TotInvVal { get; set; }
        public double TotInvValFc { get; set; }
    }

    public class PayDtls
    {
        public string Nm { get; set; }
        public string Accdet { get; set; }
        public string Mode { get; set; }
        public string Fininsbr { get; set; }
        public string Payterm { get; set; }
        public string Payinstr { get; set; }
        public string Crtrn { get; set; }
        public string Dirdr { get; set; }
        public int Crday { get; set; }
        public double Paidamt { get; set; }
        public double Paymtdue { get; set; }
    }

    public class DocPerDDtls
    {
        public string InvStDt { get; set; }
        public string InvEndDt { get; set; }
    }

    public class PrecDocDtl
    {
        public string InvNo { get; set; }
        public string InvDt { get; set; }
        public string OthRefNo { get; set; }
    }

    public class COntrDtl
    {
        public string RecAdvRefr { get; set; }
        public string RecAdvDt { get; set; }
        public string Tendrefr { get; set; }
        public string Contrrefr { get; set; }
        public string Extrefr { get; set; }
        public string Projrefr { get; set; }
        public string Porefr { get; set; }
        public string PoRefDt { get; set; }
    }

    public class RefDtls
    {
        public string InvRm { get; set; }
        public DocPerDDtls DocPerdDtls { get; set; }
        public List<PrecDocDtl> PrecDocDtls { get; set; }
        public List<COntrDtl> ContrDtls { get; set; }
    }

    public class AddlDocDtl
    {
        public string Url { get; set; }
        public string Docs { get; set; }
        public string Info { get; set; }
    }

    public class ExpDtls
    {
        public string ShipBNo { get; set; }
        public string ShipBDt { get; set; }
        public string Port { get; set; }
        public string RefClm { get; set; }
        public string ForCur { get; set; }
        public string CntCode { get; set; }
        public object ExpDuty { get; set; }
    }
   public class EwbDtls
    {
        public string Transid { get; set; }
        public string Transname { get; set; }
        public int Distance { get; set; }
        public string Transdocno { get; set; }
        public string TransdocDt { get; set; }
        public string Vehno { get; set; }
        public string Vehtype { get; set; }
        public string TransMode { get; set; }
    }
    public class AdqrRsltGenIRN
    {
        public long AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public string Status { get; set; }
        public object EwbNo { get; set; }
        public object EwbDt { get; set; }
        public object EwbValidTill { get; set; }
    }
    public class AdqrDescGenIRN
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AdqrInfoGenIRN
    {
        public string InfCd { get; set; }
        public List<AdqrDescGenIRN> Desc { get; set; }
    }
    public class AdqrRespGenIRN
    {
        public bool success { get; set; }
        public string message { get; set; }
        public AdqrRsltGenIRN result { get; set; }
        public List<AdqrInfoGenIRN> info { get; set; }
    }
    public class AdqrRsltGenIRNDesc
    {
        public long AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
    }

    public class AdqrRsltGenIRNfail
    {
        public string InfCd { get; set; }
        public AdqrRsltGenIRNDesc Desc { get; set; }
    }
    public class AdqrRespGenIRNfail
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<AdqrRsltGenIRNfail> result { get; set; }
    }

    public class AdaequareIRNCancel
    {
        public string irn { get; set; }
        public string cnlrsn { get; set; }
        public string cnlrem { get; set; }
    }
    public class AdqrRsltGenIRNCancel
    {
        public string Irn { get; set; }
        public string CancelDate { get; set; }
    }

    public class AdqrRespGenIRNCancel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public AdqrRsltGenIRNCancel result { get; set; }
    }
    public class AdqrRespExtractInvoice
    {
        public bool success { get; set; }
        public string message { get; set; }
        public AdqrRsltExtractInvoice result { get; set; }
    }
    public class AdqrRsltExtractInvoice
    {
        public string Irn { get; set; }
        public string SellerGstin { get; set; }
        public string DocTyp { get; set; }
        public double TotInvVal { get; set; }
        public string BuyerGstin { get; set; }
        public string DocDt { get; set; }
        public string DocNo { get; set; }
        public string MainHsnCode { get; set; }
        public int ItemCnt { get; set; }
        public string IrnDt { get; set; }
    }
    public class AdaequareIRNEWB
    {
        public string Irn { get; set; }
        public string TransId { get; set; }
        public string TransMode { get; set; }
        public string TrnDocNo { get; set; }
        public string TrnDocDt { get; set; }
        public string VehNo { get; set; }
        public int Distance { get; set; }
        public string VehType { get; set; }
        public string TransName { get; set; }
    }

    public class AdqrRespIRNEWB
    {
        public bool success { get; set; }
        public string message { get; set; }
        public AdqrRsltIRNEWB result { get; set; }
    }

    public class AdqrRsltIRNEWB
    {
        public long EwbNo { get; set; }
        public string Status { get; set; }
        public string GenGstin { get; set; }
        public string EwbDt { get; set; }
        public string EwbValidTill { get; set; }
    }
    public class AdqrRsltInvoiceByIRN
    {
        public long AckNo { get; set; }
        public string AckDt { get; set; }
        public string Irn { get; set; }
        public string SignedInvoice { get; set; }
        public string SignedQRCode { get; set; }
        public string Status { get; set; }
        public long? EwbNo { get; set; }
        public string EwbDt { get; set; }
        public string EwbValidTill { get; set; }
        public string Remarks { get; set; }
    }

    public class AdqrRespInvoiceByIRN
    {
        public bool success { get; set; }
        public string message { get; set; }
        public AdqrRsltInvoiceByIRN result { get; set; }
    }
}