using System.Collections.Generic;
using Improvar.Models;


namespace Improvar.ViewModels
{
    public class RepBarcodePrint : Permission
    {
        
        public List<BarcodePrint> BarcodePrint { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string Reptype { get; set; }


    }
}