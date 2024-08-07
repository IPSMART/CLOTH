﻿using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class StockAdjustmentsConversionEntry : Permission
    {
        public T_TXN T_TXN { get; set; }
        public T_TXNOTH T_TXNOTH { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        //public List<TTXNDTL> TTXNDTL { get; set; }
        //public List<TTXNDTL_OUT> TTXNDTL_OUT { get; set; }
        public List<TBATCHDTL> TBATCHDTL { get; set; }
        public List<TBATCHDTL> TBATCHDTL_OUT { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<STOCK_ADJUSTMENT> STOCK_ADJUSTMENT { get; set; }
        public string GodownName { get; set; }
        public double IN_T_QNTY { get; set; }
        public double TOTAL_IN_QNTY { get; set; }
        public double OUT_T_QNTY { get; set; }
        public double TOTAL_OUT_QNTY { get; set; }
        public string DOC_ID { get; set; }
        public double TOTAL_QNTY { get; set; }
        public string MTRLJOBNM { get; set; }
        public int SERIAL { get; set; }
        public string CUTRECDOCNO { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CUTRECDOCDT { get; set; }
        public double SIZE_T_QNTY { get; set; }
        public M_SYSCNFG M_SYSCNFG { get; set; }
        public List<BARGEN_TYPE> BARGEN_TYPE { get; set; }
        public List<DropDown_list_StkType> DropDown_list_StkType { get; set; }
        public List<TMOBDTL> TMOBDTL { get; set; }

    }
}