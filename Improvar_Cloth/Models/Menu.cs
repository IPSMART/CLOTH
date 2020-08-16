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
        public List<DashboardDetails> DashboardList { get; set; }
    }
    public class DashboardDetails
    {
        public string BoardCode { get; set; }
        public string Permission { get; set; }
        public string Caption { get; set; }
        public string RefreshedTime { get; set; }
        public DataTable DataTable { get; set; }
    }
}