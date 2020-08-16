using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class UploadDOC
    {
        public bool chk { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public string docID { get; set; }
        public string DOC_DESC { get; set; }
        public string DOC_FILE { get; set; }
        public string DOC_FILE_NAME { get; set; }
    }

    public class TEMP_helpimg
    {     
        public string DOC_NAME { get; set; }
        public string DOC_FILE { get; set; }
        public long M_AUTONO { get; set; }
    }
    public class Imagein_HELPbox
    {
        public string CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string DATA1 { get; set; }
        public string DATA2 { get; set; }
        public int INT_DATA { get; set; }
        public string DOC_FILE { get; set; }
        public string DOC_FILE_NAME { get; set; }
    }
}