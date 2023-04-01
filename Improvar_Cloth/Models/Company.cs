using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class CompanyType
    {
        public string COMPTYCD { get; set; }
        public string COMPTYNM { get; set; }
    }
    public class CompanyName
    {
        public string COMPCD { get; set; }
        public string COMPNM { get; set; }
        public bool Checked { get; set; }

    }
    public class CompanyCode
    {
        public string COMPCD { get; set; }
        public string COMPNM { get; set; }

    }
    public class CompanyLocation
    {
        public string COMPCD { get; set; }
        public string LOCCD { get; set; }
        public string LOCNM { get; set; }
        public string COMPNM { get; set; }
    }
    public class CompanyFinyr
    {
        public string COMPCD { get; set; }
        public string LOCCD { get; set; }
        public string SCHEMA_NAME { get; set; }
        public string FINYR { get; set; }
        public string COMPNM { get; set; }
    }

    public class CompanyLocationName
    {
        public string COMPCD { get; set; }
        public string COMPNM { get; set; }
        public string LOCCD { get; set; }
        public string LOCNM { get; set; }
        public bool Checked { get; set; }
        public string Docpattern { get; set; }

    }
    public class CompanyLocationChk
    {
        public string COMPCD { get; set; }
        public string COMPNM { get; set; }
        public string LOCCD { get; set; }
        public string LOCNM { get; set; }
        public bool Checked { get; set; }
        public string Docpattern { get; set; }

    }
}