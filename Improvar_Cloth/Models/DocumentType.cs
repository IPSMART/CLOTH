using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class DocumentType
    {
        public string text { get; set; }
        public string value { get; set; }
        public string PRO_TAG { get; set; }
        
    }
    public class SHIFT
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DocumentNumbering
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DocumentWithoutZero
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class DOCTYPE
    {
        public string DOCCD { get; set; }
        public string DOCNM { get; set; }
    }


}