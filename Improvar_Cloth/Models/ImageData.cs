using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Improvar.Models
{
    public class ImageData
    {
        public DataTable ImageTable { get; set; }
        public string SqlQuery { get; set; }
    }
}