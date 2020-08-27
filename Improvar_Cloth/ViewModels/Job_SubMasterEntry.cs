using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class Job_SubMasterEntry : Permission
    {
        public List<SJOBCATEGORY> SJOBCATEGORY { get; set; }
        public List<SJOBSUBCATEGORY> SJOBSUBCATEGORY { get; set; }
        public List<SJOBMACHINE> SJOBMACHINE { get; set; }
        public List<SJOBBATCH> SJOBBATCH { get; set; }
        public List<SJOBSIZE> SJOBSIZE { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_JOBMSTSUB M_JOBMSTSUB { get; set; }
        public M_JOBMST M_JOBMST { get; set; }
        public bool Deactive { get; set; }
    } 
    public class SJOBCATEGORY
    {
        public string CATEGORY { get; set; }
    }
    public class SJOBSUBCATEGORY
    {
        public string SUBCATEGORY { get; set; }
    }
    public class SJOBMACHINE
    {
        public string MACHINE { get; set; }
    }
    public class SJOBSIZE
    {
        public string SIZE { get; set; }
    }
    public class SJOBBATCH
    {
        public string BATCH { get; set; }
    }
}