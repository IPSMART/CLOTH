using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Improvar.Models
{
    public class DateLockRight
    {
        public int Menu_index { get; set; }
        public string Menu_ID { get; set; }
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string Lockdate { get; set; }
        public bool BackDate { get; set; }
        public long M_AUTONO { get; set; }
    }
}