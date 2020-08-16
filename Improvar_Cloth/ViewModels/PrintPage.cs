using Improvar.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class PrintPage : Permission
    {
        public string PAGE_TITLE { get; set; }
        public string WIDTH { get; set; }
        public string HEIGHT { get; set; }
        public List<PrintingPageContent> PrintingPageContent { get; set; }

    }
}