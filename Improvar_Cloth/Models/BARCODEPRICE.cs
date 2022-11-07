using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Improvar.Models
{
    public class BARCODEPRICE
    {
        public short SLNO { get; set; }
        public string EFFDT { get; set; }
        public string PRCCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double RATE { get; set; }
        public bool Checked { get; set; }

    }
}