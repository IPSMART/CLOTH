﻿using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System;

namespace Improvar.ViewModels
{
    public class RepBarHistory : Permission
    {
        public string ActionName { get; set; }
        public string CaptionName { get; set; }
        public string OtherPara { get; set; }
        public string RepType { get; set; }
        public string TEXTBOX1 { get; set; }
        public string TEXTBOX2 { get; set; }
        public bool Checkbox1 { get; set; }
        public bool Checkbox2 { get; set; }
        public string DOCNO { get; set; }
        public string FDOCNO { get; set; }
        public string TDOCNO { get; set; }
        public string DOCCD { get; set; }
        public bool AskSlCd { get; set; }
        public string FDT { get; set; }
        public string TDT { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public string DOCNM { get; set; }
        public string TEXTBOX6 { get; set; }
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        public string ITGRPCD { get; set; }
        public string BAR_CODE { get; set; }
        public string ITGRPNM { get; set; }
        public string BARCODE { get; set; }
        public string FABITCD { get; set; }
        public string FABITNM { get; set; }
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        public string DESIGN { get; set; }
        public string UOMCD { get; set; }
        public string DISTRICT { get; set; }
        public string GSTNO { get; set; }
        public string PDESIGN { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TOTALIN { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double TOTALOUT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double TOTINOUT { get; set; }
        public string SIZECD { get; set; }
        public string SIZENM { get; set; }
        public List<BARCODEHISTORY> BARCODEHISTORY { get; set; }
        public List<BARCODEPRICE> BARCODEPRICE { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? T_INQNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? T_OUTQNTY { get; set; }       
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? T_NOS { get; set; }
        public List<UploadDOC> UploadBarImages { get; set; }
        public string BarImages { get; set; }
        public string MTRLJOBCD { get; set; }
        public string PARTCD { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        public string GOCD { get; set; }
        public string PRCCD { get; set; }
        public string TAXGRPCD { get; set; }
        public string SHOWMTRLJOBCD { get; set; }
        public NEWBARDATA NEWBARDATA { get; set; }
        public string COMMONUNIQBAR { get; set; }
        public NEWBARDATA NEWPRICEDATA { get; set; }
        public string STYLENO { get; set; }
        public string listprccd { get; set; }
        public string T_UomTotalIn { get; set; }
        public string T_UomTotalOut { get; set; }
        //public string mtrljobcd  { get; set; }
        public string mtrljobnm { get; set; }

        public List<DropDown_list1> DropDown_list1 { get; set; }
        public bool MergeLoc { get; set; }


    }
    public class NEWBARDATA
    {
        public string BARNO { get; set; }
        public DateTime? EFFDT { get; set; }
        public double? CPRATE { get; set; }
        public double? WPRATE { get; set; }
        public double? RPRATE { get; set; }
        [StringLength(10)]
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }
        [StringLength(30)]
        public string PDESIGN { get; set; }       

    }
}