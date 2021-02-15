using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class BulkDocumentAttached
    {
        public string autono { get; set; }
        public string DDate { get; set; }
        public string Docno { get; set; }
        public string slcd { get; set; }
        public string slnm { get; set; }
        public string refno { get; set; }
        public string amount { get; set; }
        public string category { get; set; }
        public Cate0 Cate0 { get; set; }
        public Cate1 Cate1 { get; set; }
        public Cate2 Cate2 { get; set; }
        public Cate3 Cate3 { get; set; }
        public Cate4 Cate4 { get; set; }
        public Cate5 Cate5 { get; set; }
        public Cate6 Cate6 { get; set; }
        public Cate7 Cate7 { get; set; }
        public Cate8 Cate8 { get; set; }
        public Cate9 Cate9 { get; set; }      
       
    }
    public class Cate0
    {
        public string doccat0 { get; set; }
        public string DOC_FILE0 { get; set; }
        public string DOC_FILE_NAME0 { get; set; }
    }
    public class Cate1
    {
        public string doccat1 { get; set; }
        public string DOC_FILE1 { get; set; }
        public string DOC_FILE_NAME1 { get; set; }
    }
    public class Cate2
    {
        public string doccat2 { get; set; }
        public string DOC_FILE2 { get; set; }
        public string DOC_FILE_NAME2 { get; set; }
    }
    public class Cate3
    {
        public string doccat3 { get; set; }
        public string DOC_FILE3 { get; set; }
        public string DOC_FILE_NAME3 { get; set; }
    }
    public class Cate4
    {
        public string doccat4 { get; set; }
        public string DOC_FILE4 { get; set; }
        public string DOC_FILE_NAME4 { get; set; }
    }
    public class Cate5
    {
        public string doccat5 { get; set; }
        public string DOC_FILE5 { get; set; }
        public string DOC_FILE_NAME5 { get; set; }
    }
    public class Cate6
    {
        public string doccat6 { get; set; }
        public string DOC_FILE6 { get; set; }
        public string DOC_FILE_NAME6 { get; set; }
    }
    public class Cate7
    {
        public string doccat7 { get; set; }
        public string DOC_FILE7 { get; set; }
        public string DOC_FILE_NAME7 { get; set; }
    }
    public class Cate8
    {
        public string doccat8 { get; set; }
        public string DOC_FILE8 { get; set; }
        public string DOC_FILE_NAME8 { get; set; }
    }
    public class Cate9
    {
        public string doccat9 { get; set; }
        public string DOC_FILE9 { get; set; }
        public string DOC_FILE_NAME9 { get; set; }
    }
}