using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class dataLock:Permission
    {
        public bool Defaultview { get; set; }     
        public List<DateLockRight> DateLockRight { get; set; }
        public string serializeString { get; set; }
        public string ManuDetails { get; set; }
        public int index { get; set; }
    }
}