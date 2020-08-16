using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace Improvar.ViewModels
{
    public class Prepare_JSONvalidation : Permission
    {       
        public List<Prepare_JSON> Prepare_JSON { get; set; }
        public List<EWB_SUPPLY_TYPE_Ddown> EWB_SUPPLY_TYPE_Ddown { get; set; }
        public List<EWB_SUB_TYPE_Ddown> EWB_SUB_TYPE_Ddown{ get; set; }        
        public List<EWB_DOC_TYPE_Ddown> EWB_DOC_TYPE_Ddown { get; set; }
        public List<EWB_TRANSACTION_TYPE_Ddown> EWB_TRANSACTION_TYPE_Ddown { get; set; }
        public List<EWB_BlFrmSt_TYPE_Ddown> EWB_BlFrmSt_TYPE_Ddown { get; set; }
        public List<EWB_DisFrmSt_TYPE_Ddown> EWB_DisFrmSt_TYPE_Ddown { get; set; }
        public List<EWB_BlTo_TYPE_Ddown> EWB_BlTo_TYPE_Ddown { get; set; }
        public List<EWB_ShipTo_TYPE_Ddown> EWB_ShipTo_TYPE_Ddown { get; set; }
        public List<EWB_Vehicle_TYPE_Ddown> EWB_Vehicle_TYPE_Ddown { get; set; }
        public List<EWB_Unit_Ddown> EWB_Unit_Ddown { get; set; }
        public List<EWB_Transmode_Ddown> EWB_Transmode_Ddown { get; set; }
    }
}