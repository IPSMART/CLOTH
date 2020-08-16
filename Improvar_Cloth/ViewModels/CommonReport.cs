using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;
namespace Improvar.ViewModels
{
    public class CommonReport : Permission
    {
        public List<DropDown_list> DropDown_list { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }
        public List<DropDown_list3> DropDown_list3 { get; set; }
        public List<DropDown_list_GLCD> DropDown_list_GLCD { get; set; }
        public List<DropDown_list_TDS> DropDown_list_TDS { get; set; }
        public List<DropDown_list_SLCD> DropDown_list_SLCD { get; set; }
        public List<DropDown_list_Class1> DropDown_list_Class1 { get; set; }
        public List<DropDown_list_DOCCD> DropDown_DOCCD { get; set; }
        public List<DropDown_list_SubLegGrp> DropDown_list_SubLegGrp { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DATEFROM { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DATETO { get; set; }
        public bool CheckBox1 { get; set; }
        public bool CheckBox2 { get; set; }
        public bool CheckBox3 { get; set; }
        public bool CheckBox4 { get; set; }
        public bool CheckBox5 { get; set; }
        public bool CheckBox6 { get; set; }
        public bool CheckBox7 { get; set; }
        public bool CheckBox8 { get; set; }
        public bool CheckBox9 { get; set; }
        public string TextBox1 { get; set; }
        public string TextBox2 { get; set; }
        public string TextBox3 { get; set; }
        public string TextBox4 { get; set; }
        public string TextBox5 { get; set; }
        public string TextBox6{ get; set; }
        public List<DropDown_list_AGSLCD> DropDown_list_AGSLCD { get; set; }
        public List<DropDown_list_SLMSLCD> DropDown_list_SLMSLCD { get; set; }
        public string ItemName { get; set; }
        public string PartyName { get; set; }
        public string BrokerName { get; set; }
        public string EmployeeName { get; set; }
        public string ConsigneeName { get; set; }
        public string ProductGroupName { get; set; }
        public string SubLeg_Grp { get; set; }
        public string Rep_Format { get; set; }
    }
}