using System.Collections.Generic;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class ImageLinkBarcode : Permission
    {
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list_SLCD> DropDown_list_SLCD { get; set; }
        public List<DocumentType> DocumentType { get; set; }
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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public string DOCNM { get; set; }
        public string TEXTBOX6 { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        public string ITGRPNM { get; set; }
        public string STYLENO { get; set; }
        public string FABITNM { get; set; }
        public string PDESIGN { get; set; }
        public string BLSLNO { get; set; }
        public string AUTONO { get; set; }
        


    }
}