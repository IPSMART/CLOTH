using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using OfficeOpenXml;
using System.Collections.Generic;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class Rep_AgentController : Controller
    {
        // GET: Rep_Agent
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();

        public ActionResult Rep_Agent()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Agent wise Report";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("A");
                    VE.Agslnm = MasterHelp.ComboFill("agslcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false;
                    VE.Checkbox2 = false;

                    List<DropDown_list1> RT = new List<DropDown_list1>();
                    DropDown_list1 RT1 = new DropDown_list1();
                    RT1.value = "Sales";
                    RT1.text = "Sales";
                    RT.Add(RT1);
                    DropDown_list1 RT2 = new DropDown_list1();
                    RT2.value = "Sales with Payment";
                    RT2.text = "Sales with Payment";
                    RT.Add(RT2);
                    VE.DropDown_list1 = RT;

                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        [HttpPost]
        public ActionResult Rep_Agent(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string reptype = VE.TEXTBOX1;
                if (reptype == "Sales")
                {
                    return Rep_Agent_Sale(FC, VE);
                }
                else
                {

                }
                return Content("");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        public ActionResult Rep_Agent_Sale(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();
                bool showitems = VE.Checkbox3;

                string selagslcd = "";
                if (FC.AllKeys.Contains("agslcdvalue")) selagslcd = CommFunc.retSqlformat(FC["agslcdvalue"].ToString());

                string repon = FC["ReportTakenOn"].retStr();
                bool exldump = VE.Checkbox2, showdocno = VE.Checkbox1; string agdsp = "Agent";
                string sorton = FC["sorton"]; //Party,Date
                if (repon == "S") agdsp = "Sub Agent";
                if (showdocno == false) sorton = "P";

                string sql = "";
                sql = "";
                sql += "select a.autono, a.slcd, d.docno, d.docdt, b.slnm, nvl(b.slarea,b.district) slarea, ";
                if (showitems == true) sql += "a.itcd, f.itnm, f.styleno, f.uomcd, a.qnty, "; else sql += "'' itcd, '' itnm, '' styleno, '' uomcd, 0 qnty, ";
                sql += "a.agslcd, c.slnm agslnm, nvl(c.slarea,c.district) agslarea, ";
                sql += "(case when a.doctag in ('SB','SD') then 1 else -1 end) mult, ";
                sql += "a.blamt, a.txblval from ";

                sql += "( select a.autono, b.slcd, " + (repon == "A"?"c.agslcd":"c.sagslcd") + " agslcd, b.doctag, b.blamt, sum(a.txblval) txblval ";
                if (showitems == true) sql += ", a.itcd, sum(a.qnty) qnty ";
                sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_txnoth c, " + scm + ".t_cntrl_hdr d	";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.autono=d.autono(+) and ";
                sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' ";
                sql += "group by a.autono, b.slcd, " + (repon == "A" ? "c.agslcd" : "c.sagslcd") + ", b.doctag, b.blamt ";
                if (showitems == true) sql += ", a.itcd ";
                sql += ") a, ";
                sql += scmf + ".m_subleg b, " + scmf + ".m_subleg c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
                if (showitems == true) sql += ", " + scm + ".m_sitem f ";
                sql += "where a.slcd=b.slcd(+) and a.agslcd=c.slcd(+) and a.autono=d.autono(+) and ";
                if (selagslcd != "") sql += "a.agslcd in (" + selagslcd + ") and ";
                if (showitems == true) sql += "a.itcd=f.itcd(+) and ";
                sql += "d.doccd=e.doccd(+) and e.doctype in ('SBILD','SBILL','SRET','SMSCN','SMSDN','SBEXP','SBCMR','SBCM') ";
                sql += "order by agslnm, agslcd, ";
                if (sorton == "P") sql += "slnm, slcd, ";
                if (showitems == true) sql += "styleno, itnm, itcd, ";
                sql += "docdt, docno ";

                DataTable tbl = MasterHelp.SQLquery(sql);

                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "", chkval2 = "", gonm = "";
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                if (exldump == true) HC.GetPrintHeader(IR, "agslcd", "string", "c,8", agdsp + " Code");
                if (exldump == true) HC.GetPrintHeader(IR, "agslnm", "string", "c,40", agdsp + " Name");
                if (exldump == true) HC.GetPrintHeader(IR, "agslarea", "string", "c,15",agdsp + " Area");
                if (showdocno == true) HC.GetPrintHeader(IR, "docno", "string", "c,20", "Bill No");
                if (showdocno == true) HC.GetPrintHeader(IR, "docdt", "string", "d,10", "Date");
                HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party cd");
                HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party Name");
                HC.GetPrintHeader(IR, "slarea", "string", "c,15", "Area");
                if (showitems == true)
                {
                    HC.GetPrintHeader(IR, "itcd", "string", "c,8", "Item cd");
                    HC.GetPrintHeader(IR, "itnm", "string", "c,15", "Item Name");
                    HC.GetPrintHeader(IR, "styleno", "string", "c,15", "Style no");
                    HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Qnty");
                }
                HC.GetPrintHeader(IR, "txblval", "double", "n,15,2", "Item Value");
                if (showitems == false) HC.GetPrintHeader(IR, "blamt", "double", "n,15,2", "Bill Amt");

                Int32 rNo = 0;
                double gamt1 = 0, gamt2 = 0, gqnty = 0;
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int gcount = 0;
                while (i <= maxR)
                {
                    chkval1 = tbl.Rows[i]["agslcd"].ToString();
                    if (exldump == false)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["agslcd"].retStr() + "  " + " </span>" + tbl.Rows[i]["agslnm"].retStr();
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }
                    double agamt1 = 0, agamt2 = 0, aqnty = 0;
                    int scount = 0;
                    gcount++;
                    while (tbl.Rows[i]["agslcd"].ToString() == chkval1)
                    {
                        string chkval2fld = "autono";
                        if (sorton == "P" || showdocno == false) chkval2fld = "slcd"; 
                        chkval2 = tbl.Rows[i][chkval2fld].ToString();

                        chkval = tbl.Rows[i][chkval2fld].ToString();
                        double pamt1 = 0, pamt2 = 0, pqnty = 0;
                        scount++;
                        while (tbl.Rows[i]["agslcd"].ToString() == chkval1 && tbl.Rows[i][chkval2fld].ToString() == chkval2)
                        {
                            string itcd = "";
                            if (showitems == true) itcd = tbl.Rows[i]["itcd"].ToString();
                            double iamt1 = 0, iamt2 = 0, iqnty = 0;
                            while (tbl.Rows[i]["agslcd"].ToString() == chkval1 && tbl.Rows[i][chkval2fld].ToString() == chkval2 && tbl.Rows[i]["itcd"].ToString() == itcd)
                            {
                                double txblval = tbl.Rows[i]["txblval"].retDbl() * tbl.Rows[i]["mult"].retDbl();
                                double blamt = tbl.Rows[i]["blamt"].retDbl() * tbl.Rows[i]["mult"].retDbl();
                                double qnty = 0;
                                if (showitems == true) qnty = tbl.Rows[i]["qnty"].retDbl() * tbl.Rows[i]["mult"].retDbl();
                                if (showdocno == true)
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    if (exldump == true)
                                    {
                                        IR.Rows[rNo]["agslcd"] = tbl.Rows[i]["agslcd"].retStr();
                                        IR.Rows[rNo]["agslnm"] = tbl.Rows[i]["agslnm"].retStr();
                                        IR.Rows[rNo]["agslarea"] = tbl.Rows[i]["agslarea"].retStr();
                                    }
                                    IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                                    IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                                    IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
                                    IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr();
                                    IR.Rows[rNo]["slarea"] = tbl.Rows[i]["slarea"].retStr();
                                    if (showitems == true)
                                    {
                                        IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"].retStr();
                                        IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].retStr();
                                        IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].retStr();
                                        IR.Rows[rNo]["qnty"] = qnty;
                                    }
                                    IR.Rows[rNo]["txblval"] = txblval;
                                    if (showitems == false) IR.Rows[rNo]["blamt"] = blamt;
                                }
                                pamt1 = pamt1 + txblval; pamt2 = pamt2 + blamt; pqnty = pqnty + qnty;
                                iamt1 = iamt1 + txblval; iamt2 = iamt2 + blamt; iqnty = iqnty + qnty;
                                i++;
                                if (i > maxR) break;
                            }
                            if (showitems == true)
                            {
                                if (showdocno == false && exldump == false)
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    if (exldump == true)
                                    {
                                        IR.Rows[rNo]["agslcd"] = tbl.Rows[i - 1]["agslcd"].retStr();
                                        IR.Rows[rNo]["agslnm"] = tbl.Rows[i - 1]["agslnm"].retStr();
                                        IR.Rows[rNo]["agslarea"] = tbl.Rows[i - 1]["agslarea"].retStr();
                                    }
                                    IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                                    IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                                    IR.Rows[rNo]["slarea"] = tbl.Rows[i - 1]["slarea"].retStr();
                                    IR.Rows[rNo]["itcd"] = tbl.Rows[i-1]["itcd"].retStr();
                                    IR.Rows[rNo]["itnm"] = tbl.Rows[i-1]["itnm"].retStr();
                                    IR.Rows[rNo]["styleno"] = tbl.Rows[i-1]["styleno"].retStr();
                                    IR.Rows[rNo]["qnty"] = iqnty;
                                    IR.Rows[rNo]["txblval"] = iamt1;
                                }
                            }
                            if (i > maxR) break;
                        }
                        if ((showdocno == true && sorton == "P" && exldump == false) || showitems == true)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["slnm"] + "] ";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            if (showitems == true) IR.Rows[rNo]["qnty"] = pqnty;
                            IR.Rows[rNo]["txblval"] = pamt1;
                            if (showitems == false) IR.Rows[rNo]["blamt"] = pamt2;
                        }
                        else if (showdocno == false && exldump == false)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            if (exldump == true)
                            {
                                IR.Rows[rNo]["agslcd"] = tbl.Rows[i-1]["agslcd"].retStr();
                                IR.Rows[rNo]["agslnm"] = tbl.Rows[i-1]["agslnm"].retStr();
                                IR.Rows[rNo]["agslarea"] = tbl.Rows[i-1]["agslarea"].retStr();
                            }
                            IR.Rows[rNo]["slcd"] = tbl.Rows[i-1]["slcd"].retStr();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i-1]["slnm"].retStr();
                            IR.Rows[rNo]["slarea"] = tbl.Rows[i-1]["slarea"].retStr();
                            if (showitems == true) IR.Rows[rNo]["qnty"] = pqnty;
                            IR.Rows[rNo]["txblval"] = pamt1;
                            if (showitems == false) IR.Rows[rNo]["blamt"] = pamt2;
                        }
                        agamt1 = agamt1 + pamt1; agamt2 = agamt2 + pamt2; aqnty = aqnty + pqnty;
                        if (i > maxR) break;
                    }
                    if (exldump == false)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (showdocno == false) IR.Rows[rNo]["slcd"] = scount.retStr();
                        IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["agslnm"] + "] ";
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        if (showitems == true) IR.Rows[rNo]["qnty"] = aqnty;
                        IR.Rows[rNo]["txblval"] = agamt1;
                        if (showitems == false) IR.Rows[rNo]["blamt"] = agamt2;
                    }
                    gamt1 = gamt1 + agamt1; gamt2 = gamt2 + agamt2; gqnty = gqnty + aqnty;
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["slcd"] = gcount.retStr();
                IR.Rows[rNo]["slnm"] = "Grand Total";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                if (showitems == true) IR.Rows[rNo]["qnty"] = gqnty;
                IR.Rows[rNo]["txblval"] = gamt1;
                if (showitems == false) IR.Rows[rNo]["blamt"] = gamt2;

                string pghdr1 = agdsp + " Report " + (showdocno == true?"[Detail]":"") + "from " + fdt + " to " + tdt;
                string repname = agdsp + (showdocno == true ? "Detail" : "");
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}