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
    public class Rep_SalesManController : Controller
    {
        // GET: Rep_SalesMan
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        public ActionResult Rep_SalesMan()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Salesman wise report";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("M,E");
                    VE.Slmslnm = MasterHelp.ComboFill("slmslcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false;
                    VE.Checkbox2 = false;

                    List<DropDown_list1> RT = new List<DropDown_list1>();
                    DropDown_list1 RT1 = new DropDown_list1();
                    RT1.value = "1";
                    RT1.text = "1";
                    RT.Add(RT1);
                    DropDown_list1 RT2 = new DropDown_list1();
                    RT2.value = "2";
                    RT2.text = "2";
                    RT.Add(RT2);
                    DropDown_list1 RT3 = new DropDown_list1();
                    RT3.value = "3";
                    RT3.text = "3";
                    RT.Add(RT3);
                    VE.DropDown_list1 = RT;
                    VE.TEXTBOX1 = MasterHelp.ComboFill("TEXTBOX1", VE.DropDown_list1, 0, 1);
                    VE.Checkbox1 = true; VE.Checkbox2 = true;
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
        public ActionResult Rep_SalesMan(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();
                bool showitems = false;
                bool exldump = false, showdocno = false; bool viewItem = VE.Checkbox1; bool viewUom = VE.Checkbox2;
                string selagslcd = "";
                if (FC.AllKeys.Contains("slmslcdvalue")) selagslcd = CommFunc.retSqlformat(FC["slmslcdvalue"].ToString());

                string reptype = FC["Reptype"].retStr();
                if (reptype == "D") { showitems = true; } else { exldump = true; showdocno = true; }


                //if (showdocno == false) sorton = "P";

                string sql = "";
                sql = "";
                sql += "select a.autono, d.docno, d.docdt, a.slno, a.slmslcd, e.slnm, a.per, c.txblval, b.blamt, a.mult, ";
                //sql += "c.uomcd, c.itgrpnm,c.itgrpcd, c.qnty,	";
                sql += "c.uomcd,c.qnty,	";
                sql += "round((c.qnty*a.per)/100,3) shqnty, ";
                sql += "round((c.txblval*a.per)/100,2)*a.mult shtxblval, round((b.blamt*a.per)/100,2)*a.mult shblamt from ";
                sql += "											";
                sql += "( select a.autono, a.slno, a.slmslcd, a.per, (case b.doctag when 'SB' then 1 else -1 end) mult, a.blamt ";
                sql += "from " + scm + ".t_txnslsmn a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c ";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and ";
                sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and ";
                sql += "c.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ) a, ";

                sql += "( select a.autono, sum(case a.stkdrcr when 'C' then a.txblval else a.txblval*-1 end) txblval, b.blamt ";
                sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c ";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and ";
                sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and ";
                sql += "c.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                sql += "group by a.autono, b.blamt ) b, ";

                //sql += "( select a.autono, b.uomcd, d.itgrpnm,d.itgrpcd, ";
                sql += "( select a.autono, b.uomcd,";
                sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 else 0 end) qnty, ";
                sql += "sum(case a.stkdrcr when 'C' then a.txblval when 'D' then a.txblval*-1 else 0 end) txblval ";
                sql += "from " + scm + ".t_txndtl a, " + scm + ".m_sitem b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_group d ";
                sql += "where a.itcd=b.itcd(+) and b.itgrpcd=d.itgrpcd(+) and a.autono=c.autono(+) and ";
                sql += "a.autono in (select autono from " + scm + ".t_txnslsmn) and ";
                sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and ";
                sql += "c.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                //sql += "group by a.autono, b.uomcd, d.itgrpnm,d.itgrpcd) c, ";
                sql += "group by a.autono, b.uomcd) c, ";
                sql += "" + scm + ".t_cntrl_hdr d, " + scmf + ".m_subleg e ";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.autono=d.autono(+) and a.slmslcd=e.slcd(+) ";
                if (selagslcd != "") sql += "and a.slmslcd in (" + selagslcd + ") ";
                sql += "order by slnm,autono											";
                //if (reptype == "S")
                //{
                //    sql = "select  a.slmslcd,a.slnm,a.uomcd, a.itgrpnm,a.itgrpcd, a.mult, sum(a.txblval) txblval,sum(a.blamt) blamt,sum(a.qnty) qnty,sum(a.shqnty) shqnty,sum(a.shtxblval) shtxblval,sum(a.shblamt) shblamt  from (" + sql + ") a ";
                //    sql += " group by a.slnm, a.slmslcd,a.uomcd, a.itgrpnm,a.itgrpcd, a.mult ";
                //    sql += " order by slnm";
                //}
                if (reptype == "S")
                {
                    sql = "select  a.slmslcd,a.slnm,a.uomcd, a.mult, sum(a.txblval) txblval,sum(a.blamt) blamt,sum(a.qnty) qnty,sum(a.shqnty) shqnty,sum(a.shtxblval) shtxblval,sum(a.shblamt) shblamt  from (" + sql + ") a ";
                    sql += " group by a.slnm, a.slmslcd,a.uomcd, a.mult ";
                    sql += " order by slnm";
                }

                DataTable tbl = MasterHelp.SQLquery(sql);

                Int32 i = 0;
                Int32 maxR = 0;
                string chkval1 = "";
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                if (exldump == true || showitems == true) HC.GetPrintHeader(IR, "slmslcd", "string", "c,8", "Salesman Code");
                if (exldump == true || showitems == true) HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Salesman Name");
                if (showitems == true && reptype == "D")
                {
                    HC.GetPrintHeader(IR, "docdt", "string", "d,10", "Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,20", "Doc No.");
                }
                if (viewItem == true) HC.GetPrintHeader(IR, "itgrpcd", "string", "c,8", "Item Grp cd");
                if (viewItem == true) HC.GetPrintHeader(IR, "itgrpnm", "string", "c,15", "Item Grp Name");
                if (viewUom == true) HC.GetPrintHeader(IR, "uomcd", "string", "c,15", "Uom");
                HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Qnty");
                HC.GetPrintHeader(IR, "txblval", "double", "n,15,2", "Item Value");
                HC.GetPrintHeader(IR, "blamt", "double", "n,15,2", "Bill Value");
                if (showitems == true && reptype == "D") HC.GetPrintHeader(IR, "per", "double", "n,15,2", "Per");
                HC.GetPrintHeader(IR, "shqnty", "double", "n,15,2", "Shared;Qnty");
                HC.GetPrintHeader(IR, "shtxblval", "double", "n,15,2", "Shared;Item Value");
                HC.GetPrintHeader(IR, "shblamt", "double", "n,15,2", "Shared;Bill Value");
                Int32 rNo = 0;
                double gamt1 = 0, gamt2 = 0, gqnty = 0, gshrqnty = 0, gshtxblval = 0, gshblamt = 0;
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int gcount = 0;
                while (i <= maxR)
                {
                    string slmslcd = tbl.Rows[i]["slmslcd"].ToString();
                    if (exldump == false)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["slmslcd"].retStr() + "  " + " </span>" + tbl.Rows[i]["slnm"].retStr();
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }
                    double agamt1 = 0, agamt2 = 0, aqnty = 0, ashrqnty = 0, ashtxblval = 0, ashblamt = 0;
                    gcount++;
                    double pamt1 = 0, pamt2 = 0, pqnty = 0, pshrqnty = 0, pshblamt = 0, pshtxblval = 0;

                    while (tbl.Rows[i]["slmslcd"].ToString() == slmslcd)
                    {
                        string chkfld1 = "";
                        if (showitems == true && reptype == "D") { chkfld1 = "autono"; chkval1 = tbl.Rows[i]["autono"].ToString(); }
                        else { chkfld1 = "slmslcd"; chkval1 = tbl.Rows[i]["slmslcd"].ToString(); }

                        while (tbl.Rows[i]["slmslcd"].ToString() == slmslcd && tbl.Rows[i][chkfld1].ToString() == chkval1)
                        {
                            double iamt1 = 0, iamt2 = 0, iqnty = 0;
                            string chkfld2 = "", chkval2 = "";
                            if (viewItem == true) { chkfld2 = "itgrpcd"; chkval2 = tbl.Rows[i]["itgrpcd"].ToString(); }
                            else { chkfld2 = "slmslcd"; chkval2 = tbl.Rows[i]["slmslcd"].ToString(); }
                            while (tbl.Rows[i]["slmslcd"].ToString() == slmslcd && tbl.Rows[i][chkfld1].ToString() == chkval1 && tbl.Rows[i][chkfld2].ToString() == chkval2)
                            {
                                string chkfld3 = "", chkval3 = "";
                                if (viewUom == true) { chkfld3 = "uomcd"; chkval3 = tbl.Rows[i]["uomcd"].ToString(); }
                                else { chkfld3 = "slmslcd"; chkval3 = tbl.Rows[i]["slmslcd"].ToString(); }
                                double tqnty = 0, ttxblval = 0, tblamt = 0, tshqnty = 0, tshtxblval = 0, tshblamt = 0;
                                while (tbl.Rows[i]["slmslcd"].ToString() == slmslcd && tbl.Rows[i][chkfld1].ToString() == chkval1 && tbl.Rows[i][chkfld2].ToString() == chkval2 && tbl.Rows[i][chkfld3].ToString() == chkval3)
                                {
                                    double txblval = tbl.Rows[i]["txblval"].retDbl() * tbl.Rows[i]["mult"].retDbl();
                                    double blamt = tbl.Rows[i]["blamt"].retDbl() * tbl.Rows[i]["mult"].retDbl();
                                    double qnty = 0;
                                    qnty = tbl.Rows[i]["qnty"].retDbl() * tbl.Rows[i]["mult"].retDbl();

                                    pamt1 = pamt1 + txblval; pamt2 = pamt2 + blamt; pqnty = pqnty + qnty;
                                    pshrqnty = pshrqnty + tbl.Rows[i]["shqnty"].retDbl();
                                    pshblamt = pshblamt + tbl.Rows[i]["shblamt"].retDbl();
                                    pshtxblval = pshtxblval + tbl.Rows[i]["shtxblval"].retDbl();
                                    gshrqnty += tbl.Rows[i]["shqnty"].retDbl();
                                    gshtxblval += tbl.Rows[i]["shtxblval"].retDbl();
                                    gshblamt += tbl.Rows[i]["shblamt"].retDbl();

                                    tqnty += qnty;
                                    ttxblval += txblval;
                                    if (showitems == true && reptype == "D")
                                    {
                                        tblamt = blamt;
                                    }
                                    else
                                    {
                                        tblamt += blamt;
                                    }
                                    tshqnty += tbl.Rows[i]["shqnty"].retDbl();
                                    tshtxblval += tbl.Rows[i]["shtxblval"].retDbl();
                                    tshblamt += tbl.Rows[i]["shblamt"].retDbl();

                                    iamt1 = iamt1 + txblval; iamt2 = iamt2 + blamt; iqnty = iqnty + qnty;
                                    i++;
                                    if (i > maxR) break;
                                }
                                //if (showdocno == true)
                                //{
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                if (exldump == true)
                                {
                                    IR.Rows[rNo]["slmslcd"] = tbl.Rows[i - 1]["slmslcd"].retStr();
                                    IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                                }
                                if (showitems == true) IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].retStr();
                                if (showitems == true) IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].retDateStr();
                                if (viewItem == true) IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i - 1]["itgrpcd"].retStr();
                                if (viewItem == true) IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                                if (viewUom == true) IR.Rows[rNo]["uomcd"] = tbl.Rows[i - 1]["uomcd"].retStr();
                                IR.Rows[rNo]["qnty"] = tqnty;

                                //}
                                IR.Rows[rNo]["txblval"] = ttxblval;
                                IR.Rows[rNo]["blamt"] = tblamt;
                                if (showitems == true)
                                {

                                    IR.Rows[rNo]["per"] = tbl.Rows[i - 1]["per"].retDbl();
                                }
                                IR.Rows[rNo]["shqnty"] = tshqnty;
                                IR.Rows[rNo]["shtxblval"] = tshtxblval;
                                IR.Rows[rNo]["shblamt"] = tshblamt;

                                if (i > maxR) break;
                            }
                            //if (showitems == true)
                            //{
                            //    if (showdocno == false && exldump == false)
                            //    {
                            //        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            //        if (exldump == true)
                            //        {
                            //            IR.Rows[rNo]["slmslcd"] = tbl.Rows[i - 1]["slmslcd"].retStr();
                            //            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                            //        }
                            //        IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].retStr();
                            //        IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].retDateStr();
                            //        if (viewItem == true) IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i - 1]["itgrpcd"].retStr();
                            //        if (viewItem == true) IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                            //        if (viewUom == true) IR.Rows[rNo]["uomcd"] = tbl.Rows[i - 1]["uomcd"].retStr();
                            //        IR.Rows[rNo]["qnty"] = iqnty;
                            //        IR.Rows[rNo]["txblval"] = iamt1;
                            //        IR.Rows[rNo]["per"] = tbl.Rows[i - 1]["per"].retDbl();
                            //        IR.Rows[rNo]["shqnty"] = tbl.Rows[i - 1]["shqnty"].retDbl();
                            //        IR.Rows[rNo]["shtxblval"] = tbl.Rows[i - 1]["shtxblval"].retDbl();
                            //        IR.Rows[rNo]["shblamt"] = tbl.Rows[i - 1]["shblamt"].retDbl();
                            //    }
                            //}
                            if (i > maxR) break;
                        }
                        //if (showitems == true && reptype == "D")
                        //{
                        //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        //    IR.Rows[rNo]["dammy"] = "";
                        //    IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["slnm"] + "] ";
                        //    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        //    IR.Rows[rNo]["shqnty"] = pshrqnty;
                        //    IR.Rows[rNo]["shtxblval"] = pshblamt;
                        //    IR.Rows[rNo]["shblamt"] = pshtxblval;


                        //}
                        //else if (showdocno == false && exldump == false)
                        //{
                        //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        //    if (exldump == true)
                        //    {
                        //        IR.Rows[rNo]["slmslcd"] = tbl.Rows[i - 1]["slmslcd"].retStr();
                        //        IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                        //    }
                        //    IR.Rows[rNo]["shqnty"] = pshrqnty;
                        //    IR.Rows[rNo]["shtxblval"] = pshtxblval;
                        //    IR.Rows[rNo]["shblamt"] = pshblamt;

                        //}
                        agamt1 = agamt1 + pamt1; agamt2 = agamt2 + pamt2; aqnty = aqnty + pqnty; ashrqnty = ashrqnty + pshrqnty; ashtxblval = ashtxblval + pshtxblval; ashblamt = ashblamt + pshblamt;
                        if (i > maxR) break;
                    }
                    if (showitems == true && reptype == "D")
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = "";
                        IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["slnm"] + "] ";
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        IR.Rows[rNo]["shqnty"] = pshrqnty;
                        IR.Rows[rNo]["shtxblval"] = pshtxblval;
                        IR.Rows[rNo]["shblamt"] = pshblamt;
                    }
                    else if (showdocno == false && exldump == false)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (exldump == true)
                        {
                            IR.Rows[rNo]["slmslcd"] = tbl.Rows[i - 1]["slmslcd"].retStr();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                        }
                        IR.Rows[rNo]["shqnty"] = pshrqnty;
                        IR.Rows[rNo]["shtxblval"] = pshtxblval;
                        IR.Rows[rNo]["shblamt"] = pshblamt;
                    }
                    gamt1 = gamt1 + agamt1; gamt2 = gamt2 + agamt2; gqnty = gqnty + aqnty;
                    //gshrqnty = gshrqnty + ashrqnty;
                    //gshtxblval = gshtxblval + ashtxblval;
                    //gshblamt = gshblamt + ashblamt;
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["slmslcd"] = gcount.retStr();
                IR.Rows[rNo]["slnm"] = "Grand Total";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["shqnty"] = gshrqnty;
                IR.Rows[rNo]["shtxblval"] = gshtxblval;
                IR.Rows[rNo]["shblamt"] = gshblamt;

                string pghdr1 = "Salesman wise " + " Report " + (reptype == "D" ? "[Detail]" : "[Summary]") + "from " + fdt + " to " + tdt;
                string repname = "Salesman wise " + (reptype == "D" ? "Detail" : "Summary");
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
        //public ActionResult Rep_SalesMan(FormCollection FC, ReportViewinHtml VE)
        //{
        //    try
        //    {
        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
        //        fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();
        //        bool showitems = false;
        //        bool exldump = false, showdocno = false; bool viewItem = VE.Checkbox1; bool viewUom = VE.Checkbox2;
        //        string selagslcd = "";
        //        if (FC.AllKeys.Contains("slmslcdvalue")) selagslcd = CommFunc.retSqlformat(FC["slmslcdvalue"].ToString());

        //        string reptype = FC["Reptype"].retStr();
        //        if (reptype == "D") { showitems = true; } else { exldump = true; showdocno = true; }


        //        //if (showdocno == false) sorton = "P";

        //        string sql = "";
        //        sql = "";
        //        sql += "select a.autono, d.docno, d.docdt, a.slno, a.slmslcd, e.slnm, a.per, b.txblval, b.blamt, a.mult, ";
        //        sql += "c.uomcd, c.itgrpnm,c.itgrpcd, c.qnty,	";
        //        sql += "round((c.qnty*a.per)/100,3) shqnty, ";
        //        sql += "round((b.txblval*a.per)/100,2)*a.mult shtxblval, round((b.blamt*a.per)/100,2)*a.mult shblamt from ";
        //        sql += "											";
        //        sql += "( select a.autono, a.slno, a.slmslcd, a.per, (case b.doctag when 'SB' then 1 else -1 end) mult, a.blamt ";
        //        sql += "from " + scm + ".t_txnslsmn a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c ";
        //        sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and ";
        //        sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and ";
        //        sql += "c.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ) a, ";

        //        sql += "( select a.autono, sum(case a.stkdrcr when 'C' then a.txblval else a.txblval*-1 end) txblval, b.blamt ";
        //        sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c ";
        //        sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and ";
        //        sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and nvl(c.cancel,'N')='N' and ";
        //        sql += "c.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
        //        sql += "group by a.autono, b.blamt ) b, ";

        //        sql += "( select a.autono, b.uomcd, d.itgrpnm,d.itgrpcd, ";
        //        sql += "sum(case a.stkdrcr when 'C' then a.qnty when 'D' then a.qnty*-1 else 0 end) qnty, ";
        //        sql += "sum(case a.stkdrcr when 'C' then a.txblval when 'D' then a.txblval*-1 else 0 end) txblval ";
        //        sql += "from " + scm + ".t_txndtl a, " + scm + ".m_sitem b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_group d ";
        //        sql += "where a.itcd=b.itcd(+) and b.itgrpcd=d.itgrpcd(+) and a.autono=c.autono(+) and ";
        //        sql += "a.autono in (select autono from " + scm + ".t_txnslsmn) and ";
        //        sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and ";
        //        sql += "c.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
        //        sql += "group by a.autono, b.uomcd, d.itgrpnm,d.itgrpcd) c, ";
        //        sql += "" + scm + ".t_cntrl_hdr d, " + scmf + ".m_subleg e ";
        //        sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.autono=d.autono(+) and a.slmslcd=e.slcd(+) ";
        //        if (selagslcd != "") sql += "and a.slmslcd in (" + selagslcd + ") ";
        //        sql += "order by slnm,autono											";
        //        if (reptype == "S")
        //        {
        //            sql = "select  a.slmslcd,a.slnm,a.uomcd, a.itgrpnm,a.itgrpcd, a.mult, sum(a.txblval) txblval,sum(a.blamt) blamt,sum(a.qnty) qnty,sum(a.shqnty) shqnty,sum(a.shtxblval) shtxblval,sum(a.shblamt) shblamt  from (" + sql + ") a ";
        //            sql += " group by a.slnm, a.slmslcd,a.uomcd, a.itgrpnm,a.itgrpcd, a.mult ";
        //            sql += " order by slnm";
        //        }

        //        DataTable tbl = MasterHelp.SQLquery(sql);

        //        Int32 i = 0;
        //        Int32 maxR = 0;
        //        string chkval, chkval1 = "", chkval2 = "", gonm = "";
        //        if (tbl.Rows.Count == 0)
        //        {
        //            return RedirectToAction("NoRecords", "RPTViewer");
        //        }

        //        DataTable IR = new DataTable("mstrep");

        //        Models.PrintViewer PV = new Models.PrintViewer();
        //        HtmlConverter HC = new HtmlConverter();

        //        HC.RepStart(IR, 3);
        //        if (exldump == true || showitems == true) HC.GetPrintHeader(IR, "slmslcd", "string", "c,8", "Salesman Code");
        //        if (exldump == true || showitems == true) HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Salesman Name");
        //        if (showitems == true && reptype == "D")
        //        {
        //            HC.GetPrintHeader(IR, "docdt", "string", "d,10", "Date");
        //            HC.GetPrintHeader(IR, "docno", "string", "c,20", "Doc No.");

        //        }
        //        if (viewItem == true) HC.GetPrintHeader(IR, "itgrpcd", "string", "c,8", "Item cd");
        //        if (viewItem == true) HC.GetPrintHeader(IR, "itgrpnm", "string", "c,15", "Item Name");
        //        if (viewUom == true) HC.GetPrintHeader(IR, "uomcd", "string", "c,15", "Uom");
        //        HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Qnty");
        //        HC.GetPrintHeader(IR, "txblval", "double", "n,15,2", "Item Value");
        //        HC.GetPrintHeader(IR, "blamt", "double", "n,15,2", "Bill Value");
        //        if (showitems == true && reptype == "D") HC.GetPrintHeader(IR, "per", "double", "n,15,2", "Per");
        //        HC.GetPrintHeader(IR, "shqnty", "double", "n,15,2", "Shared;Qnty");
        //        HC.GetPrintHeader(IR, "shtxblval", "double", "n,15,2", "Shared;Item Value");
        //        HC.GetPrintHeader(IR, "shblamt", "double", "n,15,2", "Shared;Bill Value");
        //        Int32 rNo = 0;
        //        double gamt1 = 0, gamt2 = 0, gqnty = 0, gshrqnty = 0, gshtxblval = 0, gshblamt = 0;
        //        // Report begins
        //        i = 0; maxR = tbl.Rows.Count - 1;
        //        int gcount = 0;
        //        while (i <= maxR)
        //        {
        //            chkval1 = tbl.Rows[i]["slmslcd"].ToString();
        //            if (exldump == false)
        //            {
        //                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["slmslcd"].retStr() + "  " + " </span>" + tbl.Rows[i]["slnm"].retStr();
        //                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
        //            }
        //            double agamt1 = 0, agamt2 = 0, aqnty = 0, ashrqnty = 0, ashtxblval = 0, ashblamt = 0;
        //            int scount = 0;
        //            gcount++;
        //            while (tbl.Rows[i]["slmslcd"].ToString() == chkval1)
        //            {
        //                string chkval2fld = "autono";
        //                //if (showdocno == false) chkval2fld = "slcd";
        //                //chkval2 = tbl.Rows[i][chkval2fld].ToString();

        //                // chkval = tbl.Rows[i][chkval2fld].ToString();
        //                double pamt1 = 0, pamt2 = 0, pqnty = 0, pshrqnty = 0, pshblamt = 0, pshtxblval = 0;
        //                scount++;
        //                while (tbl.Rows[i]["slmslcd"].ToString() == chkval1)//&& tbl.Rows[i]["autono"].ToString() == chkval2
        //                {
        //                    string itcd = "";
        //                    // if (showitems == true && reptype == "D")
        //                    itcd = tbl.Rows[i]["itgrpcd"].ToString();
        //                    double iamt1 = 0, iamt2 = 0, iqnty = 0;
        //                    while (tbl.Rows[i]["slmslcd"].ToString() == chkval1 && tbl.Rows[i]["itgrpcd"].ToString() == itcd)//&& tbl.Rows[i]["autono"].ToString() == chkval2
        //                    {
        //                        double txblval = tbl.Rows[i]["txblval"].retDbl() * tbl.Rows[i]["mult"].retDbl();
        //                        double blamt = tbl.Rows[i]["blamt"].retDbl() * tbl.Rows[i]["mult"].retDbl();
        //                        double qnty = 0;
        //                        qnty = tbl.Rows[i]["qnty"].retDbl() * tbl.Rows[i]["mult"].retDbl();
        //                        if (showdocno == true)
        //                        {
        //                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                            if (exldump == true)
        //                            {
        //                                IR.Rows[rNo]["slmslcd"] = tbl.Rows[i]["slmslcd"].retStr();
        //                                IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr();
        //                            }
        //                            if (showitems == true) IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
        //                            if (showitems == true) IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
        //                            if (viewItem == true) IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i]["itgrpcd"].retStr();
        //                            if (viewItem == true) IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
        //                            if (viewUom == true) IR.Rows[rNo]["uomcd"] = tbl.Rows[i]["uomcd"].retStr();
        //                            IR.Rows[rNo]["qnty"] = qnty;

        //                        }
        //                        IR.Rows[rNo]["txblval"] = txblval;
        //                        IR.Rows[rNo]["blamt"] = blamt;
        //                        if (showitems == true)
        //                        {

        //                            IR.Rows[rNo]["per"] = tbl.Rows[i]["per"].retDbl();
        //                        }
        //                        IR.Rows[rNo]["shqnty"] = tbl.Rows[i]["shqnty"].retDbl();
        //                        IR.Rows[rNo]["shtxblval"] = tbl.Rows[i]["shtxblval"].retDbl();
        //                        IR.Rows[rNo]["shblamt"] = tbl.Rows[i]["shblamt"].retDbl();
        //                        pamt1 = pamt1 + txblval; pamt2 = pamt2 + blamt; pqnty = pqnty + qnty;
        //                        pshrqnty = pshrqnty + tbl.Rows[i]["shqnty"].retDbl();
        //                        pshblamt = pshblamt + tbl.Rows[i]["shblamt"].retDbl();
        //                        pshtxblval = pshtxblval + tbl.Rows[i]["shtxblval"].retDbl();
        //                        iamt1 = iamt1 + txblval; iamt2 = iamt2 + blamt; iqnty = iqnty + qnty;
        //                        i++;
        //                        if (i > maxR) break;
        //                    }

        //                    if (showitems == true)
        //                    {
        //                        if (showdocno == false && exldump == false)
        //                        {
        //                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                            if (exldump == true)
        //                            {
        //                                IR.Rows[rNo]["slmslcd"] = tbl.Rows[i - 1]["slmslcd"].retStr();
        //                                IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
        //                            }
        //                            IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].retStr();
        //                            IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].retDateStr();
        //                            if (viewItem == true) IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i - 1]["itgrpcd"].retStr();
        //                            if (viewItem == true) IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
        //                            if (viewUom == true) IR.Rows[rNo]["uomcd"] = tbl.Rows[i - 1]["uomcd"].retStr();
        //                            IR.Rows[rNo]["qnty"] = iqnty;
        //                            IR.Rows[rNo]["txblval"] = iamt1;
        //                            IR.Rows[rNo]["per"] = tbl.Rows[i - 1]["per"].retDbl();
        //                            IR.Rows[rNo]["shqnty"] = tbl.Rows[i - 1]["shqnty"].retDbl();
        //                            IR.Rows[rNo]["shtxblval"] = tbl.Rows[i - 1]["shtxblval"].retDbl();
        //                            IR.Rows[rNo]["shblamt"] = tbl.Rows[i - 1]["shblamt"].retDbl();
        //                        }
        //                    }
        //                    if (i > maxR) break;
        //                }
        //                if (showitems == true && reptype == "D")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["dammy"] = "";
        //                    IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["slnm"] + "] ";
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
        //                    //if (showitems == true && reptype == "D")
        //                    //IR.Rows[rNo]["qnty"] = pqnty;
        //                    //IR.Rows[rNo]["txblval"] = pamt1;
        //                    //IR.Rows[rNo]["blamt"] = pamt2;
        //                    IR.Rows[rNo]["shqnty"] = pshrqnty;
        //                    IR.Rows[rNo]["shtxblval"] = pshblamt;
        //                    IR.Rows[rNo]["shblamt"] = pshtxblval;


        //                }
        //                else if (showdocno == false && exldump == false)
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    if (exldump == true)
        //                    {
        //                        IR.Rows[rNo]["slmslcd"] = tbl.Rows[i - 1]["slmslcd"].retStr();
        //                        IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
        //                    }
        //                    // if (showitems == true && reptype == "D")
        //                    //IR.Rows[rNo]["qnty"] = pqnty;
        //                    //IR.Rows[rNo]["txblval"] = pamt1;
        //                    //IR.Rows[rNo]["blamt"] = pamt2;
        //                    IR.Rows[rNo]["shqnty"] = pshrqnty;
        //                    IR.Rows[rNo]["shtxblval"] = pshtxblval;
        //                    IR.Rows[rNo]["shblamt"] = pshblamt;

        //                }
        //                agamt1 = agamt1 + pamt1; agamt2 = agamt2 + pamt2; aqnty = aqnty + pqnty; ashrqnty = ashrqnty + pshrqnty; ashtxblval = ashtxblval + pshtxblval; ashblamt = ashblamt + pshblamt;
        //                if (i > maxR) break;
        //            }
        //            //if (showitems == false)
        //            //{
        //            //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //            //    if (showitems == false) IR.Rows[rNo]["slmslcd"] = scount.retStr();
        //            //    IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["slnm"] + "] ";
        //            //    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
        //            //    if (showitems == true && reptype == "D") IR.Rows[rNo]["qnty"] = aqnty;
        //            //    IR.Rows[rNo]["txblval"] = agamt1;
        //            //    IR.Rows[rNo]["blamt"] = agamt2;
        //            //}
        //            gamt1 = gamt1 + agamt1; gamt2 = gamt2 + agamt2; gqnty = gqnty + aqnty;
        //            gshrqnty = gshrqnty + ashrqnty;
        //            gshtxblval = gshtxblval + ashtxblval;
        //            gshblamt = gshblamt + ashblamt;
        //            if (i > maxR) break;
        //        }
        //        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //        IR.Rows[rNo]["slmslcd"] = gcount.retStr();
        //        IR.Rows[rNo]["slnm"] = "Grand Total";
        //        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
        //        // if (showitems == true && reptype == "D")
        //        //IR.Rows[rNo]["qnty"] = gqnty;
        //        //IR.Rows[rNo]["txblval"] = gamt1;
        //        //if (showitems == false)
        //        //IR.Rows[rNo]["blamt"] = gamt2;
        //        IR.Rows[rNo]["shqnty"] = gshrqnty;
        //        IR.Rows[rNo]["shtxblval"] = gshtxblval;
        //        IR.Rows[rNo]["shblamt"] = gshblamt;

        //        string pghdr1 = "Salesman wise " + " Report " + (reptype == "D" ? "[Detail]" : "[Summary]") + "from " + fdt + " to " + tdt;
        //        string repname = "Salesman wise " + (reptype == "D" ? "Detail" : "Summary");
        //        PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

        //        TempData[repname] = PV;
        //        TempData[repname + "xxx"] = IR;
        //        return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message);
        //    }
        //}
    }
}