using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace Improvar.ViewModels
{
    public class TranAltBill : Permission
    {
        
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_TXNMEMO T_TXNMEMO { get; set; }
        public T_TXN T_TXN { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public string SLNM { get; set; }
        public string REGMOBILE { get; set; }
        public string STRTNO { get; set; }
        public string GONM { get; set; }
        public string TRANSLNM { get; set; }
        public List<VECHLTYPE> VECHLTYPE { get; set; }
        public List<TRANSMODE> TRANSMODE { get; set; }
        public string SLNM1 { get; set; }
        public string BALENO { get; set; }
        public string GONM1 { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
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
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? OTHAMT { get; set; }
        public double TOTTAX { get; set; }
        public double GSTPER { get; set; }
        public double IGSTPER { get; set; }

        public double TAXABVAL { get; set; }
        public bool RoundOff { get; set; }
        public List<TTXNAMT> TTXNAMT { get; set; }
        public string ADDR { get; set; }
        public string PSLCD { get; set; }
        public string EXPGLNM { get; set; }
        public string JOBNM { get; set; }
        public string UOM { get; set; }
        public string JOBEXPGLCD { get; set; }
        public string JOBHSNCODE { get; set; }
        public string SHOWSTKTYPE { get; set; }
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
        public string HSNCODE { get; set; }
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
        public List<TTXNPYMT> TTXNPYMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_PER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_ITAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_BLAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double B_T_NET { get; set; }
        public string RETDEBSLCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double R_T_QNTY { get; set; }
        public double R_T_NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double R_T_NET { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_R_GROSSAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_GSTAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_CESS_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_NET_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_SGST_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_CGST_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? R_T_IGST_AMT { get; set; }
        public string FDT { get; set; }
        public string TDT { get; set; }
        public string R_DOCNO { get; set; }
        public string EFFDT { get; set; }

        public List<DISC_TYPE> DISC_TYPE { get; set; }
        public List<PCSection> PCSActionList { get; set; }
        public List<DropDown_list2> TDDISC_TYPE { get; set; }
        public List<DropDown_list3> SCMDISC_TYPE { get; set; }
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
    }
}