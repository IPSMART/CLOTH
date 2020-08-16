using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class Permission
    {
        public string Add { get; set; }
        public string Edit { get; set; }
        public string Delete { get; set; }
        public string View { get; set; }
        public string Check { get; set; }
        public bool Search_nav { get; set; }
        public string AuthorisationAUTHCD { get; set; }
        public string AuthorisationLVL { get; set; }
        public string AuthorisationSLNO { get; set; }
        public string AuthorisationStatus { get; set; }
        public bool Audit_nav { get; set; }
        public int AddDay { get; set; }
        public int EditDay { get; set; }
        public int DeleteDay { get; set; }
        public int DefaultDay { get; set; }
        public bool DefaultView { get; set; }
        public string DefaultAction { get; set; }
        public int ExitMode { get; set; }
        public string maxdate { get; set; }
        public string mindate { get; set; }
        public int Index { get; set; }
        public List<UploadDOC> UploadDOC { get; set; }
        public T_CNTRL_HDR_REM T_CNTRL_HDR_REM { get; set; }
        public string button { get; set; }
        public bool IsChecked { get; set; }
        public string MenuID { get; set; }
        public string MenuIndex { get; set; }        
        public string ControllerName { get; set; }
        public string MENU_DETAILS { get; set; }
        public string MENU_RIGHT { get; set; }
        public string UNQSNO { get; set; }//UNIQUESESSIONNO
        public string UNQSNO_ENCRYPTED { get; set; }//UNIQUESESSIONNO ENCRYPTED
        public string DOC_CODE { get; set; }
        public string MENU_PARA { get; set; }
        public string MENU_TYPE { get; set; }
        public string MENU_DATE_OPTION { get; set; }
        public string SearchValue { get; set; }
        public List<IndexKey> IndexKey { get; set; }
        public bool Searchpannel_State { get; set; }
        //public bool Searchpannel_Loadtm { get; set; }
        public bool CancelRecord { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOCDT { get; set; }
        public DateTime? LOCKDT { get; set; }
        public DateTime? DBMAXDT_DOCNO { get; set; }
        public bool AllowBackDate { get; set; }
        public bool VisiblePark { get; set; }
        public M_CNTRL_HDR_REM M_CNTRL_HDR_REM { get; set; }
        public string SrcFlagCaption { get; set; }
    }
}