using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class DataUploadVM : Permission
    {
        public string BLNO { get; set; }
        public string BLDT { get; set; }
        public List<DUpGrid> DUpGrid { get; set; }
        public string STATUS { get; set; }
    }
    public class ItemDet
    {
        public string ITCD { get; set; }
        public string BARNO { get; set; }
        public string PURGLCD { get; set; }
    }
    public class DUpGrid
    {
        public bool Checked { get; set; }
        public int Slno { get; set; }
        public string CUSTOMERNO { get; set; }
        public string BLNO { get; set; }
        public string BLDT { get; set; }
        public string TCSAMT { get; set; }
        public string BLAMT { get; set; }        
        public string MESSAGE { get; set; }
        public string ROAMT { get; set; }
    }
}