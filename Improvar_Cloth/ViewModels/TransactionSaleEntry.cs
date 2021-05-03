using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Improvar.ViewModels
{
    public class TransactionSaleEntry : Permission
    {
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_TXN T_TXN { get; set; }

        public M_SYSCNFG M_SYSCNFG { get; set; }
        public T_TXN_LINKNO T_TXN_LINKNO { get; set; }
        public T_TXNACK T_TXNACK { get; set; }
        public T_TXNAMT T_TXNAMT { get; set; }
        public T_TXNDTL T_TXNDTL { get; set; }
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_TXNPYMT T_TXNPYMT { get; set; }
        public T_TXNSTATUS T_TXNSTATUS { get; set; }
        //public T_TXNTRANS T_TXNTRANS { get; set; }
        public T_VCH_GST T_VCH_GST { get; set; }
        public T_STKTRNF T_STKTRNF { get; set; }
        public T_TXNEINV T_TXNEINV { get; set; }
        public T_TXNEWB T_TXNEWB { get; set; }
        public T_TXNTRANS T_TXNTRANS { get; set; }
        public List<TTXNAMT> TTXNAMT { get; set; }
        public List<TTXNDTL> TTXNDTL { get; set; }
        public List<TBATCHDTL> TBATCHDTL { get; set; }
        public List<TSALEBARNOPOPUP> TSALEBARNOPOPUP { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<Database_Combo3> Database_Combo3 { get; set; }
        public List<Database_Combo4> Database_Combo4 { get; set; }
        public List<HSN_CODE> HSN_CODE { get; set; }
        public List<BL_TYPE> BL_TYPE { get; set; }
        public List<DropDown_list_StkType> DropDown_list_StkType { get; set; }
        public List<BARGEN_TYPE> BARGEN_TYPE { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
        public List<PENDINGORDER> PENDINGORDER { get; set; }
        public List<DISC_TYPE> DISC_TYPE { get; set; }
        public List<DISC_TYPE> DISC_TYPE1 { get; set; }
        public List<RateHistoryGrid> RateHistoryGrid { get; set; }
        public List<TTXNDTLPOPUP> TTXNDTLPOPUP { get; set; }
        public string M_SLIP_NO { get; set; }
        public string PSLCD { get; set; }
        public string SLNM { get; set; }
        public string PARTYCD { get; set; }
        public string SLAREA { get; set; }
        public string GSTNO { get; set; }
        public string GSTSLNM { get; set; }
        public string CONSLNM { get; set; }
        public string AGSLNM { get; set; }
        public string SAGSLNM { get; set; }
        public double TOTNOS { get; set; }
        public double TOTQNTY { get; set; }
        public double TOTTAX { get; set; }
        public double TOTTAXVAL { get; set; }
        public bool RoundOff { get; set; }
        public string GONM { get; set; }
        public string PRCNM { get; set; }
        public string TransporterName { get; set; }
        public string POS { get; set; }
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
        public string TRANSLNM { get; set; }
        public string BARCODE { get; set; }
        public short? TXNSLNO { get; set; }
        [StringLength(4)]
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        [StringLength(10)]
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        public string ITSTYLE { get; set; }
        public string STYLENO { get; set; }
        public string FABITCD { get; set; }
        public string FABITNM { get; set; }
        [StringLength(1)]
        public string STKTYPE { get; set; }
        [StringLength(15)]
        public string STKNAME { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        [StringLength(4)]
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        [StringLength(4)]
        public string SIZECD { get; set; }
        public string SIZENM { get; set; }
        [StringLength(15)]
        public string SHADE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        public string UOM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISCRATE { get; set; }
        [StringLength(8)]
        public string HSNCODE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? GSTPER { get; set; }
        [StringLength(30)]
        public string PDESIGN { get; set; }
        public double? BALSTOCK { get; set; }
        public double? FLAGMTR { get; set; }
        [StringLength(1)]
        public string DISCTYPE { get; set; }
        [StringLength(30)]
        public string BALENO { get; set; }
        public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }
        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }
        [StringLength(10)]
        public string LOCABIN { get; set; }
        public double? T_NOS { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? T_QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_AMT { get; set; }
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
        public double? T_NET_AMT { get; set; }
        public short SLNO { get; set; }
        public string ALL_GSTPER { get; set; }
        public string PRODGRPGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double B_T_QNTY { get; set; }
        public string MTBARCODE { get; set; }
        public string PRTBARCODE { get; set; }
        public string CLRBARCODE { get; set; }
        public string SZBARCODE { get; set; }
        [StringLength(4)]
        public string BALEYR { get; set; }
        public string LINKDOCNO { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        public string TGONM { get; set; }
        public string SGONM { get; set; }
        public string TLOCNM { get; set; }
        public string SLOCNM { get; set; }
        public string PORTNM { get; set; }
        public List<INVTYPE_list> INVTYPE_list { get; set; }
        public List<EXPCD_list> EXPCD_list { get; set; }
        public List<REV_CHRG> Reverse_Charge { get; set; }
        public string TDSNM { get; set; }
        [StringLength(30)]
        public string OURDESIGN { get; set; }
        public double B_T_NOS { get; set; }
        [StringLength(8)]
        public string GLCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? WPPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RPPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? MRPRATE { get; set; }
        public string TCSAPPL { get; set; }
        public double TDSLIMIT { get; set; }
        [StringLength(1)]
        public string TDSCALCON { get; set; }
        public double AMT { get; set; }
        public string TDSROUNDCAL { get; set; }
        public string ORDDOCNO { get; set; }
        public string ORDDOCDT { get; set; }
        public string ORDAUTONO { get; set; }
        public string ORDSLNO { get; set; }
        public string NEGSTOCK { get; set; }
        public string WPPRICEGEN { get; set; }
        public string RPPRICEGEN { get; set; }
        public bool itemfilter { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_DISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_TDDISCAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_SCMDISCAMT { get; set; }
        public string AGDOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? AGDOCDT { get; set; }
        public List<VECHLTYPE> VECHLTYPE { get; set; }
        public List<TRANSMODE> TRANSMODE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? LISTPRICE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? LISTDISCPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? CUTLENGTH { get; set; }
        public string Last_ITCD { get; set; }
        public string FDT { get; set; }
        public string TDT { get; set; }
        public string R_DOCNO { get; set; }
        public string Last_SLCD { get; set; }
        public string Last_FDT { get; set; }
        public string Last_TDT { get; set; }
        public string Last_R_DOCNO { get; set; }
        public string Last_BARNO { get; set; }
        public string Last_BARCODE { get; set; }
        public string Last_STYLENO { get; set; }
        public bool TCSAUTOCAL { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISPBLAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISPTCSAMT { get; set; }
        public string SHOWMTRLJOBCD { get; set; }
        public string SHOWBLTYPE { get; set; }
        public string MUTSLNM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? FIXEDAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? WPPERMANUAL { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RPPERMANUAL { get; set; }
        [AllowHtml]
        public string PCSTYPEVALUE { get; set; }
        public List<PENDING_ISSUE> PENDING_ISSUE { get; set; }
        [StringLength(30)]
        public string BALENO_HELP { get; set; }
        [StringLength(8)]
        public string EXPGLCD { get; set; }
        [StringLength(8)]
        public string EXPGLNM { get; set; }
        public string JOBNM { get; set; }
        public string JOBEXPGLCD { get; set; }
        public string JOBHSNCODE { get; set; }
        public string SHOWSTKTYPE { get; set; }
        public string SLDISCDESC { get; set; }
        public double? DISCRTINRATE { get; set; }
        public string DISCTYPEINRATE { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public bool MergeBarItem { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_FLAGMTR { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISCONBILL { get; set; }
        public string BLUOMCD { get; set; }
        public double? CONVQTYPUNIT { get; set; }
        public double? BLQNTY { get; set; }
    }
}