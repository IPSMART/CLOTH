using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Improvar.ViewModels
{
    public class GenEinvIRNCancel : Permission
    {
        public string COMPCD { get; set; }
        public string FROMDT { get; set; }
        public string TODT { get; set; }
        public string Message { get; set; }
        public List<GenEinvIRNGrid> GenEinvIRNGrid { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
    }
}