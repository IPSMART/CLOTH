using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class TTxnAckEntry : Permission
    {
        public T_TXNACK T_TXNACK { get; set; }
        public string ACK_DOCCTG { get; set; }
        public string ACK_FLAG1 { get; set; }
        public T_CNTRL_HDR T_CNTRL_HDR { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public string BLDOCCD { get; set; }
        public string BLDOCNM { get; set; }
        public string BLDOCNO { get; set; }
        public string BLDOCDT { get; set; }
        public string BLAUTONO { get; set; }
        public string BLSLCD { get; set; }
        public string BLSLNM { get; set; }
        public string BLTRANSLCD { get; set; }
        public string BLTRANSLNM { get; set; }
        public string BLLRNO { get; set; }
        public string BLLRDT { get; set; }
        public string TRSLNM { get; set; }

    }
}