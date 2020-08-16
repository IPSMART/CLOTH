using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Improvar.Models
{
    public class Prepare_JSON
    {

        public string Supply_Type { get; set; }
        public string SubSupply_Type { get; set; }
        public string Doctype { get; set; }
        public string blno { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime bldt { get; set; }
        public string Transaction_Type { get; set; }

       
        [StringLength(100)]
        public string compnm{ get; set; }
        public string frmgstno{ get; set; }

        [StringLength(100)]
        public string frmadd1{ get; set; }

        [StringLength(100)]
        public string frmadd2{ get; set; }
        public string frmdistrict{ get; set; }
        public string frmpin{ get; set; }
        public string frmstatecd { get; set; }
        public string disptchfrmstatecd { get; set; }
        public string disptchfrmstnm{ get; set; }
        public string slnm{ get; set; }
        public string togstno{ get; set; }
        public string toadd1{ get; set; }
        [StringLength(100)]
        public string toadd2 { get; set; }
        [StringLength(30)]
        public string todistrict { get; set; }
        public string shiptopin { get; set; }
        public string billtostcd { get; set; }    
        public string shiptostcd { get; set; }
        public string itnm { get; set; }
        public string itdscp { get; set; }
        public string hsncode { get; set; }
        public string guomcd { get; set; }
        public string guomnmw { get; set; }
        public double qnty { get; set; }
        public double amt { get; set; }
        public string TtlTax { get; set; }
        public double cgstamt { get; set; }
        public double cgstper { get; set; }
        public double sgstper { get; set; }
        public double igstper { get; set; }
        public double cessper { get; set; }
        public double sgstamt { get; set; }
        public double igstamt { get; set; }
        public double cessamt { get; set; }
        public double cess_non_advol { get; set; }
        public double othramt { get; set; }
        public double invamt { get; set; }
        public string transMode { get; set; }
        public int distance { get; set; }
        public string trslnm { get; set; }
        public string trgst { get; set; }
        public string lrno { get; set; }
        public string lrdt { get; set; }
        public string Vehicle_No { get; set; }
        public string Vehile_Type { get; set; }
        public bool Checked { get; set; }
        public short SLNO { get; set; }


    }
    public class EWB_SUPPLY_TYPE_Ddown
    {
        public  string text { get;set;}
        public string value { get; set; }
    }
    public class EWB_SUB_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_DOC_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_TRANSACTION_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_BlFrmSt_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_DisFrmSt_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_BlTo_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_ShipTo_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class EWB_Vehicle_TYPE_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class EWB_Unit_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class EWB_Transmode_Ddown
    {
        public string text { get; set; }
        public string value { get; set; }
    }

}