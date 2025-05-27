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
        public List<MGRPLIST> MGRPLIST { get; set; }
        [StringLength(5)]
        public string MGRPCD { get; set; }
        public string MGRPNM { get; set; }
        public bool FOUNDMGRP { get; set; }
        public string Tree { get; set; }
        public string Base64Tree { get; set; }
        public string SCHDL { get; set; }
        public bool LEGDTLSKP { get; set; }
        public List<Temp_TGRP> MLIST { get; set; }
        public List<AvailableACGroup> AvailableGroup { get; set; }
        public List<AvailableACGroup> ExistingGroup { get; set; }
        public List<DropDown_list1> SchedulePart { get; set; }        
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT { get; set; }
        public string ITGRPCD { get; set; }
        public bool CHECK { get; set; }
        public List<DropDown_list_ITGRP> DropDown_list_ITGRP { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FDT2 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TDT2 { get; set; }
        public bool CHECK2 { get; set; }
        public double T_sqnty { get; set; }
        public double T_samt { get; set; }
        public double T_rqnty { get; set; }
        public double T_ramt { get; set; }
        public double T_netamt { get; set; }
        public double T_netqnty { get; set; }
        public double T_rrate { get; set; }
        public List<billdet> billdet { get; set; }
        public List<ItmDet> ItemDet { get; set; }
    }
}
public class billdet
{
    public string styleno { get; set; }
    public string itgrpnm { get; set; }
    public string itnm { get; set; }    
    public double sqnty { get; set; }
    public double samt { get; set; }
    public double srate { get; set; }
    public double rqnty { get; set; }
    public double ramt { get; set; }
    public double rrate { get; set; }
    public double netqnty { get; set; }
    public double netamt { get; set; }
    public double netrate { get; set; }
}

public class ItmDet
{
    public string refdt { get; set; }
    public string refno { get; set; }
    public string slnm { get; set; }
    public string docnm { get; set; }
    public double sqnty { get; set; }
    public double samt { get; set; }
    public double rqnty { get; set; }
    public double ramt { get; set; } 
}

    public class MGRPLIST
{
    [StringLength(5)]
    public string MGRPCD { get; set; }

    [StringLength(40)]
    public string MGRPNM { get; set; }

    [StringLength(2)]
    public string MGRPTYPE { get; set; }
}

public class Temp_TGRP
{
   
    [Key]
    [Column(Order = 0)]
    [StringLength(5)]
    public string MGRPCD { get; set; }

    [StringLength(6)]
    public string GCD { get; set; }

    [StringLength(6)]
    public string PARENTCD { get; set; }


    [StringLength(10)]
    public string ROOTCD { get; set; }

    public int GRPSLNO { get; set; }

    [Key]
    [Column(Order = 1)]
    [StringLength(100)]
    public string GRPCDFULL { get; set; }

    [StringLength(50)]
    public string GRPNM { get; set; }
    public string Space { get; set; }

}

public class AvailableACGroup
{
    public string GLCD { get; set; }
    public string GLNM { get; set; }
    //public string SLCD { get; set; }
    //public string SLNM { get; set; }
    public string CLASS1CD { get; set; }
    public string CLASS1NM { get; set; }
    public bool Checked { get; set; }
}