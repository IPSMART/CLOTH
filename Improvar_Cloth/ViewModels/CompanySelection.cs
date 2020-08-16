using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class CompanySelection
    {
        [Display(Name = "Select Company")]
        public string COMPCD { get; set; }
        [Display(Name = " Select Location")]
        public string LOCCD { get; set; }
        [Display(Name = " Select Financial Year")]
        public string Finyr { get; set; }
        public bool Ischecked { get; set; }
        [Required]
        public List<CompanyCode> CompanyCode { get; set; }
        [Required]
        public List<CompanyLocation> CompanyLocation { get; set; }
        [Required]
        public List<CompanyFinyr> CompanyFinyr { get; set; }
        public string LocationJSON { get; set; }
        public string FinYearJSON { get; set; }
    }
}