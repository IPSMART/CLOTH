using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class DataUploadVM : Permission
    {
        public List<DropDown_list> DropDown_list { get; set; }
    }
}