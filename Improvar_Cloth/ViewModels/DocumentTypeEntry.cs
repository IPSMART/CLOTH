using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class DocumentTypeEntry : Permission
    {
        public List<DocumentType> DocumentType { get; set; }
        public List<DocumentNumbering> DocumentNumbering { get; set; }
        public List<DocumentWithoutZero> DocumentWithoutZero { get; set; }
        public List<CompanyLocationName> CompanyLocationName { get; set; }
        public List<CompanyLocationName> CompanyLocationChk { get; set; }
        public M_DOCTYPE M_DOCTYPE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public bool Checked { get; set; }
        public bool FDATE { get; set; }
        public bool BACKDATE { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public List<Database_Combo1> LASTDOCNOPATTERN { get; set; }
        public string MAINDOCNM { get; set; }
        public string isPresentinLastYrSchema { get; set; }

    }
}