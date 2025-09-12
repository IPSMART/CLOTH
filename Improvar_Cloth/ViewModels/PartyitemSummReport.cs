using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Improvar.ViewModels
{
    public class PartyitemSummReport : Permission
    {
        public string MSG { get; set; }
        [StringLength(5)]
        public string MGRPCD { get; set; }
        public string MGRPNM { get; set; }
        public bool FOUNDMGRP { get; set; }
        public string Tree { get; set; }
        public string Base64Tree { get; set; }
        public string SCHDL { get; set; }
        public bool LEGDTLSKP { get; set; }
        public List<DropDown_list1> SchedulePart { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT { get; set; }
        public string ITGRPCD { get; set; }
        public string LOCATION { get; set; }
        public bool CHECK { get; set; }
        public List<DropDown_list_ITGRP> DropDown_list_ITGRP { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT2 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT2 { get; set; }
        public bool CHECK2 { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_sqnty { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_samt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_rqnty { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_ramt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_netamt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_netqnty { get; set; }
        public double T_rrate { get; set; }
        public List<billdet> billdet { get; set; }
        public List<ItmDet> ItmDet { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_qnty { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_samti { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_amt { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_ramti { get; set; }
        public string SLCD2 { get; set; }
        public string ITGRPCD2 { get; set; }
        public string ONLYSALES2 { get; set; }
        public List<DropDown_list> DropDown_list { get; set; }
        public string LOCATION2 { get; set; }
        public string SALPUR { get; set; }
        public string SALPUR2 { get; set; }

    }
}
public class billdet
{
    public bool Checked { get; set; }
    public string itcd { get; set; }

    public string styleno { get; set; }
    public string itgrpnm { get; set; }
    public string itnm { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double sqnty { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double samt { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double srate { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double rqnty { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double ramt { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double rrate { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double netqnty { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double netamt { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double netrate { get; set; }
    public string BarImages { get; set; }
    public string uom { get; set; }
    public string BarImagesCount { get; set; }
}

public class ItmDet
{
    public string refdt { get; set; }
    public string refno { get; set; }
    public string Design { get; set; }
    public string slnm { get; set; }
    public string docnm { get; set; }
    public string colrnm { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double qnty { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double rate { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double amt { get; set; }
    [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
    public double ramt { get; set; }
    public string itrem { get; set; }
    public double disc { get; set; }
}

