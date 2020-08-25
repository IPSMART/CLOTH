using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Improvar.ViewModels
{
    public class TransactionSalesPurchaseEntry : Permission
    {
        public List<User> User { get; set; }
        public string DOC_ID { get; set; }
        public string DOC_CD { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<TRANSMODE> TRANSMODE { get; set; }
        public List<VECHLTYPE> VECHLTYPE { get; set; }
        public List<INSURANCE> INSURANCE { get; set; }
        public M_ITEMPLIST M_ITEMPLIST { get; set; }
        public List<TTXNDTL_ORDERLIST> TTXNDTL_ORDERLIST { get; set; }
        public List<TTXNDTL_PENDING> TTXNDTL_PENDING { get; set; }
        public List<TTXNPRODDTL> TTXNPRODDTL { get; set; }
        public T_MIN_HDR T_MIN_HDR { get; set; }
        public T_VCH_GST T_VCH_GST { get; set; }
        public T_MIN_DTL T_MIN_DTL { get; set; }
        public M_TAXGRP M_TAXGRP { get; set; }
        public M_GODOWN M_GODOWN { get; set; }
        public M_SUBLEG M_SUBLEG { get; set; }
        public MS_CURRENCY MS_CURRENCY { get; set; }
        public M_GROUP M_GROUP { get; set; }
        public T_TXN T_TXN { get; set; }
        public string PREV_SLCD { get; set; }
        public T_TXNDTL T_TXNDTL { get; set; }
        public T_TXNAMT T_TXNAMT { get; set; }
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_STKTRNF T_STKTRNF { get; set; }
        public string SLOCNM { get; set; }
        public string TLOCNM { get; set; }
        public string SGONM { get; set; }
        public string TGONM { get; set; }
        public T_TXNTRANS T_TXNTRANS { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_CNTRL_HDR_DOC T_CNTRL_HDR_DOC { get; set; }
        public T_CNTRL_HDR_DOC_DTL T_CNTRL_HDR_DOC_DTL { get; set; }
        public List<TTXNAMT> TTXNAMT { get; set; }
        public List<TTXNDTL> TTXNDTL { get; set; }
        public List<TBATCHDTL> TBATCHDTL { get; set; }
        public bool Checked { get; set; }
        public bool RoundOff { get; set; }
        public string BuyerName { get; set; }
        public string BuyerGSTNO { get; set; }
        public string BuyerLegBal { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsigneeGSTNO { get; set; }
        public double P_QNTYNOS { get; set; }
        public double P_QNTY { get; set; }
        public double BILL_AMT { get; set; }
        public string AgentName { get; set; }
        public string SalemenName { get; set; }
        public string TransporterName { get; set; }
        public string WEEKNOMUST { get; set; }
        public string RATECHANGE { get; set; }
        public string QNTYCHNG { get; set; }
        public string BATCHWISESTK { get; set; }
        public string PBILLWISESTK { get; set; }
        public string OPENPRICE { get; set; }
        public string RATEQNTYBAG { get; set; }
        public string STKQNTYBAG { get; set; }
        public double CRDAYS { get; set; }
        public double CRLIMIT { get; set; }
        public string DIST_CRCD { get; set; }
        public short SERIAL { get; set; }
        public long? T_NOS { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
        public double? T_QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_QNTYDAM { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_CURR_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_STD_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_DIS_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_GROSS_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_IGST_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_SGST_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_CGST_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_CESS_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_DUTY_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_OTHER_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_NET_AMT { get; set; }
        public string T_NET_AMT_DISP { get; set; }

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

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TOTAL_TXBL { get; set; }
        public double? TOTAL_TAX { get; set; }
        public string PSLCD { get; set; }
        public string GLCD { get; set; }
        public string CLASS1CD { get; set; }
        public string LAST_ITMPRCCD { get; set; }
        public double? PBLAMT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? FROMDT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? TODT { get; set; }
        public string PLISTADDLESSASK { get; set; }
        public string PLISTITEMWISE { get; set; }
        public string MAKECDASK { get; set; }
        public string MSGDSP { get; set; }
        [StringLength(6)]
        public string PORTNM { get; set; }
        [StringLength(15)]
        public string LRNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? LRDT { get; set; }
        public double? GRWT { get; set; }
        [StringLength(20)]
        public string EWAYBILLNO { get; set; }
        public List<DropDown_list1> Reverse_Charge { get; set; }
        public int? M_SLIP_NO { get; set; }
        public string DOCDISPNO { get; set; }
        public string INCLRATEASK { get; set; }
        [StringLength(2)]
        public string POS { get; set; }
        [StringLength(1)]
        public string GTSREGNTYPE { get; set; }
        [StringLength(50)]
        public string GSTSLNM { get; set; }
        [StringLength(15)]
        public string GSTNO { get; set; }
        public string vchrauthremarks { get; set; }
        public string smsSendInfo { get; set; }
        public List<INVTYPE_list> INVTYPE_list { get; set; }
        public List<EXPCD_list> EXPCD_list { get; set; }
        public List<TTXNBLTAG> TTXNBLTAG { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? BILL_AMTRecvd { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? NET_AMTRecvd { get; set; }
    }
}