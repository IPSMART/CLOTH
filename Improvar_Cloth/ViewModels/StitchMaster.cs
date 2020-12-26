using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class StitchMaster :Permission
    {
        public M_STCHGRP M_STCHGRP { get; set; }
        public M_STCHGRP_COMP M_STCHGRP_COMP { get; set; }
        public M_GENLEG M_GENLEG { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public string METHOD { get; set; }
        public bool Checked { get; set; }
        public bool Checked_INCLTAX { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<MSTCHGRPCOMP> MSTCHGRPCOMP { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        //public List<MFIXGRPDOCCD> MFIXGRPDOCCD { get; set; }
        public string JOBNM { get; set; }
       
    }
}