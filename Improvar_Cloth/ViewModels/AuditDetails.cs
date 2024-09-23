using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Improvar.ViewModels
{
    public class AuditDetails : Permission
    {
        public string DOCNO { get; set; }
        public string DOCDT1 { get; set; }
        public string AUTONO { get; set; }
        public string EMD_NO { get; set; }
        public string OldEMDNO { get; set; }
        public string OldActivby { get; set; }
        public string OldActivDate { get; set; }
        public string NewEMDNO { get; set; }
        public string NewEMDDT { get; set; }
        public string NewActivby { get; set; }
        public string NewActivDate { get; set; }
        public List<AuditDetailsGrid_Old> AuditDetailsGrid_Old { get; set; }
        public string PrevEMD_NO { get; set; }
        public string NextEMD_NO { get; set; }

    }
}
namespace Improvar.Models
{
    public class AuditDetailsGrid_Old
    {

        public bool Checked { get; set; }
        public string ColName { get; set; }
        public string TableName { get; set; }
        public string OLD_ColValue { get; set; }
        public string New_ColValue { get; set; }

    }

}