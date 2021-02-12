using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Improvar.ViewModels
{
    public class Trans_document_Attach : Permission
    {
        public List<DocumentType> DocumentType { get; set; }
        public string docTypeID { get; set; }
        public string docTypeName { get; set; }
        public string docCatID { get; set; }
        public string USERID { get; set; }
        public string msg { get; set; }
        public List<BulkDocumentAttached> GridData { get; set; }
        public string UserSelectedCategory { get; set; }
    }
}