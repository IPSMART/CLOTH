using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;

namespace Improvar.ViewModels
{
    public class UserRight : Menu
    {
        public List<MenuRightByUser> MenuRightByUser = new List<MenuRightByUser>();
        public string userID { get; set; }
        public List<DocumentType> user { get; set; }
        public int index { get; set; }
        public string serializeString { get; set; }
        public string serializeStringChild { get; set; }
        public List<URightByComp> Comp_List { get; set; }       
    }
    public class URightByComp
    {
        public bool Check { get; set; }
        public string comcd { get; set; }
        public string comnm { get; set; }
        public string loccd { get; set; }
        public string locnm { get; set; }
    }
}