using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class UserWiseDocumentTypeRightsEntry : Permission
    {
        public List<User> User { get; set; }
        public string userID { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<MUSRACSDOCCD> MUSRACSDOCCD { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_USR_ACS_DOCCD M_USR_ACS_DOCCD { get; set; }
      
    }
}