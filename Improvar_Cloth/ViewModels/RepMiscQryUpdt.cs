using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Improvar.ViewModels
{
    public class RepMiscQryUpdt : Permission
    {
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string BALEYR2 { get; set; }
        public string BALEYR1 { get; set; }
        public string BALEYR3 { get; set; }
        public string BALENO2 { get; set; }
        public string BALENO1 { get; set; }
        public string OLDBALENO { get; set; }
        public string NEWBALENO { get; set; }
        public string TEXTBOX1 { get; set; }
        public string LRNO1 { get; set; }
        public string LRNO2 { get; set; }
        public string LRNO3 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? LRDT1 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? LRDT2 { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? LRDT3 { get; set; }
        public string PBLNO2 { get; set; }
        public string BLAUTONO1 { get; set; }
        public string BLAUTONO2 { get; set; }
        public string BLAUTONO3 { get; set; }
        public string OLDSTYLENO { get; set; }
        public string OLDPAGENO { get; set; }
        public string NEWPAGENO { get; set; }
        public string NEWSTYLENO { get; set; }
        public string NEWPAGESLNO { get; set; }
        public string OLDPAGESLNO { get; set; }
        public string BLSLNO1 { get; set; }
        public string BLSLNO3 { get; set; }
        public string BLSLNO2 { get; set; }
        public string ITCD1 { get; set; }
        public string GOCD1 { get; set; }
        public string GOCD3 { get; set; }
        public string ITCD2 { get; set; }
        public string GOCD2 { get; set; }
        public string M_BARCODE { get; set; }
        public string MTRLJOBCD { get; set; }
        public string PARTCD { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DOCDT { get; set; }
        public string TAXGRPCD { get; set; }
        public string PRCCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        public string ITCD3 { get; set; }
        public string ITCD4 { get; set; }
        public string OLDPAGENOSLNO { get; set; }
        public string OLDBARNO { get; set; }
        public string NEWBARNO { get; set; }
        public string DOCNO { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public string BLAUTONO4 { get; set; }
        public string OLDPREFNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string OLDPREFDT { get; set; }
        public string NEWPREFNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? NEWPREFDT { get; set; }
        public double NEWRATE { get; set; }
        public double OLDRATE { get; set; }
        public double QNTY { get; set; }
        public string AUTONO { get; set; }
        public string ITCD { get; set; }
        public short SLNO { get; set; }
        public string BARNO { get; set; }
        public string STYLENO { get; set; }
        public string BALENO { get; set; }



        public DateTime? NEWLRDT { get; set; }
        public string NEWLRNO { get; set; }
        public DateTime? OLDLRDT { get; set; }
        public string OLDLRNO { get; set; }
        public string BLAUTONO7 { get; set; }
        public string SLNM7 { get; set; }
        public string SLCD7 { get; set; }
        public DateTime? DOCDT7 { get; set; }
        public string DOCNO7 { get; set; }

    }
}