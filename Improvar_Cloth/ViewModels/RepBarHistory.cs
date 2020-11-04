using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

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
        


    }
}