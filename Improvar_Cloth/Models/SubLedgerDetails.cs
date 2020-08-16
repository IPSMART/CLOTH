using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class BusinessActivity
    {
        public string NATBUSCD { get; set; }
        public string NATBUSNM { get; set; }
        public bool Checked { get; set; }
    }

    public class ProductDeals
    {
        public string HSNSACCD { get; set; }
        public string HSNDESC { get; set; }
        public bool Checked { get; set; }
    }
    public class GeneralLedgerDetails
    {
        public string GLCD { get; set; }
        public string GLNM { get; set; }
        public string LINKCD { get; set; }
        public string LINKNM { get; set; }
        public bool Checked { get; set; }
    }
    public class PartyGroup
    {
        public string PARTYCD { get; set; }
        public string PARTYNM { get; set; }
    }
    public class RegistrationType
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
    public class Designation
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
    public class LinkType
    {
        public string LINKCD { get; set; }
        public string LINKNM { get; set; }
        public bool Checked { get; set; }
    }
    public class LedgerType
    {
        public string text { get; set; }
        public string value { get; set; }
    }

}