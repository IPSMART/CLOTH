using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace Improvar.Models
{
    public class PrintingPageContent
    {
        [DataType(DataType.Text)]
        public string NAME { get; set; }

        [DataType(DataType.MultilineText)]
        public string PNTTXT { get; set; }

    }
}