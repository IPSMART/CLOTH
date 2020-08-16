using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSCHEMEFREE
    {
        public bool Checked { get; set; }
        public short SLNO { get; set; }
        public string SCMITMGRPCD { get; set; }
        public string SCMITMGRPNM { get; set; }
        public double SCMQNTY { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string SCMBOXPCS { get; set; }
        public string FREEITMGRPCD { get; set; }
        public string FREEITMGRPNM { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public string FREETYPE { get; set; }
        public string INCRMNTL { get; set; }
        public List<MSCHEMEINCREMENTAL> MSCHEMEINCREMENTAL { get; set; }
        [StringLength(1)]
        public string FREEBOXPCS { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double? FREEQNTY { get; set; }
        public List<DropDown_list4> DropDown_list4 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double DISCPER { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double DISCRATE { get; set; }
        //next grid
        public string FREEITMGRPCD1{ get; set; }
        public string FREEITMGRPNM1 { get; set; }
        public string FREETYPE1 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }
        [StringLength(1)]
        public string FREEBOXPCS1 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double? FREEQNTY1 { get; set; }
        public List<DropDown_list5> DropDown_list5 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double DISCPER1 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double DISCRATE1 { get; set; }

        //extra
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double SCMFRMQNTY { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        public double SCMTOQNTY { get; set; }

        //Control
        public bool activerow { get; set; }


    }
}