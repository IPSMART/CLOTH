using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_Stk_StmtController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();

        // GET: Rep_Stk_Stmt
        public ActionResult Rep_Stk_Stmt()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Stock Valuation";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //VE.DropDown_list1 = Salesfunc.Stock_Calc_Mehtod();

                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");

                    //VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection(selgrp);
                    VE.DropDown_list_ITGRP = (from i in DB.M_GROUP select new DropDown_list_ITGRP() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    //VE.DropDown_list_BRGRP = DropDownHelp.GetBrgrpcdforSelection(selgrp);
                    //VE.Brgrpnm = MasterHelp.ComboFill("brgrpcd", VE.DropDown_list_BRGRP, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_GODOWN = DropDownHelp.GetGocdforSelection();
                    VE.Gonm = MasterHelp.ComboFill("gocd", VE.DropDown_list_GODOWN, 0, 1);

                    List<DropDown_list2> drplst = new List<DropDown_list2>();
                    DropDown_list2 dropobj1 = new DropDown_list2();
                    dropobj1.value = "S";
                    dropobj1.text = "Summary";
                    drplst.Add(dropobj1);

                    DropDown_list2 dropobj2 = new DropDown_list2();
                    dropobj2.value = "D";
                    dropobj2.text = "Details";
                    drplst.Add(dropobj2);

                    DropDown_list2 dropobj3 = new DropDown_list2();
                    dropobj3.value = "G";
                    dropobj3.text = "Godown wise";
                    drplst.Add(dropobj3);
                    VE.DropDown_list2 = drplst;
                    DropDown_list2 dropobj4 = new DropDown_list2();
                    dropobj4.value = "B";
                    dropobj4.text = " Summary(Barcode)";
                    drplst.Add(dropobj4);
                    DropDown_list2 dropobj5 = new DropDown_list2();
                    dropobj5.value = "F";
                    dropobj5.text = "Stock Statement(FIFO)";
                    drplst.Add(dropobj5);
                    VE.DropDown_list2 = drplst;
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.PRCCD = "CP"; VE.PRCNM = "CP";
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
        public ActionResult GetPriceDetails(string val)
        {
            try
            {
                var str = MasterHelp.PRCCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        [HttpPost]
        public ActionResult Rep_Stk_Stmt(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                //VE.DropDown_list1 = Salesfunc.Stock_Calc_Mehtod();
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO);
                string asdt = VE.TDT.retDateStr();
                string calctype = VE.TEXTBOX2;

                //   string calmethod = (from x in VE.DropDown_list1 where x.value == calctype select x.text).SingleOrDefault();

                string selgocd = "", selbrgrpcd = "", selitcd = "", unselitcd, selitgrpcd = "", prccd = "";
                string summary = VE.TEXTBOX3; // == true?"S":"D";
                string repon = FC["repon"].ToString();
                if (FC.AllKeys.Contains("itcdvalue")) selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = CommFunc.retSqlformat(FC["itcdunselvalue"].ToString());

                if (FC.AllKeys.Contains("gocdvalue")) selgocd = CommFunc.retSqlformat(FC["gocdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                if (FC.AllKeys.Contains("brgrpcdvalue")) selbrgrpcd = CommFunc.retSqlformat(FC["brgrpcdvalue"].ToString());
                string prcd = VE.PRCCD;
                if (selgocd == "" && summary == "G")
                {
                    selgocd = string.Join(",", (from a in DBF.M_GODOWN select a.GOCD).ToList()).retSqlformat();
                }
                if (summary == "B") prccd = VE.PRCCD;


                Boolean summdtl = ((summary == "F" && repon == "D") ? false : true);
                //if (ageingperiod != 0) summdtl = false;

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("");

                Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;

                // Report begins
                //  i = 0; maxR = tbl.Rows.Count - 1;

                string pghdr1 = "";
                string repname = "Stock_Val" + System.DateTime.Now;
                double qdeci = 4;
                string qdsp = "n,16," + qdeci.ToString();
                bool ignoreitems = VE.Checkbox2;
                DataTable tbl = new DataTable();
                if (summary == "F")
                {
                    //tbl = Salesfunc.GetStockFifo("FIFO", asdt, "", "", selitgrpcd, "", selgocd, true, "", false, "", "", "", "", "CP");
                    tbl = Salesfunc.GenStocktblwithVal("FIFO", asdt, "", "", selitgrpcd, selitcd, selgocd, true, "", summdtl, "", "", "");
                }
                else
                {
                    //tbl = Salesfunc.GetStock(asdt, selgocd, "", selitcd, "FS".retSqlformat(), "", selitgrpcd, "", "CP");
                    tbl = Salesfunc.GetStock(asdt, selgocd, "", selitcd, "FS".retSqlformat(), "", selitgrpcd, "", "CP", "C001", "", "", true, false, "", "", false, false, true, "", false, "", "", VE.Checkbox7);
                }
                if (summary == "D" || (summary == "F" && repon == "D"))
                {
                    //DataTable tbl = Salesfunc.GetStock(asdt, selgocd, "", selitcd, "FS".retSqlformat(), "", selitgrpcd, "", "CP");
                    return Details(FC, VE, tbl, COM, LOC, asdt, prccd, qdsp, summary);
                }
                else if (summary == "S" || (summary == "F" && repon == "S"))
                {
                    //DataTable tbl = Salesfunc.GetStock(asdt, selgocd, "", selitcd, "FS".retSqlformat(), "", selitgrpcd, "", "CP");
                    return Summary(FC, VE, tbl, COM, LOC, asdt, prccd, qdsp, ignoreitems, summary);
                }
                else if (summary == "G")
                {
                    //DataTable tbl = Salesfunc.GetStock(asdt, selgocd, "", selitcd, "FS".retSqlformat(), "", selitgrpcd, "", "CP");
                    return Godownwise(FC, VE, tbl, COM, LOC, asdt, prccd, qdsp);
                }
                else if (summary == "B")
                {
                    return SummaryWise_Barcode(FC, VE, COM, LOC, asdt, prcd, selitgrpcd, selgocd, selitcd);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult Details(FormCollection FC, ReportViewinHtml VE, DataTable tbl, string COM, string LOC, string ASDT, string PRCCD, string QDSP, string summary)
        {
            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();
            DataTable IR = new DataTable("");
            Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;

            HC.RepStart(IR);
            HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
            HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
            HC.GetPrintHeader(IR, "slcd", "string", "c,8", "slcd");
            HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Name of Party");
            HC.GetPrintHeader(IR, "qnty", "double", QDSP, "Bal.Qnty");
            HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Av.Rate");
            HC.GetPrintHeader(IR, "amt", "double", "n,14,2", "Stock Value");

            maxR = tbl.Rows.Count - 1;

            string strbrgrpcd = "", stritcd = "";
            double gamt = 0, gqnty = 0;
            i = 0;
            while (i <= maxR)
            {
                strbrgrpcd = tbl.Rows[i]["itgrpcd"].ToString();

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["itgrpnm"].ToString();
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                double bamt = 0, tqnty = 0;
                while (tbl.Rows[i]["itgrpcd"].ToString() == strbrgrpcd)
                {
                    stritcd = tbl.Rows[i]["itcd"].ToString();
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + stritcd + "  " + " </span>" + tbl.Rows[i]["itstyle"].ToString();
                    IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + " </span>" + " [" + tbl.Rows[i]["uomcd"] + "]";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    double iqnty = 0, iamt = 0;
                    while (tbl.Rows[i]["itcd"].ToString() == stritcd)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"];
                        IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().retDateStr();
                        IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].ToString();
                        IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                        IR.Rows[rNo]["qnty"] = tbl.Rows[i]["balqnty"].ToString();
                        IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                        IR.Rows[rNo]["amt"] = (tbl.Rows[i]["rate"].retDbl() * tbl.Rows[i]["balqnty"].retDbl()).retDbl();
                        iqnty = iqnty + Convert.ToDouble(tbl.Rows[i]["balqnty"].retDbl());
                        iamt = iamt + (tbl.Rows[i]["rate"].retDbl() * tbl.Rows[i]["balqnty"].retDbl()).retDbl();
                        i++;
                        if (i > maxR) break;
                    }
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["slnm"] = "Total of " + tbl.Rows[i - 1]["itstyle"].ToString();
                    IR.Rows[rNo]["qnty"] = iqnty;
                    IR.Rows[rNo]["amt"] = iamt;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

                    bamt = bamt + iamt;
                    tqnty = tqnty + iqnty;
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["slnm"] = "Total of " + tbl.Rows[i - 1]["itgrpnm"].ToString();
                IR.Rows[rNo]["amt"] = bamt;
                //IR.Rows[rNo]["qnty"] = tqnty;
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                gamt = gamt + bamt;
                gqnty = gqnty + tqnty;
                //i++;
                if (i > maxR) break;
            }
            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
            IR.Rows[rNo]["slnm"] = "Grand Total";
            IR.Rows[rNo]["amt"] = gamt;
            IR.Rows[rNo]["qnty"] = gqnty;
            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

            string pghdr1 = "";
            string repname = "Stock_Val" + System.DateTime.Now;
            pghdr1 = summary == "F" ? "Stock Valuation(FIFO)[Detail] as on " + ASDT : "Stock Valuation[Detail] as on " + ASDT;
            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

            TempData[repname] = PV;
            TempData[repname + "xxx"] = IR;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        }
        public ActionResult Summary(FormCollection FC, ReportViewinHtml VE, DataTable tbl, string COM, string LOC, string ASDT, string PRCCD, string QDSP, bool ignoreitems, string summary)
        {
            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();
            DataTable IR = new DataTable("");
            Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;

            string days_aging = VE.TEXTBOX5; // Days aging value
            string days1 = "0", days2 = "0", days3 = "0";
            if (days_aging == null)
            {
            }
            else if (days_aging == "1")
            {
                days1 = FC["days1"];
            }
            else if (days_aging == "2")
            {
                days1 = FC["days1"];
                days2 = FC["days2"];
            }
            else if (days_aging == "3")
            {
                days1 = FC["days1"];
                days2 = FC["days2"];
                days3 = FC["days3"];
            }

            long duedays1, duedays2, duedays3, duedays4, duedays5 = 0;
            long ageingperiod = 0;
            duedays1 = Convert.ToInt64(days1); duedays2 = Convert.ToInt64(days2); duedays3 = Convert.ToInt64(days3);
            ageingperiod = Convert.ToInt64(days_aging);

            long due1fDys = 0, due1tDys = 0, due2fDys = 0, due2tDys = 0, due3fDys = 0, due3tDys = 0, due4fDys = 0, due4tDys = 0;
            if (ageingperiod != 0)
            {
                ageingperiod = ageingperiod + 1;

                if (ageingperiod >= 1) due1fDys = 0; due1tDys = duedays1;
                if (ageingperiod >= 2) due2fDys = due1tDys + 1; due2tDys = duedays2;
                if (ageingperiod >= 3) due3fDys = due2tDys + 1; due3tDys = duedays3;
                if (ageingperiod >= 4) due4fDys = due3tDys + 1; due4tDys = 0;

                //define last ageing column
                if (ageingperiod == 2) due2tDys = 99999;
                if (ageingperiod == 3) due3tDys = 99999;
                if (ageingperiod == 4) due4tDys = 99999;
            }
            double gdue1Amt = 0, gdue2Amt = 0, gdue3Amt = 0, gdue4Amt = 0;
            double gdue1Qty = 0, gdue2Qty = 0, gdue3Qty = 0, gdue4Qty = 0;
            double bdue1Amt = 0, bdue2Amt = 0, bdue3Amt = 0, bdue4Amt = 0;
            double bdue1Qty = 0, bdue2Qty = 0, bdue3Qty = 0, bdue4Qty = 0;
            double due1Amt = 0, due2Amt = 0, due3Amt = 0, due4Amt = 0;
            double due1Qty = 0, due2Qty = 0, due3Qty = 0, due4Qty = 0;


            HC.RepStart(IR, 3);
            HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
            HC.GetPrintHeader(IR, "itcd", "string", "c,10", "itcd");
            HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
            HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
            HC.GetPrintHeader(IR, "qnty", "double", QDSP, "Stk.Qnty");
            HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Av.Rate");
            HC.GetPrintHeader(IR, "amt", "double", "n,14,2", "Stock Value");
            if (ageingperiod >= 1) HC.GetPrintHeader(IR, "stk1qty", "double", "n,14,3", "<= " + due1tDys.ToString() + ";Qty");
            if (ageingperiod >= 1) HC.GetPrintHeader(IR, "stk1amt", "double", "n,14,2", "<= " + due1tDys.ToString() + ";Amt");
            if (ageingperiod >= 2) HC.GetPrintHeader(IR, "stk2qty", "double", "n,14,3", due2fDys.ToString() + " to " + due2tDys.ToString() + ";Qty");
            if (ageingperiod >= 2) HC.GetPrintHeader(IR, "stk2amt", "double", "n,14,2", due2fDys.ToString() + " to " + due2tDys.ToString() + ";Amt");
            if (ageingperiod >= 3) HC.GetPrintHeader(IR, "stk3qty", "double", "n,14,3", due3fDys.ToString() + " to " + due3tDys.ToString() + ";Qty");
            if (ageingperiod >= 3) HC.GetPrintHeader(IR, "stk3amt", "double", "n,14,2", due3fDys.ToString() + " to " + due3tDys.ToString() + ";Amt");
            if (ageingperiod >= 4) HC.GetPrintHeader(IR, "stk4qty", "double", "n,14,3", "> " + due3tDys.ToString() + ";Qty");
            if (ageingperiod >= 4) HC.GetPrintHeader(IR, "stk4amt", "double", "n,14,2", "> " + due3tDys.ToString() + ";Amt");
            maxR = tbl.Rows.Count - 1;
            DataView dv = new DataView(tbl);
            dv.Sort = "itgrpcd, itcd ASC";
            tbl = dv.ToTable();

            string strbrgrpcd = "", stritcd = "";
            double gamt = 0, gqnty = 0;
            i = 0;
            Int32 islno = 0;
            gdue1Amt = 0; gdue2Amt = 0; gdue3Amt = 0; gdue4Amt = 0;
            gdue1Qty = 0; gdue2Qty = 0; gdue3Qty = 0; gdue4Qty = 0;
            while (i <= maxR)
            {
                strbrgrpcd = tbl.Rows[i]["itgrpcd"].ToString();
                if (ignoreitems == false)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["itgrpnm"].ToString();
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                }

                double bamt = 0, bqnty = 0;
                if (ignoreitems == false) islno = 0;
                bdue1Amt = 0; bdue2Amt = 0; bdue3Amt = 0; bdue4Amt = 0;
                bdue1Qty = 0; bdue2Qty = 0; bdue3Qty = 0; bdue4Qty = 0;
                while (tbl.Rows[i]["itgrpcd"].ToString() == strbrgrpcd)
                {
                    stritcd = tbl.Rows[i]["itcd"].ToString();
                    double iqnty = 0, iamt = 0;
                    due1Amt = 0; due2Amt = 0; due3Amt = 0; due4Amt = 0;
                    due1Qty = 0; due2Qty = 0; due3Qty = 0; due4Qty = 0;
                    while (tbl.Rows[i]["itcd"].ToString() == stritcd)
                    {
                        double days = 0;
                        TimeSpan TSdys;
                        if (tbl.Rows[i]["docdt"] == DBNull.Value) days = 0;
                        else
                        {
                            TSdys = Convert.ToDateTime(ASDT) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
                            days = TSdys.Days;
                        }

                        double _qty = Convert.ToDouble(tbl.Rows[i]["balqnty"].ToString()), _amt = (tbl.Rows[i]["rate"].retDbl() * tbl.Rows[i]["balqnty"].retDbl()).retDbl();
                        if (ageingperiod > 0)
                        {
                            if (days <= due1tDys && due1tDys != 0) { due1Qty = due1Qty + _qty; due1Amt = due1Amt + _amt; }
                            else if (days <= due2tDys && due2tDys != 0) { due2Qty = due2Qty + _qty; due2Amt = due2Amt + _amt; }
                            else if (days <= due3tDys && due3tDys != 0) { due3Qty = due3Qty + _qty; due3Amt = due3Amt + _amt; }
                            else { due4Qty = due4Qty + _qty; due4Amt = due4Amt + _amt; }
                        }
                        iqnty = iqnty + _qty;
                        iamt = iamt + _amt;
                        i++;
                        if (i > maxR) break;
                    }
                    if (Math.Round(iqnty, 6) != 0)
                    {
                        double avrt = 0;
                        if (iqnty != 0) avrt = iamt / iqnty;
                        if (ignoreitems == false)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            islno++;
                            IR.Rows[rNo]["slno"] = islno;
                            IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itcd"].ToString();
                            IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itstyle"].ToString();
                            IR.Rows[rNo]["uomnm"] = tbl.Rows[i - 1]["uomcd"].ToString();
                            IR.Rows[rNo]["qnty"] = iqnty;
                            IR.Rows[rNo]["rate"] = avrt;
                            IR.Rows[rNo]["amt"] = iamt;

                            if (ageingperiod > 0)
                            {
                                if (due1Qty != 0) { IR.Rows[rNo]["stk1qty"] = due1Qty; IR.Rows[rNo]["stk1amt"] = due1Amt; }
                                if (due2Qty != 0) { IR.Rows[rNo]["stk2qty"] = due2Qty; IR.Rows[rNo]["stk2amt"] = due2Amt; }
                                if (due3Qty != 0) { IR.Rows[rNo]["stk3qty"] = due3Qty; IR.Rows[rNo]["stk3amt"] = due3Amt; }
                                if (due4Qty != 0) { IR.Rows[rNo]["stk4qty"] = due4Qty; IR.Rows[rNo]["stk4amt"] = due4Amt; }
                            }
                        }
                        bdue1Qty = bdue1Qty + due1Qty; bdue1Amt = bdue1Amt + due1Amt;
                        bdue2Qty = bdue2Qty + due2Qty; bdue2Amt = bdue2Amt + due2Amt;
                        bdue3Qty = bdue3Qty + due3Qty; bdue3Amt = bdue3Amt + due3Amt;
                        bdue4Qty = bdue4Qty + due4Qty; bdue4Amt = bdue4Amt + due4Amt;
                        bamt = bamt + iamt;
                        bqnty = bqnty + iqnty;
                    }

                    if (i > maxR) break;
                }
                if (ignoreitems == false)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["itgrpnm"].ToString();
                    IR.Rows[rNo]["amt"] = bamt;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                }
                else
                {
                    if (bqnty != 0)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        islno++;
                        IR.Rows[rNo]["slno"] = islno;
                        IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itgrpcd"].ToString();
                        IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itgrpnm"].ToString();
                        IR.Rows[rNo]["uomnm"] = tbl.Rows[i - 1]["uomcd"].ToString();
                        IR.Rows[rNo]["qnty"] = bqnty;
                        IR.Rows[rNo]["amt"] = bamt;
                    }
                }
                if (ageingperiod > 0)
                {
                    if (bdue1Qty != 0) { IR.Rows[rNo]["stk1qty"] = bdue1Qty; IR.Rows[rNo]["stk1amt"] = bdue1Amt; gdue1Qty = gdue1Qty + bdue1Qty; gdue1Amt = gdue1Amt + bdue1Amt; }
                    if (bdue2Qty != 0) { IR.Rows[rNo]["stk2qty"] = bdue2Qty; IR.Rows[rNo]["stk2amt"] = bdue2Amt; gdue2Qty = gdue2Qty + bdue2Qty; gdue2Amt = gdue2Amt + bdue2Amt; }
                    if (bdue3Qty != 0) { IR.Rows[rNo]["stk3qty"] = bdue3Qty; IR.Rows[rNo]["stk3amt"] = bdue3Amt; gdue3Qty = gdue3Qty + bdue3Qty; gdue3Amt = gdue3Amt + bdue3Amt; }
                    if (bdue4Qty != 0) { IR.Rows[rNo]["stk4qty"] = bdue4Qty; IR.Rows[rNo]["stk4amt"] = bdue4Amt; gdue4Qty = gdue4Qty + bdue4Qty; gdue4Amt = gdue4Amt + bdue4Amt; }
                }
                gamt = gamt + bamt;
                gqnty = gqnty + bqnty;
                //i++;
                if (i > maxR) break;
            }
            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
            IR.Rows[rNo]["itnm"] = "Grand Total";
            IR.Rows[rNo]["amt"] = gamt;
            IR.Rows[rNo]["qnty"] = gqnty;
            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
            if (ageingperiod > 0)
            {
                if (gdue1Qty != 0) { IR.Rows[rNo]["stk1amt"] = gdue1Amt; }
                if (gdue2Qty != 0) { IR.Rows[rNo]["stk2amt"] = gdue2Amt; }
                if (gdue3Qty != 0) { IR.Rows[rNo]["stk3amt"] = gdue3Amt; }
                if (gdue4Qty != 0) { IR.Rows[rNo]["stk4amt"] = gdue4Amt; }
            }

            string pghdr1 = "";
            string repname = "Stock_Val" + System.DateTime.Now;
            pghdr1 = summary == "F" ? "Stock Valuation(FIFO)[Summary] as on " + ASDT : "Stock Valuation[Summary] as on " + ASDT;
            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

            TempData[repname] = PV;
            TempData[repname + "xxx"] = IR;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        }
        public ActionResult Godownwise(FormCollection FC, ReportViewinHtml VE, DataTable tbl, string COM, string LOC, string ASDT, string PRCCD, string QDSP)
        {
            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();
            DataTable IR = new DataTable("");
            Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;


            DataView dv = new DataView(tbl);
            dv.Sort = "itcd, gocd asc";
            tbl = dv.ToTable();
            DataTable amtDT = new DataTable("goDT");
            string[] amtTBLCOLS = new string[] { "gocd", "gonm" };
            amtDT = tbl.DefaultView.ToTable(true, amtTBLCOLS);
            amtDT.Columns.Add("goqty", typeof(double));

            HC.RepStart(IR, 3);
            HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
            HC.GetPrintHeader(IR, "itcd", "string", "c,10", "itcd");
            HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
            HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
            foreach (DataRow dr in amtDT.Rows)
            {
                HC.GetPrintHeader(IR, dr["gocd"].ToString(), "double", "n,14,3", dr["gonm"].ToString());
            }
            HC.GetPrintHeader(IR, "totalqnty", "double", "n,14,3", "Total Qty");
            maxR = tbl.Rows.Count - 1;

            string strbrgrpcd = "", stritcd = "", gocd = "";
            double gamt = 0, gqnty = 0;
            i = 0;
            Int32 islno = 0;
            double gdue1Amt = 0, gdue2Amt = 0, gdue3Amt = 0, gdue4Amt = 0;
            double gdue1Qty = 0, gdue2Qty = 0, gdue3Qty = 0, gdue4Qty = 0;
            double bdue1Qty = 0, bdue2Qty = 0, bdue3Qty = 0, bdue4Qty = 0;
            double due1Amt = 0, due2Amt = 0, due3Amt = 0, due4Amt = 0;
            double due1Qty = 0, due2Qty = 0, due3Qty = 0, due4Qty = 0;
            double bdue1Amt = 0, bdue2Amt = 0, bdue3Amt = 0, bdue4Amt = 0;
            double _qty = 0, _amt = 0;
            while (i <= maxR)
            {
                strbrgrpcd = tbl.Rows[i]["itgrpcd"].ToString();

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["itgrpnm"].ToString();
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                foreach (DataRow amtdr in amtDT.Rows) amtdr["goqty"] = 0;
                double bamt = 0, bqnty = 0;

                bdue1Amt = 0; bdue2Amt = 0; bdue3Amt = 0; bdue4Amt = 0;
                bdue1Qty = 0; bdue2Qty = 0; bdue3Qty = 0; bdue4Qty = 0; double tqty = 0, tqty_ = 0;
                while (tbl.Rows[i]["itgrpcd"].ToString() == strbrgrpcd)
                {
                    stritcd = tbl.Rows[i]["itcd"].ToString();
                    double iqnty = 0, iamt = 0;
                    due1Amt = 0; due2Amt = 0; due3Amt = 0; due4Amt = 0;
                    due1Qty = 0; due2Qty = 0; due3Qty = 0; due4Qty = 0;
                    tqty_ = 0;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    islno++;
                    IR.Rows[rNo]["slno"] = islno;
                    IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"].ToString();
                    IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itstyle"].ToString();
                    IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomcd"].ToString();

                    while (tbl.Rows[i]["itcd"].ToString() == stritcd)
                    {
                        double avrt = 0;
                        if (iqnty != 0) avrt = iamt / iqnty;

                        gocd = tbl.Rows[i]["gocd"].ToString();

                        foreach (DataRow amtdr in amtDT.Rows)
                        {
                            if (gocd == amtdr["gocd"].retStr())
                            {
                                amtdr["goqty"] = amtdr["goqty"].retDbl() + tbl.Rows[i]["balqnty"].retDbl();
                                tqty_ += tbl.Rows[i]["balqnty"].retDbl();
                                //IR.Rows[rNo][amtdr["gocd"].ToString()] = tbl.Rows[i]["balqnty"].retDbl();
                                IR.Rows[rNo][amtdr["gocd"].ToString()] = tqty_;

                            }
                        }
                        i++;
                        if (i > maxR) break;
                    }
                    IR.Rows[rNo]["totalqnty"] = tqty_.retDbl();
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                tqty = 0;
                IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["itgrpnm"].ToString();
                foreach (DataRow amtdr in amtDT.Rows)
                {
                    IR.Rows[rNo][amtdr["gocd"].ToString()] = amtdr["goqty"].retDbl();
                    tqty += amtdr["goqty"].retDbl();

                }
                //tqty += tqty_.retDbl();
                IR.Rows[rNo]["totalqnty"] = tqty;
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

                if (i > maxR) break;
            }


            string pghdr1 = "";
            string repname = "Stock_Val" + System.DateTime.Now;

            pghdr1 = "Stock Valuation(Godown Wise) as on " + ASDT;
            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

            TempData[repname] = PV;
            TempData[repname + "xxx"] = IR;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        }
        public ActionResult SummaryWise_Barcode(FormCollection FC, ReportViewinHtml VE, string COM, string LOC, string ASDT, string PRCCD, string ITGRPCD, string GOCD, string ITCD)
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string fdt = CommVar.FinStartDate(UNQSNO);

            string query = "select a.barno, e.itcd, e.fabitcd, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
            query += "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(c.rate, 0) clrate, ";
            query += "f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm,e.hsncode   from ";

            query += "(select a.barno, 'OP' doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            query += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            query += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            query += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            query += "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            query += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and ";
            query += "d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            if (GOCD.retStr() != "") query += "and a.gocd in (" + GOCD + ") ";
            query += "group by a.barno, 'OP' ";
            query += "union all ";
            query += "select a.barno, c.doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            query += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            query += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            query += "    from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            query += "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            query += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and ";
            query += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + ASDT + "', 'dd/mm/yyyy') ";
            if (GOCD.retStr() != "") query += "and a.gocd in (" + GOCD + ") ";
            query += "group by a.barno, c.doctag ) a, ";

            query += "(select barno, effdt, prccd, rate from ( ";
            query += "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            query += "from " + scm + ".t_batchmst_price a ";
            query += "where a.effdt < to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = '" + PRCCD + "' ) where rn = 1) b, ";

            query += "(select barno, effdt, prccd, rate from ( ";
            query += "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            query += "from " + scm + ".t_batchmst_price a ";
            query += "where a.effdt <= to_date('" + ASDT + "', 'dd/mm/yyyy') and a.prccd = '" + PRCCD + "' ) where rn = 1) c, ";

            query += "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i ";
            query += "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
            query += "a.barno = b.barno(+) and a.barno = c.barno(+) and ";
            query += "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) ";
            if (ITGRPCD.retStr() != "") query += "and f.itgrpcd in (" + ITGRPCD + ") ";
            if (ITCD.retStr() != "") query += "and e.itcd in (" + ITCD + ") ";
            query += "order by itgrpnm, itgrpcd, fabitnm, fabitcd, itnm, itcd, styleno, barno ";
            DataTable tbl1 = MasterHelp.SQLquery(query);
            if (tbl1.Rows.Count == 0) return Content("no records..");


            Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;
            maxR = tbl1.Rows.Count - 1;

            string strbrgrpcd = "";

            i = 0;
            Int32 islno = 0;

            #region Generate Temp Data Table

            DataTable summarybarcode = new DataTable("SUMMBARCODE");
            summarybarcode.Columns.Add("key", typeof(string), "");
            summarybarcode.Columns.Add("itgrpcd", typeof(string), "");
            summarybarcode.Columns.Add("itgrpnm", typeof(string), "");
            summarybarcode.Columns.Add("fabitcd", typeof(string), "");
            summarybarcode.Columns.Add("fabitnm", typeof(string), "");
            summarybarcode.Columns.Add("itnm", typeof(string), "");
            summarybarcode.Columns.Add("itcd", typeof(string), "");
            summarybarcode.Columns.Add("styleno", typeof(string), "");
            summarybarcode.Columns.Add("barno", typeof(string), "");
            summarybarcode.Columns.Add("uomcd", typeof(string), "");
            summarybarcode.Columns.Add("uomnm", typeof(string), "");
            summarybarcode.Columns.Add("qnty", typeof(double), "");
            summarybarcode.Columns.Add("txblval", typeof(double), "");
            summarybarcode.Columns.Add("opqty", typeof(double), "");
            summarybarcode.Columns.Add("opval", typeof(double), "");
            summarybarcode.Columns.Add("netpur", typeof(double), "");
            summarybarcode.Columns.Add("purval", typeof(double), "");
            summarybarcode.Columns.Add("karqty", typeof(double), "");
            summarybarcode.Columns.Add("karval", typeof(double), "");
            summarybarcode.Columns.Add("netsale", typeof(double), "");
            summarybarcode.Columns.Add("salevalue", typeof(double), "");
            summarybarcode.Columns.Add("approval", typeof(double), "");
            summarybarcode.Columns.Add("netstktrans", typeof(double), "");
            summarybarcode.Columns.Add("netadj", typeof(double), "");
            summarybarcode.Columns.Add("balqty", typeof(double), "");
            summarybarcode.Columns.Add("balval", typeof(double), "");
            summarybarcode.Columns.Add("itfabitcd", typeof(string), "");

            while (i <= maxR)
            {
                string keyval = tbl1.Rows[i]["uomcd"].retStr() + tbl1.Rows[i]["itgrpcd"].retStr() + tbl1.Rows[i]["fabitcd"].retStr() + tbl1.Rows[i]["itcd"].retStr() + tbl1.Rows[i]["styleno"].retStr() + tbl1.Rows[i]["barno"].retStr();

                //calculation
                double opqty = 0, opval = 0, netpur = 0, purval = 0, karqty = 0, karval = 0, netsale = 0, salevalue = 0, approval = 0, netstktrans = 0, netadj = 0, balqty = 0, balval = 0;

                if (tbl1.Rows[i]["doctag"].retStr() == "OP")
                {
                    opqty = tbl1.Rows[i]["doctag"].retStr() == "OP" ? tbl1.Rows[i]["qnty"].retDbl() : 0;
                    opval = (tbl1.Rows[i]["txblval"].retDbl() == 0 ? (opqty.retDbl() * (tbl1.Rows[i]["oprate"].retDbl() + tbl1.Rows[i]["othrate"].retDbl())).toRound(2) : (tbl1.Rows[i]["txblval"].retDbl()));
                }

                netpur = (tbl1.Rows[i]["doctag"].retStr() == "PR") || (tbl1.Rows[i]["doctag"].retStr() == "PB") ? tbl1.Rows[i]["qnty"].retDbl() : 0;
                purval = (tbl1.Rows[i]["doctag"].retStr() == "PR") || (tbl1.Rows[i]["doctag"].retStr() == "PB") ? tbl1.Rows[i]["txblval"].retDbl() : 0;

                karqty = (tbl1.Rows[i]["doctag"].retStr() == "KR") || (tbl1.Rows[i]["doctag"].retStr() == "KI") ? tbl1.Rows[i]["qnty"].retDbl() : 0;
                karval = (tbl1.Rows[i]["doctag"].retStr() == "KR") || (tbl1.Rows[i]["doctag"].retStr() == "KI") ? tbl1.Rows[i]["txblval"].retDbl() : 0;

                netsale = (tbl1.Rows[i]["doctag"].retStr() == "SR") || (tbl1.Rows[i]["doctag"].retStr() == "SB") ? tbl1.Rows[i]["qnty"].retDbl() * (-1) : 0;
                salevalue = (tbl1.Rows[i]["doctag"].retStr() == "SR") || (tbl1.Rows[i]["doctag"].retStr() == "SB") ? tbl1.Rows[i]["txblval"].retDbl() * (-1) : 0;

                approval = tbl1.Rows[i]["doctag"].retStr() == "AP" ? tbl1.Rows[i]["qnty"].retDbl() * (-1) : 0;

                netstktrans = tbl1.Rows[i]["doctag"].retStr() == "ST" ? tbl1.Rows[i]["qnty"].retDbl() : 0;

                netadj = (tbl1.Rows[i]["doctag"].retStr() == "SC") || (tbl1.Rows[i]["doctag"].retStr() == "SA") ? tbl1.Rows[i]["qnty"].retDbl() : 0;

                balqty = opqty.retDbl() + netpur.retDbl() + karqty.retDbl() - netsale.retDbl() - approval.retDbl() + netstktrans.retDbl() + netadj.retDbl();

                balval = (balqty.retDbl() * (tbl1.Rows[i]["clrate"].retDbl() + tbl1.Rows[i]["othrate"].retDbl())).toRound(2);
                //
                DataRow existdr = null;
                if (summarybarcode != null && summarybarcode.Rows.Count > 0)
                {
                    existdr = summarybarcode.Select("key ='" + keyval + "'").FirstOrDefault();
                }
                if (existdr != null)//if exist then update
                {
                    existdr["qnty"] = existdr["qnty"].retDbl() + tbl1.Rows[i]["qnty"].retDbl();
                    existdr["txblval"] = existdr["txblval"].retDbl() + tbl1.Rows[i]["txblval"].retDbl();

                    existdr["opqty"] = existdr["opqty"].retDbl() + opqty;
                    existdr["opval"] = existdr["opval"].retDbl() + opval;

                    existdr["netpur"] = existdr["netpur"].retDbl() + netpur;
                    existdr["purval"] = existdr["purval"].retDbl() + purval;

                    existdr["karqty"] = existdr["karqty"].retDbl() + karqty;
                    existdr["karval"] = existdr["karval"].retDbl() + karval;

                    existdr["netsale"] = existdr["netsale"].retDbl() + netsale;
                    existdr["salevalue"] = existdr["salevalue"].retDbl() + salevalue;

                    existdr["approval"] = existdr["approval"].retDbl() + approval;

                    existdr["netstktrans"] = existdr["netstktrans"].retDbl() + netstktrans;

                    existdr["netadj"] = existdr["netadj"].retDbl() + netadj;

                    existdr["balqty"] = existdr["balqty"].retDbl() + balqty;
                    existdr["balval"] = existdr["balval"].retDbl() + balval;
                }
                else//new row add
                {
                    summarybarcode.Rows.Add(""); rNo = summarybarcode.Rows.Count - 1;
                    summarybarcode.Rows[rNo]["key"] = keyval;
                    summarybarcode.Rows[rNo]["itgrpcd"] = tbl1.Rows[i]["itgrpcd"].retStr();
                    summarybarcode.Rows[rNo]["itgrpnm"] = tbl1.Rows[i]["itgrpnm"].retStr();
                    summarybarcode.Rows[rNo]["fabitcd"] = tbl1.Rows[i]["fabitcd"].retStr();
                    summarybarcode.Rows[rNo]["fabitnm"] = tbl1.Rows[i]["fabitnm"].retStr();
                    summarybarcode.Rows[rNo]["itgrpnm"] = tbl1.Rows[i]["itgrpnm"].retStr();
                    summarybarcode.Rows[rNo]["itnm"] = tbl1.Rows[i]["itnm"].retStr();
                    summarybarcode.Rows[rNo]["itcd"] = tbl1.Rows[i]["itcd"].retStr();
                    summarybarcode.Rows[rNo]["styleno"] = tbl1.Rows[i]["styleno"].retStr();
                    summarybarcode.Rows[rNo]["barno"] = tbl1.Rows[i]["barno"].retStr();
                    summarybarcode.Rows[rNo]["uomcd"] = tbl1.Rows[i]["uomcd"].retStr();
                    summarybarcode.Rows[rNo]["uomnm"] = tbl1.Rows[i]["uomnm"].retStr();
                    summarybarcode.Rows[rNo]["itfabitcd"] = VE.Checkbox8==true? tbl1.Rows[i]["itcd"].retStr() + tbl1.Rows[i]["fabitcd"].retStr()+ tbl1.Rows[i]["hsncode"].retStr(): tbl1.Rows[i]["itcd"].retStr() + tbl1.Rows[i]["fabitcd"].retStr();
                    summarybarcode.Rows[rNo]["qnty"] = tbl1.Rows[i]["qnty"].retDbl();
                    summarybarcode.Rows[rNo]["txblval"] = tbl1.Rows[i]["txblval"].retDbl();

                    summarybarcode.Rows[rNo]["opqty"] = opqty;
                    summarybarcode.Rows[rNo]["opval"] = opval;

                    summarybarcode.Rows[rNo]["netpur"] = netpur;
                    summarybarcode.Rows[rNo]["purval"] = purval;

                    summarybarcode.Rows[rNo]["karqty"] = karqty;
                    summarybarcode.Rows[rNo]["karval"] = karval;

                    summarybarcode.Rows[rNo]["netsale"] = netsale;
                    summarybarcode.Rows[rNo]["salevalue"] = salevalue;

                    summarybarcode.Rows[rNo]["approval"] = approval;

                    summarybarcode.Rows[rNo]["netstktrans"] = netstktrans;

                    summarybarcode.Rows[rNo]["netadj"] = netadj;

                    summarybarcode.Rows[rNo]["balqty"] = balqty;
                    summarybarcode.Rows[rNo]["balval"] = balval;
                }
                i++;
                if (i > maxR) break;
            }
            summarybarcode.DefaultView.Sort = "itgrpcd,itcd,fabitcd,styleno,barno";
            summarybarcode = summarybarcode.DefaultView.ToTable();
            #endregion
            string chkfld1 = "", chkval1 = "", chkfld2 = "", chkval2 = "";
            chkfld1 = VE.Checkbox3 == true ? "styleno" : "itfabitcd";
            chkfld2 = VE.Checkbox4 == true ? "barno" : "itfabitcd";

            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();
            DataTable IR = new DataTable("");

            HC.RepStart(IR, 3);
            HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
            if (VE.Checkbox4 == true) HC.GetPrintHeader(IR, "barno", "string", "c,10", "Bar No.");
            HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
            if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "styleno", "string", "c,40", "Style No.");
            HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
            HC.GetPrintHeader(IR, "opqty", "double", "n,16,3", "OP.Qnty");
            HC.GetPrintHeader(IR, "opval", "double", "n,10,2", "OP.Value");
            HC.GetPrintHeader(IR, "netpur", "double", "n,14,2", "Net Pur");
            HC.GetPrintHeader(IR, "purval", "double", "n,14,2", "Purch Value");
            HC.GetPrintHeader(IR, "karqty", "double", "n,14,2", "Kar Qnty");
            HC.GetPrintHeader(IR, "karval", "double", "n,14,2", "Kar Value");
            HC.GetPrintHeader(IR, "netsale", "double", "n,14,2", "Net Sale");
            HC.GetPrintHeader(IR, "salevalue", "double", "n,14,2", "Sale Value");
            HC.GetPrintHeader(IR, "approval", "double", "n,14,2", "Approval");
            HC.GetPrintHeader(IR, "netstktrans", "double", "n,14,2", "Net Stk.Trnf");
            HC.GetPrintHeader(IR, "netadj", "double", "n,14,2", "Net Adj");
            HC.GetPrintHeader(IR, "balqty", "double", "n,14,2", "Bal Qnty");
            HC.GetPrintHeader(IR, "balval", "double", "n,14,2", "Bal Value");
            IR.Columns.Add("itgrpcd", typeof(string), "");

            maxB = summarybarcode.Rows.Count - 1;
            i = 0;
            while (i <= maxB)
            {
                strbrgrpcd = summarybarcode.Rows[i]["itgrpcd"].retStr();

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + summarybarcode.Rows[i]["itgrpnm"].ToString();
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                while (summarybarcode.Rows[i]["itgrpcd"].retStr() == strbrgrpcd)
                {
                    string itcdfabitcd = summarybarcode.Rows[i]["itfabitcd"].retStr();
                    while (summarybarcode.Rows[i]["itgrpcd"].retStr() == strbrgrpcd && itcdfabitcd == summarybarcode.Rows[i]["itfabitcd"].retStr())
                    {
                        chkval1 = summarybarcode.Rows[i][chkfld1].ToString();
                        while (summarybarcode.Rows[i]["itgrpcd"].retStr() == strbrgrpcd && itcdfabitcd == summarybarcode.Rows[i]["itfabitcd"].retStr() && chkval1 == summarybarcode.Rows[i][chkfld1].ToString())
                        {
                            chkval2 = summarybarcode.Rows[i][chkfld2].ToString();
                            double opqty = 0, opval = 0, netpur = 0, purval = 0, karqty = 0, karval = 0, netsale = 0, salevalue = 0, approval = 0, netstktrans = 0, netadj = 0, balqty = 0, balval = 0;

                            while (summarybarcode.Rows[i]["itgrpcd"].retStr() == strbrgrpcd && itcdfabitcd == summarybarcode.Rows[i]["itfabitcd"].retStr() && chkval1 == summarybarcode.Rows[i][chkfld1].ToString() && chkval2 == summarybarcode.Rows[i][chkfld2].ToString())
                            {

                                opqty += summarybarcode.Rows[i]["opqty"].retDbl();
                                opval += summarybarcode.Rows[i]["opval"].retDbl();
                                netpur += summarybarcode.Rows[i]["netpur"].retDbl();
                                purval += summarybarcode.Rows[i]["purval"].retDbl();
                                karqty += summarybarcode.Rows[i]["karqty"].retDbl();
                                karval += summarybarcode.Rows[i]["karval"].retDbl();
                                netsale += summarybarcode.Rows[i]["netsale"].retDbl();
                                salevalue += summarybarcode.Rows[i]["salevalue"].retDbl();
                                approval += summarybarcode.Rows[i]["approval"].retDbl();
                                netstktrans += summarybarcode.Rows[i]["netstktrans"].retDbl();
                                netadj += summarybarcode.Rows[i]["netadj"].retDbl();
                                balqty += summarybarcode.Rows[i]["balqty"].retDbl();
                                balval += summarybarcode.Rows[i]["balval"].retDbl();
                                i++;
                                if (i > maxB) break;
                            }
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            islno++;
                            IR.Rows[rNo]["itgrpcd"] = summarybarcode.Rows[i - 1]["itgrpcd"].ToString();
                            IR.Rows[rNo]["slno"] = islno;
                            if (VE.Checkbox4 == true) IR.Rows[rNo]["barno"] = summarybarcode.Rows[i - 1]["barno"].ToString();
                            IR.Rows[rNo]["itnm"] = VE.Checkbox8 == true ? tbl1.Rows[i - 1]["fabitnm"].ToString()+"[ "+ tbl1.Rows[i]["hsncode"].retStr()+" ]": tbl1.Rows[i - 1]["fabitnm"].ToString();
                            if (VE.Checkbox3 == true) IR.Rows[rNo]["styleno"] = summarybarcode.Rows[i - 1]["styleno"].ToString();
                            IR.Rows[rNo]["uomnm"] = summarybarcode.Rows[i - 1]["uomcd"].ToString();
                            IR.Rows[rNo]["opqty"] = opqty;
                            IR.Rows[rNo]["opval"] = opval;
                            IR.Rows[rNo]["netpur"] = netpur;
                            IR.Rows[rNo]["purval"] = purval;
                            IR.Rows[rNo]["karqty"] = karqty;
                            IR.Rows[rNo]["karval"] = karval;
                            IR.Rows[rNo]["netsale"] = netsale;
                            IR.Rows[rNo]["salevalue"] = salevalue;
                            IR.Rows[rNo]["approval"] = approval;
                            IR.Rows[rNo]["netstktrans"] = netstktrans;
                            IR.Rows[rNo]["netadj"] = netadj;
                            IR.Rows[rNo]["balqty"] = balqty;
                            IR.Rows[rNo]["balval"] = balval;
                            if (i > maxB) break;
                        }
                        if (i > maxB) break;
                    }
                    if (i > maxB) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["itnm"] = "Total of " + summarybarcode.Rows[i - 1]["itgrpnm"].ToString();
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                string itgrpcd = summarybarcode.Rows[i - 1]["itgrpcd"].ToString();
                var unitwisegrptotal = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("itgrpcd").retStr() == itgrpcd)
                            .GroupBy(g => g.Field<string>("uomnm"))
                            .Select(g =>
                            {
                                var row = IR.NewRow();
                                row["uomnm"] = g.Key;
                                row["opqty"] = g.Sum(r => r.Field<double?>("opqty") == null ? 0 : r.Field<double>("opqty"));
                                row["opval"] = g.Sum(r => r.Field<double?>("opval") == null ? 0 : r.Field<double>("opval"));
                                row["netpur"] = g.Sum(r => r.Field<double?>("netpur").retDbl());
                                row["purval"] = g.Sum(r => r.Field<double?>("purval").retDbl());
                                row["karqty"] = g.Sum(r => r.Field<double?>("karqty").retDbl());
                                row["karval"] = g.Sum(r => r.Field<double?>("karval").retDbl());
                                row["approval"] = g.Sum(r => r.Field<double?>("approval").retDbl());
                                row["netstktrans"] = g.Sum(r => r.Field<double?>("netstktrans").retDbl());
                                row["netadj"] = g.Sum(r => r.Field<double?>("netadj").retDbl());
                                row["netsale"] = g.Sum(r => r.Field<double?>("netsale").retDbl());
                                row["salevalue"] = g.Sum(r => r.Field<double?>("salevalue").retDbl());
                                row["balqty"] = g.Sum(r => r.Field<double?>("balqty").retDbl());
                                row["balval"] = g.Sum(r => r.Field<double?>("balval").retDbl());
                                return row;
                            }).CopyToDataTable();
                int cnt = 0;
                for (int k = 0; k <= unitwisegrptotal.Rows.Count - 1; k++)
                {
                    if (unitwisegrptotal.Rows[k]["opqty"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netpur"].retDbl() != 0 || unitwisegrptotal.Rows[k]["karqty"].retDbl() != 0 || unitwisegrptotal.Rows[k]["approval"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netstktrans"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netadj"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netsale"].retDbl() != 0 || unitwisegrptotal.Rows[k]["balqty"].retDbl() != 0)
                    {
                        cnt++;
                        if (k == 0) { }
                        else { IR.Rows.Add(""); rNo = IR.Rows.Count - 1; }
                        IR.Rows[rNo]["uomnm"] = unitwisegrptotal.Rows[k]["uomnm"];
                        IR.Rows[rNo]["opqty"] = unitwisegrptotal.Rows[k]["opqty"];
                        IR.Rows[rNo]["opval"] = unitwisegrptotal.Rows[k]["opval"];
                        IR.Rows[rNo]["netpur"] = unitwisegrptotal.Rows[k]["netpur"];
                        IR.Rows[rNo]["purval"] = unitwisegrptotal.Rows[k]["purval"];
                        IR.Rows[rNo]["karqty"] = unitwisegrptotal.Rows[k]["karqty"];
                        IR.Rows[rNo]["karval"] = unitwisegrptotal.Rows[k]["karval"];
                        IR.Rows[rNo]["approval"] = unitwisegrptotal.Rows[k]["approval"];
                        IR.Rows[rNo]["netstktrans"] = unitwisegrptotal.Rows[k]["netstktrans"];
                        IR.Rows[rNo]["netadj"] = unitwisegrptotal.Rows[k]["netadj"];
                        IR.Rows[rNo]["netsale"] = unitwisegrptotal.Rows[k]["netsale"];
                        IR.Rows[rNo]["salevalue"] = unitwisegrptotal.Rows[k]["salevalue"];
                        IR.Rows[rNo]["balqty"] = unitwisegrptotal.Rows[k]["balqty"];
                        IR.Rows[rNo]["balval"] = unitwisegrptotal.Rows[k]["balval"];

                    }
                }
                if (cnt > 1)
                {
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;";
                }
                else
                {
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;border-top: 3px solid;";
                }
                if (i > maxB) break;
            }
            // Create Blank line
            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
            IR.Rows[rNo]["dammy"] = " ";
            IR.Rows[rNo]["flag"] = " height:8px; ";

            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
            IR.Rows[rNo]["itnm"] = "Grand Total";
            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;";

            var grptbl = IR.AsEnumerable().Where(g => g.Field<string>("itgrpcd").retStr() != "")
                            .GroupBy(g => g.Field<string>("uomnm"))
                            .Select(g =>
                            {
                                var row = IR.NewRow();
                                row["uomnm"] = g.Key;
                                row["opqty"] = g.Sum(r => r.Field<double?>("opqty") == null ? 0 : r.Field<double>("opqty"));
                                row["opval"] = g.Sum(r => r.Field<double?>("opval") == null ? 0 : r.Field<double>("opval"));
                                row["netpur"] = g.Sum(r => r.Field<double?>("netpur").retDbl());
                                row["purval"] = g.Sum(r => r.Field<double?>("purval").retDbl());
                                row["karqty"] = g.Sum(r => r.Field<double?>("karqty").retDbl());
                                row["karval"] = g.Sum(r => r.Field<double?>("karval").retDbl());
                                row["approval"] = g.Sum(r => r.Field<double?>("approval").retDbl());
                                row["netstktrans"] = g.Sum(r => r.Field<double?>("netstktrans").retDbl());
                                row["netadj"] = g.Sum(r => r.Field<double?>("netadj").retDbl());
                                row["netsale"] = g.Sum(r => r.Field<double?>("netsale").retDbl());
                                row["salevalue"] = g.Sum(r => r.Field<double?>("salevalue").retDbl());
                                row["balqty"] = g.Sum(r => r.Field<double?>("balqty").retDbl());
                                row["balval"] = g.Sum(r => r.Field<double?>("balval").retDbl());
                                return row;
                            }).CopyToDataTable();
            int cnt1 = 0;
            for (int k = 0; k <= grptbl.Rows.Count - 1; k++)
            {
                if (grptbl.Rows[k]["opqty"].retDbl() != 0 || grptbl.Rows[k]["netpur"].retDbl() != 0 || grptbl.Rows[k]["karqty"].retDbl() != 0 || grptbl.Rows[k]["approval"].retDbl() != 0 || grptbl.Rows[k]["netstktrans"].retDbl() != 0 || grptbl.Rows[k]["netadj"].retDbl() != 0 || grptbl.Rows[k]["netsale"].retDbl() != 0 || grptbl.Rows[k]["balqty"].retDbl() != 0)
                {
                    cnt1++;
                    if (k != 0)
                    {
                        IR.Rows.Add("");
                        rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;";
                    }

                    IR.Rows[rNo]["itgrpcd"] = "grandtotal";
                    IR.Rows[rNo]["uomnm"] = grptbl.Rows[k]["uomnm"];
                    IR.Rows[rNo]["opqty"] = grptbl.Rows[k]["opqty"];
                    IR.Rows[rNo]["opval"] = grptbl.Rows[k]["opval"];
                    IR.Rows[rNo]["netpur"] = grptbl.Rows[k]["netpur"];
                    IR.Rows[rNo]["purval"] = grptbl.Rows[k]["purval"];
                    IR.Rows[rNo]["karqty"] = grptbl.Rows[k]["karqty"];
                    IR.Rows[rNo]["karval"] = grptbl.Rows[k]["karval"];
                    IR.Rows[rNo]["approval"] = grptbl.Rows[k]["approval"];
                    IR.Rows[rNo]["netstktrans"] = grptbl.Rows[k]["netstktrans"];
                    IR.Rows[rNo]["netadj"] = grptbl.Rows[k]["netadj"];
                    IR.Rows[rNo]["netsale"] = grptbl.Rows[k]["netsale"];
                    IR.Rows[rNo]["salevalue"] = grptbl.Rows[k]["salevalue"];
                    IR.Rows[rNo]["balqty"] = grptbl.Rows[k]["balqty"];
                    IR.Rows[rNo]["balval"] = grptbl.Rows[k]["balval"];

                }
            }


            if (cnt1 > 1)
            {
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["itnm"] = "Total Value";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";
                IR.Rows[rNo]["opval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("opval"));
                IR.Rows[rNo]["purval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("purval"));
                IR.Rows[rNo]["karval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("karval"));
                IR.Rows[rNo]["salevalue"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("salevalue"));
                IR.Rows[rNo]["balval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("balval"));
            }
            IR.Columns.Remove("itgrpcd");
            string pghdr1 = "";
            string repname = "Stock_Val" + System.DateTime.Now;

            pghdr1 = "Stock Valuation(Barcode Wise Summary) as on " + ASDT;
            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

            TempData[repname] = PV;
            TempData[repname + "xxx"] = IR;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        }
        public ActionResult PrintReport()
        {
            try
            {
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}
