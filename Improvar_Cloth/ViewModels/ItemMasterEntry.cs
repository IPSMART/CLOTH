using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class ItemMasterEntry : Permission
    {
        public M_SITEM M_SITEM { get; set; }
        public M_GROUP M_GROUP { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public M_COLLECTION M_COLLECTION { get; set; }
        public M_SUBBRAND M_SUBBRAND { get; set; }
        public M_UOM M_UOM { get; set; }
        public M_PRODGRP M_PRODGRP { get; set; }
        public List<Gender> Gender { get; set; }
        public List<ProductType> ProductType { get; set; }
        public List<MSITEMPARTS> MSITEMPARTS { get; set; }
        public List<MSITEMBARCODE> MSITEMBARCODE { get; set; }
        public List<MSITEMSLCD> MSITEMSLCD { get; set; }
        public List<MSITEMMEASURE> MSITEMMEASURE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public bool Checked { get; set; }
        public string FABITNM { get; set; }
        public string FABUOMNM { get; set; }        
        public string PRICES_EFFDT { get; set; }
        public string FABSTYLENO { get; set; }
        public DataTable DTPRICES { get; set; }
        public string STRPRICES { get; set; }
        public string SEARCH_ITCD { get; set; }
        //public string ITEM_BARCODE { get; set; }
        public string PRICES_EFFDTDROP { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<UploadDOC> UploadBarImages { get; set; }
        public string BarImages { get; set; }
        public bool NEGSTOCK { get; set; }
        public bool HASTRANSACTION { get; set; }
        public string CONVUOMNM { get; set; }
    }
}