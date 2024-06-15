using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_SalPurController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Sales_func = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_SalPur
        public ActionResult Rep_SalPur()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Sale Purchase Report";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection();
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");
                    //string[] selgrpcd = selgrp.Split(',');

                    VE.DropDown_list = (from i in DB.M_GROUP select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();

                    VE.DefaultView = true;
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
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
        public ActionResult Rep_SalPur(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string fdt = FC["FDT"].retDateStr(), tdt = FC["TDT"].retDateStr();
                string itgrpcd = "";
                string reptype = FC["reptype"].ToString();
                string sql = "";
                string txntag = "'SB','SR'";
                string selitcd = "", party = "";
                DataTable rsTbl = new DataTable();
                int maxR = 0, i = 0, rNo = 0;
                DataTable tblpur = new DataTable();

                if (FC.AllKeys.Contains("itcdvalue"))  //'ITEM'
                {
                    selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                }
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    party = FC["slcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].retStr());

                tblpur = Sales_func.GenStocktblwithVal("FIFO", Convert.ToDateTime(fdt).AddDays(-1).retDateStr(), "", "", itgrpcd, selitcd, "", true, "", false, "", "", "", "", false, false, tdt);

                sql = "";
                sql += "select a.autono, a.doctag, a.docno, a.docdt, a.slcd, a.slnm, a.slno, a.itcd,a.ITGRPCD,a.itgrpnm,a.STYLENO, a.itnm, a.uomnm, a.batchautono, a.batchno,a.barno, " + Environment.NewLine; ;
                sql += "sum(a.qnty)qnty, sum(a.txblval)txblval, sum(ROUND(((a.rate*a.qnty) + ((a.discamt/a.iqnty)*a.qnty)),2)) btxblval,a.itnm||' '||a.STYLENO itstyle,sum( nvl(b.othamt,0)) othamt, " + Environment.NewLine; ;
                sql += "a.pblno, a.pdocdt, a.prate,a.rate from " + Environment.NewLine; ;

                sql += "( select a.autono, c.doctag, nvl(c.prefno,d.docno) docno, d.docdt, c.slcd, e.slnm, b.slno, b.itcd,f.itgrpcd,k.itgrpnm,f.STYLENO, f.itnm, h.uomnm, ''batchautono, i.batchno,i.barno, " + Environment.NewLine; ;
                sql += "nvl(a.qnty,b.qnty) qnty, nvl(b.qnty,a.qnty) iqnty, b.rate, nvl(b.discamt,0)-nvl(b.SCMDISCAMT,0) discamt, " + Environment.NewLine; ;
                sql += "b.amt-nvl(b.discamt,0)-nvl(b.SCMDISCAMT,0) txblval, nvl(j.prefno,l.docno) pblno, l.docdt pdocdt, i.rate+nvl(i.othrate,0) prate " + Environment.NewLine; ;
                sql += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_txndtl b, " + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + Environment.NewLine; ;
                sql += scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scmf + ".m_uom h, " + Environment.NewLine; ;
                sql += scm1 + ".t_batchmst i, " + scm1 + ".t_txn j, " + scm1 + ".m_group k, " + scm1 + ".t_cntrl_hdr l " + Environment.NewLine; ;
                sql += "where b.autono=a.autono(+) and b.slno=a.txnslno(+) and b.autono=c.autono and b.autono=d.autono and " + Environment.NewLine; ;
                sql += "f.uomcd=h.uomcd(+) and  a.barno=i.barno(+) and i.autono=j.autono(+) and i.autono=l.autono(+) and f.itgrpcd=k.itgrpcd(+) and " + Environment.NewLine; ;
                sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' and " + Environment.NewLine; ;
                sql += "c.doctag in (" + txntag + ") and  ";
                sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  " + Environment.NewLine; ;
                sql += "c.slcd=e.slcd and b.itcd=f.itcd(+) " + Environment.NewLine;
                if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") " + Environment.NewLine;
                sql += ") a, " + Environment.NewLine; ;

                sql += "( select a.autono, sum(case c.addless when 'A' then a.amtrate when 'L' then a.amtrate*-1 end) othamt " + Environment.NewLine; ;
                sql += "from " + scm1 + ".t_txnamt a, " + scm1 + ".t_cntrl_hdr b, " + scm1 + ".m_amttype c " + Environment.NewLine; ;
                sql += "where a.autono=b.autono and a.amtcd=c.amtcd and b.compcd='" + COM + "' and b.loccd='" + LOC + "' " + Environment.NewLine; ;
                sql += "group by a.autono ) b " + Environment.NewLine; ;

                sql += "where a.autono=b.autono(+) " + Environment.NewLine; ;

                if (selitcd.retStr() != "") sql += "and a.itcd in (" + selitcd + ")  " + Environment.NewLine;
                //if (party.retStr() != "") sql += "and a.slcd in (" + party + ") " + Environment.NewLine;

                sql += "group by a.autono, a.doctag, a.docno, a.docdt, a.slcd, a.slnm, a.slno, a.itcd,a.ITGRPCD,a.itgrpnm,a.STYLENO, a.itnm, a.uomnm, a.batchautono, a.batchno,a.barno, " + Environment.NewLine; ;
                sql += "a.itnm||' '||a.STYLENO , " + Environment.NewLine; ;
                sql += "a.pblno, a.pdocdt, a.prate,a.rate  " + Environment.NewLine; ;
                if (reptype == "details")
                {
                    sql += "order by docdt, docno " + Environment.NewLine;
                }
                else if (reptype == "sumpar")
                {
                    sql += "order by slcd,slnm " + Environment.NewLine;
                }
                else
                {
                    sql += "order by a.itcd,a.itnm,a.STYLENO " + Environment.NewLine;

                }
                rsTbl = MasterHelp.SQLquery(sql);

                if (rsTbl.Rows.Count == 0)
                {
                    return Content("no records..");
                }

                DataTable fixRs = new DataTable("stock");
                fixRs.Columns.Add("AUTONO", typeof(string), "");
                fixRs.Columns.Add("docno", typeof(string), "");
                fixRs.Columns.Add("docdt", typeof(string), "");
                fixRs.Columns.Add("slcd", typeof(string), "");
                fixRs.Columns.Add("slnm", typeof(string), "");
                fixRs.Columns.Add("ITGRPNM", typeof(string), "");
                fixRs.Columns.Add("itcd", typeof(string), "");
                fixRs.Columns.Add("itstyle", typeof(string), "");
                fixRs.Columns.Add("uom", typeof(string), "");
                fixRs.Columns.Add("qnty", typeof(double), "");
                fixRs.Columns.Add("srate", typeof(double), "");
                fixRs.Columns.Add("samt", typeof(double), "");
                fixRs.Columns.Add("prate", typeof(double), "");
                fixRs.Columns.Add("pamt", typeof(double), "");
                fixRs.Columns.Add("diffamt", typeof(double), "");
                fixRs.Columns.Add("pblno", typeof(string), "");

                i = 0; maxR = 0; rNo = 0;
                maxR = rsTbl.Rows.Count - 1;
                while (i <= maxR)
                {
                    if (rsTbl.Rows[i]["docno"].retStr() == "SNSRN/0104")
                    {
                        var xx = "";
                    }
                    string itcd = rsTbl.Rows[i]["itcd"].retStr();
                    string barno = rsTbl.Rows[i]["barno"].retStr();
                    double salBalqnty = rsTbl.Rows[i]["qnty"].retDbl();
                    double balqnty = 0;
                    double salCostAmt = 0;
                    string purdocno = "";
                    var temppur = tblpur.Select("itcd='" + itcd + "' and barno='" + barno + "' and balqnty <> outqnty");
                    if (temppur.Count() > 0)
                    {
                        for (int j = 0; j <= temppur.Count() - 1; j++)
                        {
                            balqnty = temppur[j]["balqnty"].retDbl() - temppur[j]["outqnty"].retDbl();
                            if (salBalqnty <= balqnty)
                            {
                                salCostAmt = salCostAmt + (salBalqnty * temppur[j]["rate"].retDbl()).toRound(2);
                                temppur[j]["outqnty"] = temppur[j]["outqnty"].retDbl() + salBalqnty;
                                salBalqnty = 0;
                                purdocno += temppur[j]["docno"].retStr() + ",";
                            }
                            else
                            {
                                salCostAmt = salCostAmt + (balqnty * temppur[j]["rate"].retDbl()).toRound(2);
                                salBalqnty = salBalqnty - balqnty;
                                temppur[j]["outqnty"] = temppur[j]["balqnty"].retDbl();
                                purdocno += temppur[j]["docno"].retStr() + ",";
                            }
                            if (salBalqnty <= 0) break;
                        }
                    }
                    if (purdocno != "")
                    {
                        purdocno = purdocno.Substring(0, purdocno.Length - 1);
                    }
                    fixRs.Rows.Add(""); rNo = fixRs.Rows.Count - 1;
                    fixRs.Rows[rNo]["docno"] = rsTbl.Rows[i]["docno"];
                    fixRs.Rows[rNo]["docdt"] = rsTbl.Rows[i]["docdt"].retDateStr();
                    fixRs.Rows[rNo]["slcd"] = rsTbl.Rows[i]["slcd"];
                    fixRs.Rows[rNo]["slnm"] = rsTbl.Rows[i]["slnm"];
                    fixRs.Rows[rNo]["ITGRPNM"] = rsTbl.Rows[i]["ITGRPNM"];
                    fixRs.Rows[rNo]["itcd"] = rsTbl.Rows[i]["itcd"];
                    fixRs.Rows[rNo]["itstyle"] = rsTbl.Rows[i]["itstyle"];
                    fixRs.Rows[rNo]["uom"] = rsTbl.Rows[i]["uomnm"];
                    fixRs.Rows[rNo]["qnty"] = rsTbl.Rows[i]["qnty"].retDbl();
                    fixRs.Rows[rNo]["srate"] = rsTbl.Rows[i]["rate"].retDbl();
                    fixRs.Rows[rNo]["samt"] = rsTbl.Rows[i]["btxblval"].retDbl();
                    fixRs.Rows[rNo]["prate"] = rsTbl.Rows[i]["qnty"].retDbl() == 0 ? 0 : (salCostAmt / rsTbl.Rows[i]["qnty"].retDbl()).toRound();
                    fixRs.Rows[rNo]["pamt"] = salCostAmt;
                    Double diffamt = rsTbl.Rows[i]["btxblval"].retDbl() - salCostAmt;
                    if (rsTbl.Rows[i]["doctag"].ToString() == "SR") diffamt = (rsTbl.Rows[i]["btxblval"].retDbl() * -1) - salCostAmt;
                    fixRs.Rows[rNo]["diffamt"] = diffamt;
                    fixRs.Rows[rNo]["pblno"] = purdocno;


                    i++;
                    if (i > maxR) break;
                }

                if (party.retStr() != "")
                {
                    var temp = fixRs.Select("slcd in (" + party + ") ");
                    if (temp.Count() > 0)
                    {
                        fixRs = temp.CopyToDataTable();
                    }
                    else
                    {
                        fixRs = fixRs.Clone();
                    }
                }


                DataTable IR = new DataTable("mstrep");
                PrintViewer PV = new PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                if (reptype == "details")
                {
                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "docno", "string", "c,30", "Doc No");
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                    HC.GetPrintHeader(IR, "slcd", "string", "c,10", "Party Code");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                    HC.GetPrintHeader(IR, "ITGRPNM", "string", "c,35", "Item Group");
                    HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Cd");
                    HC.GetPrintHeader(IR, "itstyle", "string", "c,35", "Design");
                    HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                    HC.GetPrintHeader(IR, "qnty", "double", "n,12,4", "Qnty");
                    HC.GetPrintHeader(IR, "srate", "double", "n,12,4", "S.Rate");
                    HC.GetPrintHeader(IR, "samt", "double", "n,12,2", "Gross Amt");
                    HC.GetPrintHeader(IR, "prate", "double", "n,12,4", "P.Rate");
                    HC.GetPrintHeader(IR, "pamt", "double", "n,12,2", "Cost Amt");
                    HC.GetPrintHeader(IR, "diffamt", "double", "n,16,2:###,##,##,##0.00", "Profit/;Loss(-)");
                    HC.GetPrintHeader(IR, "pblno", "string", "c,16", "Purch Doc No");

                    i = 0; maxR = 0; rNo = 0;
                    maxR = fixRs.Rows.Count - 1;
                    double tqnty = 0, tsamt = 0, tpamt = 0, tdiffamt = 0;
                    while (i <= maxR)
                    {

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["docno"] = fixRs.Rows[i]["docno"];
                        IR.Rows[rNo]["docdt"] = fixRs.Rows[i]["docdt"].retDateStr();
                        IR.Rows[rNo]["slcd"] = fixRs.Rows[i]["slcd"];
                        IR.Rows[rNo]["slnm"] = fixRs.Rows[i]["slnm"];
                        IR.Rows[rNo]["ITGRPNM"] = fixRs.Rows[i]["ITGRPNM"];
                        IR.Rows[rNo]["itcd"] = fixRs.Rows[i]["itcd"];
                        IR.Rows[rNo]["itstyle"] = fixRs.Rows[i]["itstyle"];
                        IR.Rows[rNo]["uom"] = fixRs.Rows[i]["uom"];
                        IR.Rows[rNo]["qnty"] = fixRs.Rows[i]["qnty"].retDbl();
                        IR.Rows[rNo]["srate"] = fixRs.Rows[i]["srate"].retDbl();
                        IR.Rows[rNo]["samt"] = fixRs.Rows[i]["samt"].retDbl();
                        IR.Rows[rNo]["prate"] = fixRs.Rows[i]["prate"].retDbl();
                        IR.Rows[rNo]["pamt"] = fixRs.Rows[i]["pamt"].retDbl();
                        IR.Rows[rNo]["diffamt"] = fixRs.Rows[i]["diffamt"].retDbl();
                        IR.Rows[rNo]["pblno"] = fixRs.Rows[i]["pblno"];
                        IR.Rows[rNo]["celldesign"] = "diffamt=font-weight:bold;font-size:13px;";

                        tqnty = tqnty + fixRs.Rows[i]["qnty"].retDbl();
                        tsamt = tsamt + fixRs.Rows[i]["samt"].retDbl();
                        tpamt = tpamt + fixRs.Rows[i]["pamt"].retDbl();
                        tdiffamt = tdiffamt + fixRs.Rows[i]["diffamt"].retDbl();

                        i++;
                        if (i > maxR) break;
                    }


                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    IR.Rows[rNo]["slnm"] = "Grand Totals";
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                    IR.Rows[rNo]["qnty"] = tqnty;
                    IR.Rows[rNo]["samt"] = tsamt;
                    IR.Rows[rNo]["pamt"] = tpamt;
                    IR.Rows[rNo]["diffamt"] = tdiffamt;
                }
                else
                {
                    HC.RepStart(IR, 3);
                    if (reptype == "sumpar")
                    {
                        HC.GetPrintHeader(IR, "slcd", "string", "c,10", "Party Code");
                        HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "ITGRPNM", "string", "c,35", "Item Group");
                        HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Cd");
                        HC.GetPrintHeader(IR, "itstyle", "string", "c,35", "Design");
                        HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                    }
                    HC.GetPrintHeader(IR, "qnty", "double", "n,12,4", "Qnty");
                    HC.GetPrintHeader(IR, "samt", "double", "n,12,2", "Gross Amt");
                    HC.GetPrintHeader(IR, "pamt", "double", "n,12,2", "Cost Amt");
                    HC.GetPrintHeader(IR, "diffamt", "double", "n,16,2:###,##,##,##0.00", "Profit/;Loss(-)");

                    i = 0; maxR = 0; rNo = 0;
                    maxR = fixRs.Rows.Count - 1;
                    double tqnty = 0, tsamt = 0, tpamt = 0, tdiffamt = 0;
                    while (i <= maxR)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (reptype == "sumpar")
                        {
                            IR.Rows[rNo]["slcd"] = fixRs.Rows[i]["slcd"];
                            IR.Rows[rNo]["slnm"] = fixRs.Rows[i]["slnm"];
                        }
                        else {
                            IR.Rows[rNo]["ITGRPNM"] = fixRs.Rows[i]["ITGRPNM"];
                            IR.Rows[rNo]["itcd"] = fixRs.Rows[i]["itcd"];
                            IR.Rows[rNo]["itstyle"] = fixRs.Rows[i]["itstyle"];
                            IR.Rows[rNo]["uom"] = fixRs.Rows[i]["uom"];
                        }

                        string fld = reptype == "sumpar" ? "slcd" : "itcd";
                        string fldval = fixRs.Rows[i][fld].retStr();
                        double qnty = 0, samt = 0, pamt = 0, diffamt = 0;
                        while (fixRs.Rows[i][fld].retStr() == fldval)
                        {
                            qnty += fixRs.Rows[i]["qnty"].retDbl();
                            samt += fixRs.Rows[i]["samt"].retDbl();
                            pamt += fixRs.Rows[i]["pamt"].retDbl();
                            diffamt += fixRs.Rows[i]["diffamt"].retDbl();

                            tqnty = tqnty + fixRs.Rows[i]["qnty"].retDbl();
                            tsamt = tsamt + fixRs.Rows[i]["samt"].retDbl();
                            tpamt = tpamt + fixRs.Rows[i]["pamt"].retDbl();
                            tdiffamt = tdiffamt + fixRs.Rows[i]["diffamt"].retDbl();

                            i++;
                            if (i > maxR) break;
                        }
                        IR.Rows[rNo]["qnty"] = qnty;
                        IR.Rows[rNo]["samt"] = samt;
                        IR.Rows[rNo]["pamt"] = pamt;
                        IR.Rows[rNo]["diffamt"] = diffamt;
                        IR.Rows[rNo]["celldesign"] = "diffamt=font-weight:bold;font-size:13px;";

                        if (i > maxR) break;
                    }


                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    if (reptype == "sumpar")
                    {
                        IR.Rows[rNo]["slnm"] = "Grand Totals";
                    }
                    else
                    {
                        IR.Rows[rNo]["itstyle"] = "Grand Totals";
                    }
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                    IR.Rows[rNo]["qnty"] = tqnty;
                    IR.Rows[rNo]["samt"] = tsamt;
                    IR.Rows[rNo]["pamt"] = tpamt;
                    IR.Rows[rNo]["diffamt"] = tdiffamt;
                }
                string pghdr1 = "Sale Purchase Statement from " + fdt + " to " + tdt;
                string rephdr = "SalPurStmt";
                PV = HC.ShowReport(IR, rephdr, pghdr1, "", true, true, "L", false);

                TempData[rephdr] = PV;
                TempData[rephdr + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = rephdr });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}
