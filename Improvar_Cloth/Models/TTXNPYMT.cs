using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Improvar.Models
{
    public class TTXNPYMT
    {
        public bool Checked { get; set; }
        public short SLNO { get; set; }
        public string PYMTCD { get; set; }
        public string PYMTNM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? AMT { get; set; }
        public string CARDNO { get; set; }
        public string INSNO { get; set; }
        public string INSDT { get; set; }
        public string PYMTREM { get; set; }
        public string GLCD { get; set; }
    }
}