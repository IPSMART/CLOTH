using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class ModifyLog
    {
        public List <ModifyLogGrid> ModifyLogGrid { get; set; }
    }
}
namespace Improvar.Models
{
    public class ModifyLogGrid
    {
        public string SLNO { get; set; }
        public string MODIFYDT { get; set; }
        public string USERID { get; set; }
    }
}