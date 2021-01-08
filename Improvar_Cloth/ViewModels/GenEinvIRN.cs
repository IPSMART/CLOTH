using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Improvar.ViewModels
{
    public class GenEinvIRN : Permission
    {
        public string FROMDT { get; set; }
        public string TODT { get; set; }
        public string Message { get; set; }
        public List<GenEinvIRNGrid> GenEinvIRNGrid { get; set; }
        public string COMPCD { get; set; }
        public string QRTEXT { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public string Irn { get; set; }
        public string SellerGstin { get; set; }
        public string DocTyp { get; set; }
        public string TotInvVal { get; set; }
        public string BuyerGstin { get; set; }
        public string DocNo { get; set; }
        public string MainHsnCode { get; set; }
        public string ItemCnt { get; set; }
        public string IrnDt { get; set; }
        public string AppType { get; set; }
    }
}