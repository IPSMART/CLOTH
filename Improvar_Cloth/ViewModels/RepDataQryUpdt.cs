using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Improvar.ViewModels
{
    public class RepDataQryUpdt : Permission
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
        public string TEXTBOX2 { get; set; }
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
        public string OLDITCD { get; set; }
        public string OLDITNM { get; set; }
        public string NEWITNM { get; set; }
        public string NEWITCD { get; set; }
        public string NEWPAGESLNO { get; set; }
        public string OLDPAGESLNO { get; set; }
        public string PARTCD { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DOCDT { get; set; }
        public string TAXGRPCD { get; set; }
        public string PRCCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
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
        public string WORKDT { get; set; }
        public string STYLENO { get; set; }
        public string WORKNO { get; set; }
        public string WORKID { get; set; }
        public string OLDWORKNO { get; set; }
        public string NEWWORKNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? KLOSSPER { get; set; }
        public string SLNM_HALMRK { get; set; }
        public string SLCD_HALMRK { get; set; }
        //public List<FINITEM_MAIN_HUID> FINITEM_MAIN_HUID { get; set; }
        public string ITCD4 { get; set; }
        public string ITNM4 { get; set; }
        public double KMAKEPERCHNG { get; set; }
        public double SMAKEPERCHNG { get; set; }
        public List<MAKINGDETAIL> MAKINGDETAIL { get; set; }
        
        public List<ISSVALDETAIL> ISSVALDETAIL { get; set; }
        public bool AutoUpdateQnty { get; set; }
        public double RATE { get; set; }
        public double T_PCS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double T_WGHT { get; set; }


        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_SAMT { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double T_PAMT { get; set; }
        public string WORKNORATE { get; set; }
        public string AUTONORATE { get; set; }

        public string HUID_WORKNO { get; set; }
        public string HUID_SLCD { get; set; }
        public string ITCDRATE { get; set; }
        public string ITNMRATE { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string FROMDT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string TODT { get; set; }


        //new section start
        public List<CUSTOMER_DTL> CUSTOMER_DTL { get; set; }
        public string NM { get; set; }
        public string area { get; set; }
        public string gstno { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string agnm { get; set; }
        public string regmail { get; set; }
        public double? pin { get; set; }
        public double? regmob { get; set; }
    }

    public class CUSTOMER_DTL
    {
        public bool Checked { get; set; }
        public double SLNO { get; set; }
        public string NM { get; set; }
        public string AREA { get; set; }
        public string GSTNO { get; set; }
        public string ADDRESS { get; set; }
        public string CITY { get; set; }
        public string STATE { get; set; }
        public string AGNM { get; set; }
        public string REGMAIL { get; set; }
        public double? PIN { get; set; }
        public double? REGMOB { get; set; }


    }
    //finish

    public class MAKINGDETAIL
    {
        public bool Checked { get; set; }
        public double SLNO { get; set; }
        public string WORKNO { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double OLDKMAKERATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double OLDSMAKERATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double NEWKMAKERATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double NEWSMAKERATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double KMAKEPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double SMAKEPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double KMAKEFIX { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SMAKEFIX { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double OLDQNTY { get; set; }

        public string PURECD { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.0000}", ApplyFormatInEditMode = true)]
        public double RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double AMT { get; set; }
        public string AUTONO { get; set; }
        public double CntDi { get; set; }
        public string ITCD { get; set; }
        public string ITNM { get; set; }
       
    }

    
    public class ISSVALDETAIL
    {
        public bool Checked { get; set; }
        public double SLNO { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string AUTONO { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
    }


}