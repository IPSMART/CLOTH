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
                    VE.DropDown_list2 = drplst;
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.PRCCD = "CP";VE.PRCNM = "CP";
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

                string selgocd = "", selbrgrpcd = "", selitcd = "", unselitcd, selitgrpcd = "", prccd="";
                string summary = VE.TEXTBOX3; // == true?"S":"D";

                if (FC.AllKeys.Contains("itcdvalue")) selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = CommFunc.retSqlformat(FC["itcdunselvalue"].ToString());

                if (FC.AllKeys.Contains("gocdvalue")) selgocd = CommFunc.retSqlformat(FC["gocdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                if (FC.AllKeys.Contains("brgrpcdvalue")) selbrgrpcd = CommFunc.retSqlformat(FC["brgrpcdvalue"].ToString());

                if (selgocd == "" && summary == "G")
                {
                    selgocd = string.Join(",", (from a in DBF.M_GODOWN select a.GOCD).ToList()).retSqlformat();
                }
                if (summary == "B") prccd = VE.PRCCD;

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
               
                DataTable tbl = Salesfunc.GetStock(asdt, selgocd,"", selitcd, "FS".retSqlformat(), "",selitgrpcd);
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("");

                Int32 rNo = 0, maxR = 0,maxB=0, i = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;

                string pghdr1 = "";
                string repname = "Stock_Val" + System.DateTime.Now;
                double qdeci = 4;
                string qdsp = "n,16," + qdeci.ToString();
                bool ignoreitems = VE.Checkbox2;

                if (summary == "D")
                {
                    #region Details
                    HC.RepStart(IR);
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                    HC.GetPrintHeader(IR, "slcd", "string", "c,8", "slcd");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Name of Party");
                    HC.GetPrintHeader(IR, "qnty", "double", qdsp, "Bal.Qnty");
                    HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Av.Rate");
                    HC.GetPrintHeader(IR, "amt", "double", "n,14,2", "Stock Value");

                    maxR = tbl.Rows.Count - 1;

                    string strbrgrpcd = "", stritcd = "";
                    double gamt = 0;
                    i = 0;
                    while (i <= maxR)
                    {
                        strbrgrpcd = tbl.Rows[i]["itgrpcd"].ToString();

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["itgrpnm"].ToString();
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                        double bamt = 0;
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
                                IR.Rows[rNo]["amt"] = (tbl.Rows[i]["rate"].retDbl()* tbl.Rows[i]["balqnty"].retDbl()).retDbl();
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
                            if (i > maxR) break;
                        }
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["slnm"] = "Total of " + tbl.Rows[i - 1]["itgrpnm"].ToString();
                        IR.Rows[rNo]["amt"] = bamt;
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        gamt = gamt + bamt;
                        //i++;
                        if (i > maxR) break;
                    }
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["slnm"] = "Grand Total";
                    IR.Rows[rNo]["amt"] = gamt;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    #endregion
                }
                else if (summary == "S")
                {
                    #region Summary
                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
                    HC.GetPrintHeader(IR, "itcd", "string", "c,10", "itcd");
                    HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
                    HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
                    HC.GetPrintHeader(IR, "qnty", "double", qdsp, "Stk.Qnty");
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
                                    TSdys = Convert.ToDateTime(asdt) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
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
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    if (ageingperiod > 0)
                    {
                        if (gdue1Qty != 0) { IR.Rows[rNo]["stk1amt"] = gdue1Amt; }
                        if (gdue2Qty != 0) { IR.Rows[rNo]["stk2amt"] = gdue2Amt; }
                        if (gdue3Qty != 0) { IR.Rows[rNo]["stk3amt"] = gdue3Amt; }
                        if (gdue4Qty != 0) { IR.Rows[rNo]["stk4amt"] = gdue4Amt; }
                    }
                    #endregion
                }
                else if (summary == "G")
                {
                    #region GodownWise
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
                    gdue1Amt = 0; gdue2Amt = 0; gdue3Amt = 0; gdue4Amt = 0;
                    gdue1Qty = 0; gdue2Qty = 0; gdue3Qty = 0; gdue4Qty = 0;
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
                                        IR.Rows[rNo][amtdr["gocd"].ToString()] = tbl.Rows[i]["balqnty"].retDbl();
                                        tqty_ += tbl.Rows[i]["balqnty"].retDbl();
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
                    #endregion }
                }
                else if(summary=="B")
                {
                    #region summary(Barcode)


                    string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), finyr = CommVar.FinEndDate(UNQSNO), prcd = VE.PRCCD; ;
                    string query = "select a.barno, e.itcd, e.fabitcd, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
                    query += "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(c.rate, 0) clrate, ";
                    query += "f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm  from ";

                    query += "(select a.barno, 'OP' doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
                    query += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
                    query += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
                    query += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
                    query += "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
                    query += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and ";
                    query += "d.docdt < to_date('" + asdt + "', 'dd/mm/yyyy') ";
                    query += "group by a.barno, 'OP' ";
                    query += "union all ";
                    query += "select a.barno, c.doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
                    query += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
                    query += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
                    query += "    from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
                    query += "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
                    query += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and ";
                    query += "d.docdt >= to_date('" + asdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + finyr + "', 'dd/mm/yyyy') ";
                    query += "group by a.barno, c.doctag ) a, ";

                    query += "(select barno, effdt, prccd, rate from ( ";
                    query += "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                    query += "from " + scm + ".t_batchmst_price a ";
                    query += "where a.effdt <= to_date('" + asdt + "', 'dd/mm/yyyy') and a.prccd = '"+ prcd + "' ) where rn = 1) b, ";                                           
                                             
                    query += "(select barno, effdt, prccd, rate from ( ";
                    query += "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                    query += "from " + scm + ".t_batchmst_price a ";
                    query += "where a.effdt <= to_date('" + finyr + "', 'dd/mm/yyyy') and a.prccd = '" + prcd + "' ) where rn = 1) c, ";

                    query += "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i ";
                    query += "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
                    query += "a.barno = b.barno(+) and a.barno = c.barno(+) and ";
                    query += "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) ";
                    query += "order by itgrpnm, itgrpcd, fabitnm, fabitcd, itnm, itcd, styleno, barno ";
                    DataTable tbl1 = MasterHelp.SQLquery(query);
                    if (tbl1.Rows.Count == 0) return Content("no records..");
                
                    maxR = tbl1.Rows.Count - 1;

                    string strbrgrpcd = "", stritcd = "",keyvalue="", fabitcd="",uomcd="",styleno1="",barno1="";
                    double gamt = 0, gqnty = 0;
                    i = 0;
                    Int32 islno = 0,j=0;
                    gdue1Amt = 0; gdue2Amt = 0; gdue3Amt = 0; gdue4Amt = 0;
                    gdue1Qty = 0; gdue2Qty = 0; gdue3Qty = 0; gdue4Qty = 0;
                    #region Generate Data Table
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
                    summarybarcode.Columns.Add("clqty", typeof(double), "");
                    summarybarcode.Columns.Add("clval", typeof(double), "");

                    #endregion
                    //while (j <= maxR)
                    //{
                    //    Int32 maxd = 0; DataTable data = new DataTable();
                    //    summarybarcode.Rows.Add(); rNo = summarybarcode.Rows.Count - 1;
                    //    string styleno = "",barno="";
                    //    if (VE.Checkbox3 == true) styleno = tbl1.Rows[j]["styleno"].retStr();
                    //    if (VE.Checkbox4 == true) barno = tbl1.Rows[j]["barno"].retStr();
                    //    string keyval= tbl1.Rows[j]["uomcd"].retStr() + tbl1.Rows[j]["itgrpcd"].retStr() + tbl1.Rows[j]["fabitcd"].retStr() + tbl1.Rows[j]["itcd"].retStr() + styleno + barno;
                    //    summarybarcode.Rows[rNo]["key"] = keyval;
                    //    if (VE.Checkbox3 == true)
                    //    {  data = tbl1.Select("uomcd||itgrpcd||fabitcd||itcd||styleno= '" + keyval + "'").CopyToDataTable();
                    //    }
                    //   else if (VE.Checkbox4 == true)
                    //    { data = tbl1.Select("uomcd||itgrpcd||fabitcd||itcd||barno= '" + keyval + "'").CopyToDataTable();
                    //    }
                    //   else if(VE.Checkbox3 == true && VE.Checkbox4 == true)
                    //    { data = tbl1.Select("uomcd||itgrpcd||fabitcd||itcd||styleno||barno= '" + keyval + "'").CopyToDataTable();
                    //    }
                    //    maxd = data.Rows.Count - 1;
                    //    while(i<=maxd)
                    //    {
                    //        summarybarcode.Rows[rNo]["itgrpcd"] = data.Rows[j]["itgrpcd"].retStr();
                    //        summarybarcode.Rows[rNo]["itgrpnm"] = data.Rows[j]["itgrpnm"].retStr();
                    //        summarybarcode.Rows[rNo]["fabitcd"] = data.Rows[j]["fabitcd"].retStr();
                    //        summarybarcode.Rows[rNo]["fabitnm"] = data.Rows[j]["fabitnm"].retStr();
                    //        summarybarcode.Rows[rNo]["itgrpnm"] = data.Rows[j]["itgrpnm"].retStr();
                    //        summarybarcode.Rows[rNo]["itnm"] = data.Rows[j]["itnm"].retStr();
                    //        summarybarcode.Rows[rNo]["itcd"] = data.Rows[j]["itcd"].retStr();
                    //        summarybarcode.Rows[rNo]["styleno"] = data.Rows[j]["styleno"].retStr();
                    //        summarybarcode.Rows[rNo]["barno"] = data.Rows[j]["barno"].retStr();
                    //        summarybarcode.Rows[rNo]["uomcd"] = data.Rows[j]["uomcd"].retStr();
                    //        summarybarcode.Rows[rNo]["uomnm"] = data.Rows[j]["uomnm"].retStr();
                    //        summarybarcode.Rows[rNo]["qnty"] = data.Rows[j]["qnty"].retDbl();
                    //        summarybarcode.Rows[rNo]["txblval"] = data.Rows[j]["txblval"].retDbl();
                    //        summarybarcode.Rows[rNo]["opqty"] = (from DataRow dr in data.Rows where dr["doctag"].retStr() == tbl1.Rows[j]["doctag"].retStr() select dr["qnty"]).FirstOrDefault();
                    //        summarybarcode.Rows[rNo]["opval"] = 0;
                    //        summarybarcode.Rows[rNo]["purval"] = 0;
                    //        summarybarcode.Rows[rNo]["netpur"] = 0;
                    //        summarybarcode.Rows[rNo]["purval"] = 0;
                    //        summarybarcode.Rows[rNo]["karqty"] = 0;
                    //        summarybarcode.Rows[rNo]["netsale"] = 0;
                    //        summarybarcode.Rows[rNo]["salevalue"] = 0;
                    //        summarybarcode.Rows[rNo]["approval"] = 0;
                    //        summarybarcode.Rows[rNo]["netstktrans"] = 0;
                    //        summarybarcode.Rows[rNo]["netadj"] = 0;
                    //        summarybarcode.Rows[rNo]["balqty"] = 0;
                    //        summarybarcode.Rows[rNo]["balval"] = 0;
                    //        summarybarcode.Rows[rNo]["clqty"] = 0;
                    //        summarybarcode.Rows[rNo]["clval"] = 0;
                    //        i++;
                    //        if (i > maxR) break;
                    //    }

                    //    j++;
                    //}
                    while (j <= maxR)
                    {
                           Int32 maxd = 0; DataTable data = new DataTable();
                            summarybarcode.Rows.Add(); rNo = summarybarcode.Rows.Count - 1;
                            string styleno = "", barno = "";
                            if (VE.Checkbox3 == true) styleno = tbl1.Rows[j]["styleno"].retStr();
                            if (VE.Checkbox4 == true) barno = tbl1.Rows[j]["barno"].retStr();
                            string keyval = tbl1.Rows[j]["uomcd"].retStr() + tbl1.Rows[j]["itgrpcd"].retStr() + tbl1.Rows[j]["fabitcd"].retStr() + tbl1.Rows[j]["itcd"].retStr() + styleno + barno;
                            summarybarcode.Rows[rNo]["key"] = keyval;
                            summarybarcode.Rows[rNo]["itgrpcd"] = tbl1.Rows[j]["itgrpcd"].retStr();
                            summarybarcode.Rows[rNo]["itgrpnm"] = tbl1.Rows[j]["itgrpnm"].retStr();
                            summarybarcode.Rows[rNo]["fabitcd"] = tbl1.Rows[j]["fabitcd"].retStr();
                            summarybarcode.Rows[rNo]["fabitnm"] = tbl1.Rows[j]["fabitnm"].retStr();
                            summarybarcode.Rows[rNo]["itgrpnm"] = tbl1.Rows[j]["itgrpnm"].retStr();
                            summarybarcode.Rows[rNo]["itnm"] = tbl1.Rows[j]["itnm"].retStr();
                            summarybarcode.Rows[rNo]["itcd"] = tbl1.Rows[j]["itcd"].retStr();
                            summarybarcode.Rows[rNo]["styleno"] = tbl1.Rows[j]["styleno"].retStr();
                            summarybarcode.Rows[rNo]["barno"] = tbl1.Rows[j]["barno"].retStr();
                            summarybarcode.Rows[rNo]["uomcd"] = tbl1.Rows[j]["uomcd"].retStr();
                            summarybarcode.Rows[rNo]["uomnm"] = tbl1.Rows[j]["uomnm"].retStr();
                            summarybarcode.Rows[rNo]["qnty"] = tbl1.Rows[j]["qnty"].retDbl();
                            summarybarcode.Rows[rNo]["txblval"] = tbl1.Rows[j]["txblval"].retDbl();
                            summarybarcode.Rows[rNo]["opqty"] = (from DataRow dr in tbl1.Rows where dr["uomcd"].retStr() + dr["itgrpcd"].retStr() + dr["fabitcd"].retStr() + dr["itcd"].retStr() + styleno + barno == keyval && dr["doctag"].retStr()=="OP".retStr() select dr["qnty"].retDbl()).FirstOrDefault();
                            summarybarcode.Rows[rNo]["opval"] = 0;
                            summarybarcode.Rows[rNo]["purval"] = 0;
                            summarybarcode.Rows[rNo]["netpur"] = 0;
                            summarybarcode.Rows[rNo]["purval"] = 0;
                            summarybarcode.Rows[rNo]["karqty"] = (from DataRow dr in tbl1.Rows where dr["uomcd"].retStr() + dr["itgrpcd"].retStr() + dr["fabitcd"].retStr() + dr["itcd"].retStr() + styleno + barno == keyval && (dr["doctag"].retStr() == "KR".retStr() || dr["doctag"].retStr() == "KI".retStr()) select dr["qnty"].retDbl()).FirstOrDefault();
                            summarybarcode.Rows[rNo]["karval"] = 0;
                            summarybarcode.Rows[rNo]["netsale"] = 0;
                            summarybarcode.Rows[rNo]["salevalue"] = 0;
                            summarybarcode.Rows[rNo]["approval"] = 0;
                            summarybarcode.Rows[rNo]["netstktrans"] = 0;
                            summarybarcode.Rows[rNo]["netadj"] = 0;
                            summarybarcode.Rows[rNo]["balqty"] = 0;
                            summarybarcode.Rows[rNo]["balval"] = 0;
                            summarybarcode.Rows[rNo]["clqty"] = 0;
                            summarybarcode.Rows[rNo]["clval"] = 0;

                        j++;
                        if (j > maxR) break;
                    }
                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
                    HC.GetPrintHeader(IR, "barno", "string", "c,10", "Bar No.");
                    HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
                    HC.GetPrintHeader(IR, "styleno", "string", "c,40", "Style No.");
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
                    double gopqty = 0, gopval = 0;
                    maxB = summarybarcode.Rows.Count - 1;
                    while (i <= maxB)
                    {
                        double topqty = 0,topval=0;
                        strbrgrpcd = tbl1.Rows[i]["itgrpcd"].retStr();
                        keyvalue= tbl1.Rows[i]["uomcd"].retStr() + tbl1.Rows[i]["itgrpcd"].retStr() + tbl1.Rows[i]["fabitcd"].retStr() + tbl1.Rows[i]["itcd"].retStr() + tbl1.Rows[i]["styleno"].retStr() + tbl1.Rows[i]["barno"].retStr();

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + summarybarcode.Rows[i]["itgrpnm"].ToString();
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                       
                        while (summarybarcode.Rows[i]["itgrpcd"].retStr() == strbrgrpcd)
                        {
                            double opqty = 0, opamt = 0,netpurqty=0, netpuramt = 0;
                            stritcd = tbl1.Rows[i]["itcd"].retStr();
                            styleno1= tbl1.Rows[i]["styleno"].retStr();
                            barno1 = tbl1.Rows[i]["barno"].retStr();
                            fabitcd= tbl1.Rows[i]["fabitcd"].retStr();
                            uomcd = tbl1.Rows[i]["uomcd"].retStr();
                            while ((summarybarcode.Rows[i]["itcd"].retStr() == stritcd && summarybarcode.Rows[i]["fabitcd"].retStr() == fabitcd && summarybarcode.Rows[i]["uomcd"].retStr() == uomcd && summarybarcode.Rows[i]["styleno"].retStr() == styleno1 && summarybarcode.Rows[i]["barno"].retStr() == barno1))
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                islno++;
                                IR.Rows[rNo]["slno"] = islno;
                                IR.Rows[rNo]["barno"] = tbl1.Rows[i]["barno"].ToString();
                                IR.Rows[rNo]["itnm"] = tbl1.Rows[i]["fabitnm"].ToString();
                                IR.Rows[rNo]["styleno"] = tbl1.Rows[i]["styleno"].ToString();
                                IR.Rows[rNo]["uomnm"] = tbl1.Rows[i]["uomcd"].ToString();
                                if(tbl1.Rows[i]["doctag"].retStr()=="OP")
                                {
                                    IR.Rows[rNo]["opqty"] = tbl1.Rows[i]["qnty"].retDbl();
                                    opamt = (tbl1.Rows[i]["qnty"].retDbl() * tbl1.Rows[i]["oprate"].retDbl());
                                }
                                if (tbl1.Rows[i]["doctag"].retStr() == "PB"|| tbl1.Rows[i]["doctag"].retStr() == "PR")
                                {
                                    IR.Rows[rNo]["netsale"] = tbl1.Rows[i]["qnty"].retDbl();                                   
                                    netpuramt = tbl1.Rows[i]["txblval"].retDbl();
                                }
                                if (tbl1.Rows[i]["doctag"].retStr() == "PB" || tbl1.Rows[i]["doctag"].retStr() == "PR")
                                {
                                    netpurqty = netpurqty + tbl1.Rows[i]["qnty"].retDbl();
                                    netpuramt = tbl1.Rows[i]["txblval"].retDbl();
                                }
                               
                                IR.Rows[rNo]["opval"] = opamt;
                                IR.Rows[rNo]["salevalue"] = netpuramt;


                                i++;
                                if (i > maxR) break;
                            }
                            topqty = topqty + IR.Rows[rNo]["opqty"].retDbl();
                            topval = topval + IR.Rows[rNo]["opval"].retDbl();

                            if (i > maxR) break;
                        }
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["itnm"] = "Total of " + summarybarcode.Rows[i-1]["itgrpnm"].ToString();
                        IR.Rows[rNo]["opqty"] = topqty;
                        IR.Rows[rNo]["opval"] = topval;
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        gopqty = gopqty + topqty;
                        gopval = gopval + topval;

                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["itnm"] = "Grand Total";
                    IR.Rows[rNo]["opqty"] = gopqty;
                    IR.Rows[rNo]["opval"] = gopval;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    #endregion
                }

                //pghdr1 = "Stock Valuation (Method - " + calmethod + ") as on " + asdt;
                pghdr1 = "Stock Valuation as on " + asdt;
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

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
