using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_AuditTrailController : Controller
    {
        Connection Cn = new Connection();
        DropDownHelp DropDownHelp = new DropDownHelp();
        MasterHelp MasterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: REP_MoneyRcpt_Reg
        public ActionResult Rep_AuditTrail()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ReportViewinHtml VE = new ReportViewinHtml();
                Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                ViewBag.formname = "Audit Trail";

                VE.FDT = CommVar.FinStartDate(UNQSNO);
                VE.TDT = CommVar.CurrDate(UNQSNO);

                string sql = "";
                sql = "select user_id, user_id||' - '||user_name user_name from user_appl union select distinct a.usr_id user_id,  a.usr_id||' - '||b.user_name user_name from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr a, user_appl b where a.usr_id=b.user_id(+) order by user_name";
                DataTable tbl = MasterHelp.SQLquery(sql);


                List<DropDown_list1> DropDown_list1 = new List<DropDown_list1>();
                VE.DropDown_list1 = (from DataRow dr in tbl.Rows
                                     select new DropDown_list1
                                     {
                                         text = dr["user_name"].retStr(),
                                         value = dr["user_id"].retStr()
                                     }).ToList();

                List<DropDown_list2> CT = new List<DropDown_list2>();
                DropDown_list2 CT1 = new DropDown_list2();
                CT1.text = "Add";
                CT1.value = "A";
                CT.Add(CT1);
                DropDown_list2 CT2 = new DropDown_list2();
                CT2.text = "Modify";
                CT2.value = "E";
                CT.Add(CT2);
                DropDown_list2 CT3 = new DropDown_list2();
                CT3.text = "Delete";
                CT3.value = "D";
                CT.Add(CT3);
                DropDown_list2 CT4 = new DropDown_list2();
                CT4.text = "Cancel";
                CT4.value = "C";
                CT.Add(CT4);
                VE.TEXTBOX1 = MasterHelp.ComboFill("Activity", CT, 0, 1);

                VE.DefaultView = true;

                return View(VE);
            }
        }

        [HttpPost]
        public ActionResult Rep_AuditTrail(ReportViewinHtml VE, FormCollection FC)
        {
            try
            {
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                string FD = VE.FDT;
                string TD = VE.TDT;
                string rephd1 = "Audit Trail for the Period " + FD + " TO " + TD;

                string seluser = "";
                if (FC.AllKeys.Contains("UserList")) seluser = FC["UserList"].ToString().retSqlformat();
                string selactivity = "";
                if (FC.AllKeys.Contains("Activityvalue")) selactivity = FC["Activityvalue"].ToString().retSqlformat();

                string DtChk = FC.Get("OPTIONS"); //DOCDT, SYSDT
                string reptype = FC.Get("reptype"); //D, S

                string scm = CommVar.CurSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), modcd = CommVar.ModuleShortCode();
                string dtflt = "docdt";
                string sql = "", comsql = "", skipdtype = "";

                switch (modcd)
                {
                    case "F": skipdtype = "'ABRS'"; break;
                    default: skipdtype = ""; break;
                }

                skipdtype = "'ABRS'";

                comsql += "from " + scm + ".t_cntrl_hdrt a, " + scm + ".m_doctype b, " + scm + ".m_dtype c " + Environment.NewLine;
                comsql += "where a.doccd=b.doccd and b.doctype=c.dcd(+) and a.modcd='" + modcd + "' and a.compcd='" + COM + "' and " + Environment.NewLine;
                comsql += "a.docdt >= to_date('" + CommVar.FinStartDate(UNQSNO) + "','dd/mm/yyyy') and " + Environment.NewLine;
                comsql += "b.doctype not in (" + skipdtype + ") and " + Environment.NewLine;
                if (DtChk == "DOCDT")
                {
                    comsql += "a.docdt >= to_date('" + FD + "','dd/mm/yyyy') and " + Environment.NewLine;
                    comsql += "a.docdt <= to_date('" + TD + "','dd/mm/yyyy') and " + Environment.NewLine;
                }

                if (reptype == "S")
                {
                    sql += "select dcd, dname, doccd, docnm,  " + Environment.NewLine;
                    sql += "sum(case  when dtag in ('A','AD') then treco else 0 end) addreco, " + Environment.NewLine;
                    sql += "sum(case  when dtag in ('E') then treco else 0 end) modreco, " + Environment.NewLine;
                    sql += "sum(case  when dtag in ('C') then treco else 0 end) cancreco, " + Environment.NewLine;
                    sql += "sum(case  when dtag in ('D') then treco else 0 end) delreco " + Environment.NewLine;
                    sql += "from " + Environment.NewLine;
                    sql += "( " + Environment.NewLine;
                    sql += "select a.dcd, a.dname, a.doccd, a.docnm, a.dtag, count(*) treco from " + Environment.NewLine;
                    sql += "( " + Environment.NewLine;
                }
                else
                {
                    sql += "select a.autono, a.dtag, a.emd_no, a.docno, a.docdt, a.usr_id, a.lm_usr_id, a.del_usr_id, a.usr_entdt, a.lm_usr_entdt,  a.del_usr_entdt, a.lm_rem, a.del_rem, a.doccd, a.docnm, a.dname, a.dcd,a.CANC_REM,a.CANC_USR_ENTDT,a.CANC_USR_ID from (" + Environment.NewLine;

                }
                sql += "/*  Voucher edit/delete */  " + Environment.NewLine;
                sql += "select a.autono, a.dtag, a.emd_no, a.docno, a.docdt, a.usr_id, a.lm_usr_id, a.del_usr_id, a.usr_entdt, a.lm_usr_entdt,  a.del_usr_entdt, a.lm_rem, a.del_rem, a.doccd, b.docnm, c.dname, c.dcd,a.CANC_REM,a.CANC_USR_ENTDT,a.CANC_USR_ID " + Environment.NewLine;
                sql += comsql;
                if (seluser != "") sql += "a.lm_usr_id in (" + seluser + ") and " + Environment.NewLine;
                if (seluser != "") sql += "a.del_usr_id in (" + seluser + ") and " + Environment.NewLine;
                if (DtChk == "SYSDT")
                {
                    dtflt = DtChk == "DOCDT" ? "a.docdt" : "a.lm_usr_entdt";
                    sql += "( ( to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') >= to_date('" + FD + "','dd/mm/yyyy') and " + Environment.NewLine;
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + TD + "','dd/mm/yyyy') ) or " + Environment.NewLine;
                    dtflt = DtChk == "DOCDT" ? "a.docdt" : "a.del_usr_entdt";
                    sql += "( to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') >= to_date('" + FD + "','dd/mm/yyyy') and " + Environment.NewLine;
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + TD + "','dd/mm/yyyy') ) ) " + Environment.NewLine;
                }
                else
                {
                    sql += "(a.usr_entdt is not null or a.lm_usr_entdt is not null or a.del_usr_entdt is not null ) " + Environment.NewLine;
                }
                sql += "union " + Environment.NewLine;
                sql += "/*  Voucher created*/ " + Environment.NewLine;
                sql += "select a.autono, 'A' dtag, a.emd_no, a.docno, a.docdt, a.usr_id, a.lm_usr_id, a.del_usr_id, a.usr_entdt, a.lm_usr_entdt, a.del_usr_entdt, a.lm_rem, a.del_rem, a.doccd, b.docnm, c.dname, c.dcd,a.CANC_REM,a.CANC_USR_ENTDT,a.CANC_USR_ID " + Environment.NewLine;
                sql += comsql.Replace("t_cntrl_hdrt", "t_cntrl_hdr");
                dtflt = DtChk == "DOCDT" ? "a.docdt" : "a.usr_entdt";
                if (seluser != "") sql += "a.usr_id in (" + seluser + ") and " + Environment.NewLine;
                if (DtChk == "SYSDT")
                {
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') >= to_date('" + FD + "','dd/mm/yyyy') and " + Environment.NewLine;
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + TD + "','dd/mm/yyyy') " + Environment.NewLine;
                }
                else
                {
                    sql += "a.usr_entdt is not null ";
                }
                sql += "union " + Environment.NewLine;
                sql += "/*  Voucher created but deleteed */ " + Environment.NewLine;
                sql += "select a.autono, 'AD' dtag, a.emd_no, a.docno, a.docdt, a.usr_id, a.lm_usr_id, a.del_usr_id, a.usr_entdt, a.lm_usr_entdt, a.del_usr_entdt, a.lm_rem, a.del_rem, a.doccd, b.docnm, c.dname, c.dcd,a.CANC_REM,a.CANC_USR_ENTDT,a.CANC_USR_ID " + Environment.NewLine;
                sql += comsql;
                dtflt = DtChk == "DOCDT" ? "a.docdt" : "a.usr_entdt";
                if (seluser != "") sql += "a.usr_id in (" + seluser + ") and " + Environment.NewLine;
                if (DtChk == "SYSDT")
                {
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') >= to_date('" + FD + "','dd/mm/yyyy') and " + Environment.NewLine;
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + TD + "','dd/mm/yyyy') " + Environment.NewLine;
                }
                else
                {
                    sql += "a.usr_entdt is not null and a.dtag='D' " + Environment.NewLine;
                }
                sql += "union " + Environment.NewLine;
                sql += "/*  Voucher cancelled */ " + Environment.NewLine;
                sql += "select a.autono, 'C' dtag, a.emd_no, a.docno, a.docdt, a.usr_id, a.lm_usr_id, a.del_usr_id, a.usr_entdt, a.lm_usr_entdt, a.del_usr_entdt, a.lm_rem, a.del_rem, a.doccd, b.docnm, c.dname, c.dcd,a.CANC_REM,a.CANC_USR_ENTDT,a.CANC_USR_ID " + Environment.NewLine;
                sql += comsql.Replace("t_cntrl_hdrt", "t_cntrl_hdr");
                dtflt = DtChk == "DOCDT" ? "a.docdt" : "a.canc_usr_entdt";
                if (seluser != "") sql += "a.canc_usr_id in (" + seluser + ") and " + Environment.NewLine;
                if (DtChk == "SYSDT")
                {
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') >= to_date('" + FD + "','dd/mm/yyyy') and " + Environment.NewLine;
                    sql += "to_date(to_char(" + dtflt + ",'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + TD + "','dd/mm/yyyy') " + Environment.NewLine;
                }
                else
                {
                    sql += "a.canc_usr_entdt is not null " + Environment.NewLine;
                }
                //sql += "order by dcd, dname, autono, emd_no " + Environment.NewLine;
                if (reptype == "S")
                {
                    sql += ") a  " + Environment.NewLine;
                    if (selactivity.retStr() != "") sql += " where a.dtag in (" + selactivity + ") " + Environment.NewLine;
                    sql += "group by a.dcd, a.dname, a.doccd, a.docnm, a.dtag " + Environment.NewLine;
                    sql += ") " + Environment.NewLine;
                    sql += "group by dcd, dname, doccd, docnm " + Environment.NewLine;
                    sql += "order by docnm " + Environment.NewLine;
                }
                else
                {
                    sql += ")a " + Environment.NewLine;
                    if (selactivity.retStr() != "") sql += " where a.dtag in (" + selactivity + ") " + Environment.NewLine;
                    sql += "order by a.dcd, a.dname, a.autono, a.emd_no " + Environment.NewLine;

                }
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found!");
                }

                DataTable IR = new DataTable("mstrep");

                Int32 rNo = 0;
                Int32 i = 0, maxR = tbl.Rows.Count - 1;

                HC.RepStart(IR, 2);
                HC.GetPrintHeader(IR, "dname", "string", "c,20", "Main Document Name");
                HC.GetPrintHeader(IR, "doccd", "string", "c,10", "DocCd");
                HC.GetPrintHeader(IR, "docnm", "string", "c,25", "Document Name");
                if (reptype == "S")
                {
                    HC.GetPrintHeader(IR, "addreco", "double", "n,9:##,##,##0", "Add");
                    HC.GetPrintHeader(IR, "modreco", "double", "n,9:##,##,##0", "Modify");
                    HC.GetPrintHeader(IR, "delreco", "double", "n,9:##,##,##0", "Delete");
                    HC.GetPrintHeader(IR, "cancreco", "double", "n,9:##,##,##0", "Cancel");

                    while (i <= maxR)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dname"] = tbl.Rows[i]["dname"];
                        IR.Rows[rNo]["doccd"] = tbl.Rows[i]["doccd"];
                        IR.Rows[rNo]["docnm"] = tbl.Rows[i]["docnm"];
                        if (tbl.Rows[i]["addreco"].retDbl() != 0) IR.Rows[rNo]["addreco"] = tbl.Rows[i]["addreco"].retDbl();
                        if (tbl.Rows[i]["modreco"].retDbl() != 0) IR.Rows[rNo]["modreco"] = tbl.Rows[i]["modreco"].retDbl();
                        if (tbl.Rows[i]["delreco"].retDbl() != 0) IR.Rows[rNo]["delreco"] = tbl.Rows[i]["delreco"].retDbl();
                        if (tbl.Rows[i]["cancreco"].retDbl() != 0) IR.Rows[rNo]["cancreco"] = tbl.Rows[i]["cancreco"].retDbl();
                        i++;
                    }
                }
                else
                {
                    if (selactivity.retStr() != "")
                    {
                        HC.GetPrintHeader(IR, "entryuserid", "string", "c,20", "Created By");
                        HC.GetPrintHeader(IR, "entryactdate", "string", "c,10", "Created Date & Time");
                    }
                    HC.GetPrintHeader(IR, "userid", "string", "c,20", "User ID");
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,20", "Doc No");
                    HC.GetPrintHeader(IR, "emdno", "double", "n,4", "Nos");
                    HC.GetPrintHeader(IR, "activity", "string", "c,10", "Activity");
                    HC.GetPrintHeader(IR, "actdate", "string", "c,10", "Date & Time");
                    HC.GetPrintHeader(IR, "actrem", "string", "c,25", "Remarks");

                    while (i <= maxR)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dname"] = tbl.Rows[i]["dname"];
                        IR.Rows[rNo]["doccd"] = tbl.Rows[i]["doccd"];
                        IR.Rows[rNo]["docnm"] = tbl.Rows[i]["docnm"];
                        IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                        IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"];
                        IR.Rows[rNo]["emdno"] = tbl.Rows[i]["emd_no"].retDbl();
                        if (selactivity.retStr() != "")
                        {
                            IR.Rows[rNo]["entryuserid"] = tbl.Rows[i]["usr_id"];
                            IR.Rows[rNo]["entryactdate"] = tbl.Rows[i]["usr_entdt"].retDateStr();
                        }
                        switch (tbl.Rows[i]["dtag"].retStr())
                        {
                            case "A":
                                IR.Rows[rNo]["activity"] = "Add";
                                IR.Rows[rNo]["userid"] = tbl.Rows[i]["usr_id"].retStr();
                                IR.Rows[rNo]["actdate"] = tbl.Rows[i]["usr_entdt"].retDateStr();
                                break;
                            case "AE":
                                IR.Rows[rNo]["activity"] = "Add";
                                IR.Rows[rNo]["userid"] = tbl.Rows[i]["usr_id"].retStr();
                                IR.Rows[rNo]["actdate"] = tbl.Rows[i]["usr_entdt"].retDateStr();
                                break;
                            case "D":
                                IR.Rows[rNo]["activity"] = "Delete";
                                IR.Rows[rNo]["userid"] = tbl.Rows[i]["del_usr_id"].retStr();
                                IR.Rows[rNo]["actdate"] = tbl.Rows[i]["del_usr_entdt"].retDateStr();
                                IR.Rows[rNo]["actrem"] = tbl.Rows[i]["del_rem"].retStr();
                                break;
                            case "E":
                                IR.Rows[rNo]["activity"] = "Modify";
                                IR.Rows[rNo]["userid"] = tbl.Rows[i]["lm_usr_id"].retStr();
                                IR.Rows[rNo]["actdate"] = tbl.Rows[i]["lm_usr_entdt"].retDateStr();
                                IR.Rows[rNo]["actrem"] = tbl.Rows[i]["lm_rem"].retStr();
                                break;
                            case "C":
                                IR.Rows[rNo]["activity"] = "Cancel";
                                IR.Rows[rNo]["userid"] = tbl.Rows[i]["CANC_USR_ID"].retStr();
                                IR.Rows[rNo]["actdate"] = tbl.Rows[i]["CANC_USR_ENTDT"].retDateStr();
                                IR.Rows[rNo]["actrem"] = tbl.Rows[i]["CANC_REM"].retStr();
                                break;
                        }

                        i++;
                    }
                }

                string pghdr1 = rephd1;
                string repname = CommFunc.retRepname("AuditTrail_");
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", true);
                TempData["Audit Trail"] = PV;
                TempData["Audit Trail" + "xxx"] = IR;

                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult ShowReport()
        {
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer");
        }
        public ActionResult PrintReport()
        {
            return RedirectToAction("PrintViewer", "RPTViewer");
        }
    }
}