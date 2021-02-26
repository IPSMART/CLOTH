using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class SalePymtEntry : Permission
    {
        public List<TTXNSLSMN> TTXNSLSMN { get; set; }
        public T_TXNPYMT_HDR T_TXNPYMT_HDR { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<TTXNPYMT> TTXNPYMT { get; set; }
        public List<SLPYMTADJ> SLPYMTADJ { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public string SLNM { get; set; }
        public string REGMOBILE { get; set; }
        public string STRTNO { get; set; }
        public string GONM { get; set; }
        public string TRANSLNM { get; set; }
        public List<VECHLTYPE> VECHLTYPE { get; set; }
        public List<TRANSMODE> TRANSMODE { get; set; }
        [StringLength(60)]
        public string NM { get; set; }

        [StringLength(12)]
        public string MOBILE { get; set; }

        [StringLength(30)]
        public string CITY { get; set; }

        [StringLength(200)]
        public string ADDR { get; set; }
        public string RTDEBNM { get; set; }
        public string MOBNO { get; set; }
        public bool INC_RATE { get; set; }
        public string RETDEBSLCD { get; set; }
        public double TOT_AMT { get; set; }
        public double TOT_BAL { get; set; }
        public double TOT_ADJ { get; set; }
        public double TOT_PRE_ADJ { get; set; }
        public double T_PYMT_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_PER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_ITAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? T_BLAMT { get; set; }
        public double? CUROSAMT { get; set; }
        public double? AVLBALFORADJ { get; set; }
    }
}