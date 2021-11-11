using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TBILTYR_POPUP
    {
        public short SLNO { get; set; }
        public string BLAUTONO { get; set; }
        public string LRNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string LRDT { get; set; }
        public string BALENO { get; set; }
        public bool Checked { get; set; }
        public string PREFNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string PREFDT { get; set; }
        public string BALEYR { get; set; }
        public string STATUS { get; set; }
        public bool Checked_LRNO { get; set; }
    }
}