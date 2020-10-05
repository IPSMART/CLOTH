using System.Collections.Generic;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class BarcodePrintFormat : Permission
    {
        public List<MBARCODEPRINTFORMAT> MBARCODEPRINTFORMAT { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list_SLCD> DropDown_list_SLCD { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public M_BARCODEPRINTFORMAT M_BARCODEPRINTFORMAT { get; set; }
    }
}