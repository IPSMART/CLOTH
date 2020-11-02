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
        public short EFFDT { get; set; }
        public short PRCCD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public short RATE { get; set; }
        public bool Checked { get; set; }
        
    }
}