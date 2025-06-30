using System.Collections.Generic;
using Improvar.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class ReportViewinHtml : Permission
    {
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }
        public List<DropDown_list4> DropDown_list4 { get; set; }
        public List<DropDown_list_text> DropDown_list_text { get; set; }
        public List<DropDown_list_LOCCD> DropDown_list_LOCCD { get; set; }
        public bool Checkbox1 { get; set; }
        public bool Checkbox2 { get; set; }
        public bool Checkbox3 { get; set; }
        public bool Checkbox4 { get; set; }
        public bool Checkbox5 { get; set; }
        public bool Checkbox6 { get; set; }
        public bool Checkbox7 { get; set; }
        public bool Checkbox8 { get; set; }
        public bool Checkbox9 { get; set; }
        public bool Checkbox10 { get; set; }
        public bool Checkbox11 { get; set; }
        public bool Checkbox12 { get; set; }
        public bool Checkbox13 { get; set; }
        public bool Checkbox14 { get; set; }
        public bool Checkbox15 { get; set; }
        public bool Checkbox16 { get; set; }
        public bool Checkbox17 { get; set; }
        public bool Checkbox18 { get; set; }
        public bool Checkbox19 { get; set; }
        public string DOCNO { get; set; }
        public string FDOCNO { get; set; }
        public string TDOCNO { get; set; }
        public string DOCCD { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT { get; set; }
        public string TEXTBOX1 { get; set; }
        public string TEXTBOX2 { get; set; }
        public string TEXTBOX3 { get; set; }
        public string TEXTBOX4 { get; set; }
        public string TEXTBOX5 { get; set; }
        public string TEXTBOX6 { get; set; }
        public string TEXTBOX7 { get; set; }
        public string TEXTBOX8 { get; set; }
        public string TEXTBOX9 { get; set; }
        public string TEXTBOX10 { get; set; }
        public string Slnm { get; set; }
        public string Compnm { get; set; }
        public string Glnm { get; set; }
        public string Docnm { get; set; }
        public string Itgrpnm { get; set; }
        public string SelAutono { get; set; }
        public string CLASS1CD { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<DropDown_list_GLCD> DropDown_list_GLCD { get; set; }
        public List<DropDown_list_SLCD> DropDown_list_SLCD { get; set; }
        public List<DropDown_list_BRAND> DropDown_list_BRAND { get; set; }
        public List<DropDown_list_AGSLCD> DropDown_list_AGSLCD { get; set; }
        public List<DropDown_list_ITGRP> DropDown_list_ITGRP { get; set; }
        public List<DropDown_list_ITEM> DropDown_list_ITEM { get; set; }
        public List<DropDown_list_GODOWN> DropDown_list_GODOWN { get; set; }
        public List<DropDown_list_LINECD> DropDown_list_LINECD { get; set; }
        public List<DropDown_list_SLMSLCD> DropDown_list_SLMSLCD { get; set; }
        public List<DropDown_list_TXN> DropDown_list_TXN { get; set; }
        public List<DropDown_list_Class1> DropDown_list_Class1 { get; set; }
        public List<DropDown_list_COMP> DropDown_list_COMP { get; set; }
        public List<DropDown_list_StkType> DropDown_list_StkType { get; set; }
        public List<DropDown_list_SubLegGrp> DropDown_list_SubLegGrp { get; set; }
        public List<DropDown_list_EMPCD> DropDown_list_EMPCD { get; set; }
        public List<DropDown_list_DOCCD> DropDown_list_DOCCD { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public List<DropDown_list_JOBCD> DropDown_list_JOBCD { get; set; }
        public List<DropDown_list_RTCD> DropDown_list_RTCD { get; set; }
        public List<DropDown_list_MTRLJOBCDList> DropDown_list_MTRLJOBCDList { get; set; }

        public string SubLeg_Grp { get; set; }
        public string Agslnm { get; set; }
        public string SubAgent { get; set; }
        public string BlType { get; set; }
        public string Brandnm { get; set; }
        public string Itnm { get; set; }
        public string Slmslnm { get; set; }
        public string Linenm { get; set; }
        public string Empnm { get; set; }
        public string Locnm { get; set; }
        public string Gonm { get; set; }
        public string Mtrljobnm { get; set; }
        
        public string PRCCD { get; set; }
        public string PRCNM { get; set; }
        public string JOBCD { get; set; }
        public List<ColumnName> ColumnName { get; set; }
        public string BARNO { get; set; }
        public string Translnm { get; set; }
        public string JOBNM { get; set; }
        public string TEXTBOX11 { get; set; }
        public string TEXTBOX12 { get; set; }
        public List<DropDown_list_BLTYPE> DropDown_list_BLTYPE { get; set; }


    }
}
