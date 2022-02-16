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
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.DropDown_list_GLCD = DropDownHelp.DropDown_list_GLCD("D,C", "Y");
                    VE.Glnm = MasterHelp.ComboFill("glcd", VE.DropDown_list_GLCD, 0, 1);
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
                //string reptype = VE.TEXTBOX1;
                string reptype = FC["reptype"].retStr();
                if (reptype == "Sales")
                {
                    return Rep_Agent_Sale(FC, VE);
                }
                else
                {
                    return Rep_Agent_SalePayment(FC, VE);

                }
                //return Content("");
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

                sql += "( select a.autono, b.slcd, " + (repon == "A" ? "c.agslcd" : "c.sagslcd") + " agslcd, b.doctag, b.blamt, sum(a.txblval) txblval ";
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
                if (exldump == true) HC.GetPrintHeader(IR, "agslarea", "string", "c,15", agdsp + " Area");
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
                                    IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itcd"].retStr();
                                    IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itnm"].retStr();
                                    IR.Rows[rNo]["styleno"] = tbl.Rows[i - 1]["styleno"].retStr();
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
                                IR.Rows[rNo]["agslcd"] = tbl.Rows[i - 1]["agslcd"].retStr();
                                IR.Rows[rNo]["agslnm"] = tbl.Rows[i - 1]["agslnm"].retStr();
                                IR.Rows[rNo]["agslarea"] = tbl.Rows[i - 1]["agslarea"].retStr();
                            }
                            IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                            IR.Rows[rNo]["slarea"] = tbl.Rows[i - 1]["slarea"].retStr();
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

                string pghdr1 = agdsp + " Report " + (showdocno == true ? "[Detail]" : "") + "from " + fdt + " to " + tdt;
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
        public ActionResult Rep_Agent_SalePayment(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();
                bool showitems = VE.Checkbox3;

                string selagslcd = "", selslcd = "", selglcd = "";
                if (FC.AllKeys.Contains("agslcdvalue")) selagslcd = CommFunc.retSqlformat(FC["agslcdvalue"].ToString());
                if (FC.AllKeys.Contains("slcdvalue")) selslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("glcdvalue")) selglcd = CommFunc.retSqlformat(FC["glcdvalue"].ToString());
                string detail = FC["deatil"].retStr();
                string repon = FC["ReportTakenOn"].retStr();
                bool exldump = VE.Checkbox2, showdocno = VE.Checkbox1; string agdsp = "Agent";
                string sorton = FC["sorton"]; //Party,Date
                if (repon == "S") agdsp = "Sub Agent";
                if (showdocno == false) sorton = "P";

                string sql = "";

                sql += "select a.autono, a.slno, d.docno, d.docdt, a.autoslno, e.agslcd,g.slnm agslnm,nvl(g.slarea,g.district) agslarea, a.slcd,f.slnm,nvl(f.slarea,f.district) slarea, a.vchtype, a.drcr, a.amt, e.itamt, " + Environment.NewLine;
                sql += "b.vchtype adjtype, b.trcd, nvl(b.amt, 0) adjamt ";
                sql += " from ";

                sql += " (select a.slcd,a.glcd, a.autono, a.slno, a.autono || a.slno autoslno, a.vchtype, a.drcr, a.amt " + Environment.NewLine;
                sql += " from " + scmf + ".t_vch_bl a, " + scmf + ".t_cntrl_hdr b, " + scmf + ".m_genleg c " + Environment.NewLine;
                sql += " where a.autono = b.autono(+) and a.glcd = c.glcd and c.linkcd = 'D' and " + Environment.NewLine;
                sql += " b.compcd = '" + COM + "' and nvl(b.cancel, 'N') = 'N' and " + Environment.NewLine;
                sql += " b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
                if (selslcd != "") sql += " and a.slcd in(" + selslcd + ") " + Environment.NewLine;
                if (selglcd != "") sql += " and a.glcd in(" + selglcd + ") " + Environment.NewLine;
                sql += ") a, " + Environment.NewLine;
                sql += "(select a.autoslno, a.vchtype, a.trcd, sum(a.amt) amt from ( " + Environment.NewLine;
                sql += "select a.i_autono || a.i_slno autoslno, c.vchtype, d.trcd, sum(case c.drcr when 'C' then nvl(a.adj_amt, 0)else nvl(a.adj_amt, 0) * -1 end) amt " + Environment.NewLine;
                sql += "from " + scmf + ".t_vch_bl_adj a, " + scmf + ".t_cntrl_hdr b, " + scmf + ".t_vch_bl c, " + scmf + ".t_vch_hdr d " + Environment.NewLine;
                sql += "where a.autono = b.autono and a.r_autono = c.autono(+) and a.r_slno = c.slno(+) and a.autono = d.autono(+) and " + Environment.NewLine;
                sql += "nvl(b.cancel, 'N') = 'N' and " + Environment.NewLine;
                sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
                sql += "group by a.i_autono || a.i_slno, c.vchtype, d.trcd " + Environment.NewLine;
                sql += "union all " + Environment.NewLine;
                sql += "select a.r_autono || a.r_slno autoslno, c.vchtype, d.trcd, sum(case c.drcr when 'D' then nvl(a.adj_amt, 0)else nvl(a.adj_amt, 0) * -1 end) amt " + Environment.NewLine;
                sql += "from " + scmf + ".t_vch_bl_adj a, " + scmf + ".t_cntrl_hdr b, " + scmf + ".t_vch_bl c, " + scmf + ".t_vch_hdr d " + Environment.NewLine;
                sql += "where a.autono = b.autono and a.i_autono = c.autono(+) and a.i_slno = c.slno(+) and a.autono = d.autono(+) and " + Environment.NewLine;
                sql += "nvl(b.cancel, 'N') = 'N' and " + Environment.NewLine;
                sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
                sql += "group by a.r_autono || a.r_slno, c.vchtype, d.trcd ) a " + Environment.NewLine;
                sql += " group by a.autoslno, a.vchtype, a.trcd ) b, " + Environment.NewLine;

                sql += "(select a.autoslno, sum(a.curadj) curadj, sum(a.discamt) discamt, sum(a.tdsamt) tdsamt from " + Environment.NewLine;
                sql += "(select a.i_autono || a.i_slno autoslno, nvl(a.curadj, 0) curadj, nvl(a.discamt, 0) discamt, nvl(a.tdsamt, 0) tdsamt " + Environment.NewLine;
                sql += "from " + scmf + ".t_pytdtl a, " + scmf + ".t_pythdr b, " + scmf + ".t_cntrl_hdr c " + Environment.NewLine;
                sql += "where a.autono = b.autono and a.autono = c.autono and nvl(c.cancel, 'N') = 'N' and " + Environment.NewLine;
                sql += "c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
                sql += "union all " + Environment.NewLine;
                sql += "select a.r_autono || a.r_slno autoslno, nvl(a.curadj, 0) curadj, nvl(a.discamt, 0) discamt, nvl(a.tdsamt, 0) tdsamt " + Environment.NewLine;
                sql += "from " + scmf + ".t_pytdtl a, " + scmf + ".t_pythdr b, " + scmf + ".t_cntrl_hdr c " + Environment.NewLine;
                sql += "where a.autono = b.autono and a.autono = c.autono and nvl(c.cancel, 'N') = 'N' and " + Environment.NewLine;
                sql += "c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ) a " + Environment.NewLine;
                sql += "group by a.autoslno ) c, " + Environment.NewLine;

                sql += "(select a.autono, a.docno, a.docdt " + Environment.NewLine;
                sql += "from " + scmf + ".t_cntrl_hdr a ) d, " + Environment.NewLine;

                sql += "" + scmf + ".t_vch_bl e," + scmf + ".m_subleg f," + scmf + ".m_subleg g " + Environment.NewLine;

                sql += "where a.autoslno = b.autoslno(+) and a.autoslno = c.autoslno(+) and a.autono = d.autono(+) and " + Environment.NewLine;
                sql += "a.autono = e.autono(+) and a.slno = e.slno(+) and e.agslcd = g.slcd(+) and a.slcd = f.slcd(+) " + Environment.NewLine;
                if (selagslcd != "") sql += " and e.agslcd in(" + selagslcd + ") " + Environment.NewLine;
                sql += "order by e.agslcd,a.slcd,autoslno " + Environment.NewLine;


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
                HC.GetPrintHeader(IR, "slno", "string", "c,8", "Sl#");
                HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party cd");
                HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party Name");
                HC.GetPrintHeader(IR, "slarea", "string", "c,15", "Area");
                HC.GetPrintHeader(IR, "opblamt", "double", "n,15,2", "Opening Balance");
                if (detail == "D") HC.GetPrintHeader(IR, "docdt", "string", "d,10", "Bill Date");
                if (detail == "D") HC.GetPrintHeader(IR, "docno", "string", "c,20", "Bill No");
                if (detail == "D") HC.GetPrintHeader(IR, "amt", "double", "n,15,2", "Bill Amt");
                HC.GetPrintHeader(IR, "itamt", "double", "n,15,2", "Item Value");
                HC.GetPrintHeader(IR, "retamt", "double", "n,15,2", "Return Amt");
                HC.GetPrintHeader(IR, "rettxbl", "double", "n,15,2", "Return Taxable");
                HC.GetPrintHeader(IR, "discamt", "double", "n,15,2", "Disc");
                HC.GetPrintHeader(IR, "othamt", "double", "n,15,2", "Others");
                HC.GetPrintHeader(IR, "othtxbl", "double", "n,15,2", "Other Taxable");
                HC.GetPrintHeader(IR, "tdsamt", "double", "n,15,2", "TDS");
                HC.GetPrintHeader(IR, "payamt", "double", "n,15,2", "Payment Amt");
                HC.GetPrintHeader(IR, "paytxbl", "double", "n,15,2", "Payment Taxable");
                HC.GetPrintHeader(IR, "blncamt", "double", "n,15,2", "Balance");
                HC.GetPrintHeader(IR, "retper", "double", "n,15,2", "Return %");
                HC.GetPrintHeader(IR, "discper", "double", "n,15,2", "Disc %");

                Int32 rNo = 0;
                double gamt1 = 0, gamt2 = 0, giamt1 = 0, gRetamt = 0, gDiscamt = 0, gPayamt = 0, gOthamt = 0, gPaytxbl = 0, gTdsamt = 0;
                bool PrintSkip = false;
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int gcount = 0;
                while (i <= maxR)
                {
                    chkval1 = tbl.Rows[i]["agslcd"].ToString();
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + agdsp + " -" + " </span>" + tbl.Rows[i]["agslnm"].retStr() + "  " +"[ "+ tbl.Rows[i]["agslarea"].retStr() +" ]"+ "  " + "[ " + tbl.Rows[i]["agslcd"].retStr() + " ]";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                    double agamt1 = 0, agamt2 = 0, aiamt1 = 0, aRetamt = 0, aDiscamt = 0, aPayamt = 0, aOthamt = 0, aPaytxbl = 0, aTdsamt = 0;
                    int acount = 0;
                    gcount++;
                    while (tbl.Rows[i]["agslcd"].ToString() == chkval1)
                    {
                        int scount = 0;
                        string chkval2fld = "";
                        chkval2fld = "slcd";
                        chkval2 = tbl.Rows[i][chkval2fld].ToString();

                        chkval = tbl.Rows[i][chkval2fld].ToString();
                        double pamt1 = 0, pamt2 = 0, pqnty = 0, pRetamt = 0, pDiscamt = 0, pPayamt = 0, pOthamt = 0, pPaytxbl = 0, pTdsamt = 0;
                        double iamt1 = 0, iamt2 = 0, iqnty = 0;
                        PrintSkip = false;
                        while (tbl.Rows[i]["agslcd"].ToString() == chkval1 && tbl.Rows[i][chkval2fld].ToString() == chkval2)
                        {
                            PrintSkip = false;
                            string itcd = "", autono = ""; double chkRetamt = 0, chkDiscamt = 0, chkOthamt = 0, chkTdsamt = 0, chkPayamt = 0, calcBalamt = 0, calcPaytxblamt = 0;

                            autono = tbl.Rows[i]["autono"].ToString();

                            while (tbl.Rows[i]["agslcd"].ToString() == chkval1 && tbl.Rows[i][chkval2fld].ToString() == chkval2 && tbl.Rows[i]["autono"].ToString() == autono)
                            {
                                if (tbl.Rows[i]["adjtype"].retStr() == "CN" || tbl.Rows[i]["trcd"].retStr() == "SC")
                                {
                                    chkRetamt = chkRetamt + tbl.Rows[i]["adjamt"].retDbl();
                                }
                                else if (tbl.Rows[i]["adjtype"].retStr() == "DSC")
                                {
                                    chkDiscamt = chkDiscamt + tbl.Rows[i]["adjamt"].retDbl();
                                }
                                else if (tbl.Rows[i]["adjtype"].retStr() == "TDS")
                                {
                                    chkTdsamt = chkTdsamt + tbl.Rows[i]["adjamt"].retDbl();
                                }
                                else if ((tbl.Rows[i]["trcd"].retStr() == "BV") && (tbl.Rows[i]["adjtype"].retStr() == ""))
                                {
                                    chkPayamt = chkPayamt + tbl.Rows[i]["adjamt"].retDbl();
                                }
                                else {
                                    chkOthamt = chkOthamt + tbl.Rows[i]["adjamt"].retDbl();
                                }
                                
                                i++;
                                if (i > maxR) break;
                            }
                            calcPaytxblamt =(((tbl.Rows[i-1]["itamt"].retDbl() / tbl.Rows[i-1]["amt"].retDbl()) * (chkPayamt + chkDiscamt)) - chkDiscamt).toRound(2);
                            calcBalamt = (tbl.Rows[i - 1]["amt"].retDbl() - chkRetamt - chkDiscamt - chkOthamt - chkPayamt - chkTdsamt).retDbl();
                            if (((tbl.Rows[i - 1]["amt"].retDbl() == chkRetamt) || (tbl.Rows[i - 1]["amt"].retDbl() == chkDiscamt) || (tbl.Rows[i - 1]["amt"].retDbl() == chkTdsamt) || (tbl.Rows[i - 1]["amt"].retDbl() == chkPayamt) || (tbl.Rows[i - 1]["amt"].retDbl() == chkOthamt)) && (tbl.Rows[i - 1]["vchtype"].retStr() != "BL"))
                            {
                                PrintSkip = true;

                            }
                            if (PrintSkip == false && detail == "D")
                            {
                                scount++; acount++;
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["retamt"] = chkRetamt.retDbl();
                                IR.Rows[rNo]["discamt"] = chkDiscamt.retDbl();
                                IR.Rows[rNo]["tdsamt"] = chkTdsamt.retDbl();
                                IR.Rows[rNo]["payamt"] = chkPayamt.retDbl();
                                IR.Rows[rNo]["othamt"] = chkOthamt.retDbl();
                                IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                                IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                                IR.Rows[rNo]["slarea"] = tbl.Rows[i - 1]["slarea"].retStr();
                                IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].retDateStr();
                                IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].retStr();
                                IR.Rows[rNo]["amt"] = tbl.Rows[i - 1]["amt"].retDbl();
                                IR.Rows[rNo]["itamt"] = tbl.Rows[i - 1]["itamt"].retDbl();
                                IR.Rows[rNo]["paytxbl"] = calcPaytxblamt.retDbl();
                                IR.Rows[rNo]["blncamt"] = calcBalamt.retDbl();
                            }
                            if (PrintSkip == false && detail == "S")
                            { scount++; }
                                if (PrintSkip == true && detail == "D")
                            {
                                if (scount == 0)
                                {

                                    IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();
                                }
                            }
                            if (PrintSkip == false)
                            {
                                pamt1 = pamt1 + tbl.Rows[i - 1]["amt"].retDbl(); pamt2 = pamt2 + calcBalamt; pRetamt = pRetamt + chkRetamt;
                                iamt1 = iamt1 + tbl.Rows[i - 1]["itamt"].retDbl(); pDiscamt = pDiscamt + chkDiscamt; pPayamt = pPayamt + chkPayamt; pOthamt = pOthamt + chkOthamt; pPaytxbl = pPaytxbl + calcPaytxblamt.retDbl();
                                pTdsamt = pTdsamt + chkTdsamt;
                            }
                            if (i > maxR) break;
                        }

                        if (detail == "S" && scount>0)
                        {
                            acount++;
                           
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["retamt"] = pRetamt.retDbl();
                            IR.Rows[rNo]["discamt"] = pDiscamt.retDbl();
                            IR.Rows[rNo]["tdsamt"] = pTdsamt.retDbl();
                            IR.Rows[rNo]["payamt"] = pPayamt.retDbl();
                            IR.Rows[rNo]["othamt"] = pOthamt.retDbl();
                            IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                            IR.Rows[rNo]["slarea"] = tbl.Rows[i - 1]["slarea"].retStr();
                            IR.Rows[rNo]["itamt"] = iamt1.retDbl();
                            IR.Rows[rNo]["paytxbl"] = pPaytxbl.retDbl();
                            IR.Rows[rNo]["blncamt"] = pamt2;
                        }
                        if (detail == "S")
                        {
                            if (acount == 0)
                            {

                                IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();
                            }
                        }
                        if (detail == "D" && scount > 0)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["slnm"] + "] ";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            IR.Rows[rNo]["amt"] = pamt1;
                            IR.Rows[rNo]["itamt"] = iamt1;
                            IR.Rows[rNo]["retamt"] = pRetamt;
                            IR.Rows[rNo]["discamt"] = pDiscamt;
                            IR.Rows[rNo]["discamt"] = pTdsamt;
                            IR.Rows[rNo]["payamt"] = pPayamt;
                            IR.Rows[rNo]["othamt"] = pOthamt;
                            IR.Rows[rNo]["paytxbl"] = pPaytxbl;
                            IR.Rows[rNo]["blncamt"] = pamt2;

                        }
                        if (acount > 0)
                        {
                            agamt1 = agamt1 + pamt1; agamt2 = agamt2 + pamt2; aiamt1 = aiamt1 + iamt1; aRetamt = aRetamt + pRetamt; aDiscamt = aDiscamt + pDiscamt;
                            aPayamt = aPayamt + pPayamt; aOthamt = aOthamt + pOthamt; aPaytxbl = aPaytxbl + pPaytxbl; aTdsamt = aTdsamt + pTdsamt;
                        }

                        if (i > maxR) break;
                    }
                    if (acount > 0)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["agslnm"] + "] ";
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        if (detail == "D") IR.Rows[rNo]["amt"] = agamt1;
                        IR.Rows[rNo]["itamt"] = aiamt1;
                        IR.Rows[rNo]["retamt"] = aRetamt;
                        IR.Rows[rNo]["discamt"] = aDiscamt;
                        IR.Rows[rNo]["tdsamt"] = aTdsamt;
                        IR.Rows[rNo]["payamt"] = aPayamt;
                        IR.Rows[rNo]["othamt"] = aOthamt;
                        IR.Rows[rNo]["paytxbl"] = aPaytxbl;
                        IR.Rows[rNo]["blncamt"] = agamt2;


                    }
                  
                    //if (PrintSkip == false)
                    //{
                        gamt1 = gamt1 + agamt1; gamt2 = gamt2 + agamt2; giamt1 = giamt1 + aiamt1; gRetamt = gRetamt + aRetamt; gDiscamt = gDiscamt + aDiscamt;
                        gPayamt = gPayamt + aPayamt; gOthamt = gOthamt + aOthamt; gPaytxbl = gPaytxbl + aPaytxbl; gTdsamt = gTdsamt + aTdsamt;
                    //}
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["slcd"] = gcount.retStr();
                IR.Rows[rNo]["slnm"] = "Grand Total";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                if (detail == "D") IR.Rows[rNo]["amt"] = gamt1;
                IR.Rows[rNo]["itamt"] = giamt1;
                IR.Rows[rNo]["retamt"] = gRetamt;
                IR.Rows[rNo]["discamt"] = gDiscamt;
                IR.Rows[rNo]["tdsamt"] = gTdsamt;
                IR.Rows[rNo]["payamt"] = gPayamt;
                IR.Rows[rNo]["othamt"] = gOthamt;
                IR.Rows[rNo]["paytxbl"] = gPaytxbl;
                IR.Rows[rNo]["blncamt"] = gamt2;
                string pghdr1 = agdsp + " Report " + (detail =="D" ? "[Detail]" : "[Summary]") + "from " + fdt + " to " + tdt;
                string repname = agdsp + (detail == "D" ? "Detail" : "Summary");
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