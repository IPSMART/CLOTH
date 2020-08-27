using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.ViewModels
{
    public class PaymentReceivedEntry : Permission
    {
        public M_PAYMENT M_PAYMENT { get; set; }
         public M_GENLEG M_GENLEG { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
        public string GLNM { get; set; }
    }
}