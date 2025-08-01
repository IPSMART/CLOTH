﻿using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace Improvar.ViewModels
{
    public class TransactionOutRecProcess : Permission
    {//kk
        public T_TXN T_TXN { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public T_TXN_LINKNO T_TXN_LINKNO { get; set; }
        public T_TXNACK T_TXNACK { get; set; }
        public T_TXNAMT T_TXNAMT { get; set; }
        public T_TXNDTL T_TXNDTL { get; set; }
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_TXNPYMT T_TXNPYMT { get; set; }
        public T_TXNSTATUS T_TXNSTATUS { get; set; }
        public T_TXNTRANS T_TXNTRANS { get; set; }
        public List<TTXNAMT> TTXNAMT { get; set; }        
        public List<TTXNDTL> TTXNDTL { get; set; }
        public List<TBATCHDTL> TBATCHDTL { get; set; }
        public List<TPROGDTL> TPROGDTL { get; set; }
        public List<TPROGBOM> TPROGBOM { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<HSN_CODE> HSN_CODE { get; set; }
        public List<DropDown_list_BLTYPE> BL_TYPE { get; set; }
        public List<DropDown_list_StkType> DropDown_list_StkType { get; set; }
        public List<BARGEN_TYPE> BARGEN_TYPE { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
        public List<DISC_TYPE> DISC_TYPE { get; set; }
        public List<TDDISC_TYPE> TDDISC_TYPE { get; set; }
        public List<SCMDISC_TYPE> SCMDISC_TYPE { get; set; }
        public List<RateHistoryGrid> RateHistoryGrid { get; set; }

        public string SLNM { get; set; }
        public string SLCD { get; set; }
        public string GSTNO { get; set; }
        public string DISTRICT { get; set; }
        public string JOBCD { get; set; }
        public string JOBNM { get; set; }
        public string SAGSLNM { get; set; }
        public double TOTNOS { get; set; }
        public double TOTQNTY { get; set; }
        public double TOTTAX { get; set; }
        public double TOTTAXVAL { get; set; }
        public bool RoundOff { get; set; }
        public string GONM { get; set; }
        public string PRCNM { get; set; }

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
        [StringLength(4)]
        public string TAXGRPCD { get; set; }
        public string PSTYLENO { get; set; }
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
        public double T_NOS { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
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
        [DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
        public double T_BOMQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
        public double T_EXTRAQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000000}", ApplyFormatInEditMode = true)]
        public double T_QQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double B_T_QNTY { get; set; }
        public double B_T_NOS { get; set; }
        public short SLNO { get; set; }
        public string MTBARCODE { get; set; }
        public string ITSTYLE { get; set; }
        public string PRTBARCODE { get; set; }
        public string CLRBARCODE { get; set; }
        public string SZBARCODE { get; set; }
        [StringLength(30)]
        public string PDESIGN { get; set; }
        public string ALL_GSTPER { get; set; }
        public string PRODGRPGSTPER { get; set; }
        [StringLength(30)]
        public string OURDESIGN { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        [StringLength(4)]
        public string BALEYR { get; set; }
        [StringLength(8)]
        public string GLCD { get; set; }
        public List<TSALEBARNOPOPUP> TSALEBARNOPOPUP { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? P_T_NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? P_T_QNTY { get; set; }
        public List<REV_CHRG> Reverse_Charge { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? NETAMT { get; set; }
        public string TDSNM { get; set; }
        public string TCSAPPL { get; set; }
        public double TDSLIMIT { get; set; }
        [StringLength(1)]
        public string TDSCALCON { get; set; }
        public double AMT { get; set; }
        public string TDSROUNDCAL { get; set; }
        public T_VCH_GST T_VCH_GST { get; set; }
        public string GSTSLNM { get; set; }
        public string POS { get; set; }
        public string PROGAUTOSLNO { get; set; }
        public string PROGUNIQNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double B_T_SHORTQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? P_T_SHORTQNTY { get; set; }
        public double? M_SLIP_NO { get; set; }
        public string Prog_UomTotal { get; set; }
        public string Rec_UomTotal { get; set; }
        public string Det_UomTotal { get; set; }
        public List<PENDING_POPUP> PENDING_POPUP { get; set; }
        public List<TBILTYR> TBILTYR { get; set; }
        public List<DropDown_list2> RETURN_TYPE { get; set; }


    }
}