using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Improvar.Models
{
    public class Menu
    {
        public string ManuDetails { get; set; }
        public string FavoriteManuDetails { get; set; }
        public string UNQSNO { get; set; }
        public string PostAction { get; set; }
        public int CurrentIndex { get; set; }
        public List<DashboardDetails> DashboardList { get; set; }
        public List<DropDown_list_GLCD> DropDown_list_GLCD { get; set; }
    }
    public class DashboardDetails
    {
        public string BoardCode { get; set; }
        public string Permission { get; set; }
        public string Caption { get; set; }
        public string RefreshedTime { get; set; }
        public DataTable DataTable { get; set; }
        public EMAILSTATUSTBL EMAILSTATUSTBL { get; set; }
        public SPREPORT SPREPORT { get; set; }
    }
    public class EMAILSTATUSTBL
    {
        public string curdt { get; set; }
        public string autoreminderoff { get; set; }
        public string caldt { get; set; }
        public string graceday { get; set; }
        public string luserid { get; set; }
        public string luserentdt { get; set; }
        public bool autoreminderofff { get; set; }
        public string AGENT_NAME { get; set; }
        public string AGENT_CODE { get; set; }
        public string CCMAIL { get; set; }
        public string PARTY_CODE { get; set; }
        public string PARTY_NAME { get; set; }
    }
    public class SPREPORT
    {
        public string fdt { get; set; }
        public string tdt { get; set; }
        public string salepur { get; set; }
        public string glcd { get; set; }
        public int noofrec { get; set; }
    }
}