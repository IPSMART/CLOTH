using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TransactionSalePosEntry :Permission
    {
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_TXN T_TXN { get; set; }
        public T_TXN_LINKNO T_TXN_LINKNO { get; set; }
        public T_TXNACK T_TXNACK { get; set; }
        public T_TXNAMT T_TXNAMT { get; set; }
        public T_TXNDTL T_TXNDTL { get; set; }
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_TXNPYMT T_TXNPYMT { get; set; }
        public T_TXNSTATUS T_TXNSTATUS { get; set; }
        public T_TXNTRANS T_TXNTRANS { get; set; }
        public T_VCH_GST T_VCH_GST { get; set; }
        public T_STKTRNF T_STKTRNF { get; set; }
        public List<TTXNAMT> TTXNAMT { get; set; }
        public List<TTXNDTL> TTXNDTL { get; set; }
        public List<TsalePos_TBATCHDTL> TsalePos_TBATCHDTL { get; set; }
        public List<TSALEBARNOPOPUP> TSALEBARNOPOPUP { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<Database_Combo3> Database_Combo3 { get; set; }
        public List<HSN_CODE> HSN_CODE { get; set; }
        public List<BL_TYPE> BL_TYPE { get; set; }
        public List<DropDown_list_StkType> DropDown_list_StkType { get; set; }
        public List<BARGEN_TYPE> BARGEN_TYPE { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
       
        public string RTDEBCD { get; set; }
        public string RTDEBNM { get; set; }
        public string MOBILE { get; set; }
        public string NM { get; set; }
        public string MOBNO { get; set; }
        public bool INC_RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? NETDUE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? MEMOAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RETAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? PAYABLE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? PAYAMT { get; set; }
        public string ADDR { get; set; }
        public string PSLCD { get; set; }
        public string SLNM { get; set; }
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
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_PYMT_AMT { get; set; }
        public string TRANSLNM { get; set; }
        public string CRSLNM { get; set; }
        public string BARCODE { get; set; }
        public short? TXNSLNO { get; set; }
        [StringLength(4)]
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        [StringLength(8)]
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
        public double? WPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? MRPRATE { get; set; }
        public string TCSAPPL { get; set; }
        public double TDSLIMIT { get; set; }
        [StringLength(1)]
        public string TDSCALCON { get; set; }
        public double AMT { get; set; }
        public string TDSROUNDCAL { get; set; }
        public string INCLRATEASK { get; set; }
        public List<TTXNSLSMN> TTXNSLSMN { get; set; }
        public List<TTXNPYMT> TTXNPYMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_PER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_ITAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_BLAMT { get; set; }
        public string GOCD { get; set; }
        

    }
}