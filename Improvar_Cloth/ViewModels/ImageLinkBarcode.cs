using System.Collections.Generic;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class ImageLinkBarcode : Permission
    {
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string ActionName { get; set; }
        public string RepType { get; set; }
        public string BARNO { get; set; }
        public string BarImages { get; set; }
        public string DOCCD { get; set; }
        public string DOCNM { get; set; }
        public List<DropDown_list_MTRLJOBCD> DropDown_list_MTRLJOBCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        public string ITGRPNM { get; set; }
        public string STYLENO { get; set; }
        public string FABITNM { get; set; }
        public string PDESIGN { get; set; }
        public string BLSLNO { get; set; }
    }
}