//using System;
//using System.Linq;
//using System.Web.Mvc;
//using Improvar.Models;
//using Improvar.ViewModels;
//using System.Data;
//using System.Collections.Generic;

//namespace Improvar.Controllers
//{
//    public class Rep_Stk_StmtController : Controller
//    {
//        public static string[,] headerArray;
//        Connection Cn = new Connection();
//        MasterHelp MasterHelp = new MasterHelp();
//        DropDownHelp DropDownHelp = new DropDownHelp();
//        Salesfunc Salesfunc = new Salesfunc();
//        string UNQSNO = CommVar.getQueryStringUNQSNO();

//        // GET: Rep_Stk_Stmt
//        public ActionResult Rep_Stk_Stmt()
//        {
//            try
//            {
//                if (Session["UR_ID"] == null)
//                {
//                    return RedirectToAction("Login", "Login");
//                }
//                else
//                {
//                    ViewBag.formname = "Stock Valuation";
//                    ReportViewinHtml VE = new ReportViewinHtml();
//                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
//                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
//                    //VE.DropDown_list1 = Salesfunc.Stock_Calc_Mehtod();

//                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");

//                    //VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection(selgrp);
//                    VE.DropDown_list_ITGRP = (from i in DB.M_GROUP select new DropDown_list_ITGRP() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();
//                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

//                    VE.DropDown_list_BRGRP = DropDownHelp.GetBrgrpcdforSelection(selgrp);
//                    VE.Brgrpnm = MasterHelp.ComboFill("brgrpcd", VE.DropDown_list_BRGRP, 0, 1);
                    
//                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection(selgrp);
//                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

//                    VE.DropDown_list_GODOWN = DropDownHelp.GetGocdforSelection();
//                    VE.Gonm = MasterHelp.ComboFill("gocd", VE.DropDown_list_GODOWN, 0, 1);

//                    List<DropDown_list2> drplst = new List<DropDown_list2>();
//                    DropDown_list2 dropobj1 = new DropDown_list2();
//                    dropobj1.value = "S";
//                    dropobj1.text = "Summary";
//                    drplst.Add(dropobj1);

//                    DropDown_list2 dropobj2 = new DropDown_list2();
//                    dropobj2.value = "D";
//                    dropobj2.text = "Details";
//                    drplst.Add(dropobj2);

//                    DropDown_list2 dropobj3 = new DropDown_list2();
//                    dropobj3.value = "G";
//                    dropobj3.text = "Godown wise";
//                    drplst.Add(dropobj3);
//                    VE.DropDown_list2 = drplst;

//                    VE.TDT = CommVar.CurrDate(UNQSNO);
//                    VE.DefaultView = true;
//                    return View(VE);
//                }
//            }
//            catch (Exception ex)
//            {
//                Cn.SaveException(ex, "");
//                return Content(ex.Message + ex.InnerException);
//            }
//        }

//        [HttpPost]
//        public ActionResult Rep_Stk_Stmt(FormCollection FC, ReportViewinHtml VE)
//        {
//            try
//            {
//                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

//                VE.DropDown_list1 = Salesfunc.Stock_Calc_Mehtod();
//                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO);
//                string asdt = VE.TDT.retDateStr();
//                string calctype = VE.TEXTBOX2;

//                string calmethod = (from x in VE.DropDown_list1 where x.value == calctype select x.text).SingleOrDefault();

//                string selgocd = "", selbrgrpcd = "", selitcd = "", unselitcd, selitgrpcd="";
//                string summary = VE.TEXTBOX3; // == true?"S":"D";

//                if (FC.AllKeys.Contains("itcdvalue")) selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
//                if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = CommFunc.retSqlformat(FC["itcdunselvalue"].ToString());

//                if (FC.AllKeys.Contains("gocdvalue")) selgocd = CommFunc.retSqlformat(FC["gocdvalue"].ToString());
//                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
//                if (FC.AllKeys.Contains("brgrpcdvalue")) selbrgrpcd = CommFunc.retSqlformat(FC["brgrpcdvalue"].ToString());

//                if (selgocd == "" && summary == "G")
//                {
//                    selgocd = string.Join(",", (from a in DB.M_GODOWN select a.GOCD).ToList()).retSqlformat();
//                }

//                string days_aging = VE.TEXTBOX5; // Days aging value
//                string days1 = "0", days2 = "0", days3 = "0";
//                if (days_aging == null)
//                {
//                }
//                else if (days_aging == "1")
//                {
//                    days1 = FC["days1"];
//                }
//                else if (days_aging == "2")
//                {
//                    days1 = FC["days1"];
//                    days2 = FC["days2"];
//                }
//                else if (days_aging == "3")
//                {
//                    days1 = FC["days1"];
//                    days2 = FC["days2"];
//                    days3 = FC["days3"];
//                }

//                long duedays1, duedays2, duedays3, duedays4, duedays5 = 0;
//                long ageingperiod = 0;
//                duedays1 = Convert.ToInt64(days1); duedays2 = Convert.ToInt64(days2); duedays3 = Convert.ToInt64(days3); 
//                ageingperiod = Convert.ToInt64(days_aging);

//                long due1fDys=0, due1tDys=0, due2fDys=0, due2tDys=0, due3fDys=0, due3tDys=0, due4fDys=0, due4tDys=0;
//                if (ageingperiod != 0)
//                {
//                    ageingperiod = ageingperiod + 1;

//                    if (ageingperiod >= 1) due1fDys = 0; due1tDys = duedays1;
//                    if (ageingperiod >= 2) due2fDys = due1tDys + 1; due2tDys = duedays2;
//                    if (ageingperiod >= 3) due3fDys = due2tDys + 1; due3tDys = duedays3;
//                    if (ageingperiod >= 4) due4fDys = due3tDys + 1; due4tDys = 0;

//                    //define last ageing column
//                    if (ageingperiod == 2) due2tDys = 99999;
//                    if (ageingperiod == 3) due3tDys = 99999;
//                    if (ageingperiod == 4) due4tDys = 99999;
//                }
//                double gdue1Amt = 0, gdue2Amt = 0, gdue3Amt = 0, gdue4Amt = 0;
//                double gdue1Qty = 0, gdue2Qty = 0, gdue3Qty = 0, gdue4Qty = 0;
//                double bdue1Amt = 0, bdue2Amt = 0, bdue3Amt = 0, bdue4Amt = 0;
//                double bdue1Qty = 0, bdue2Qty = 0, bdue3Qty = 0, bdue4Qty = 0;
//                double due1Amt = 0, due2Amt = 0, due3Amt = 0, due4Amt = 0;
//                double due1Qty = 0, due2Qty = 0, due3Qty = 0, due4Qty = 0;

//                DataTable tbl = Salesfunc.GenStocktblwithVal(calctype, asdt,selitgrpcd, selbrgrpcd, selitcd, selgocd,true,"", (summary == "D"?false:true));
//                Models.PrintViewer PV = new Models.PrintViewer();
//                HtmlConverter HC = new HtmlConverter();
//                DataTable IR = new DataTable("");

//                Int32 rNo = 0, maxR = 0, i = 0;

//                // Report begins
//                i = 0; maxR = tbl.Rows.Count - 1;

//                string pghdr1 = "";
//                string repname = "Stock_Val" + System.DateTime.Now;
//                double qdeci = 4;
//                string qdsp = "n,16," + qdeci.ToString();
//                bool ignoreitems = VE.Checkbox2;

//                if (summary == "D")
//                {
//                    #region Details
//                    HC.RepStart(IR);
//                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
//                    HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
//                    HC.GetPrintHeader(IR, "slcd", "string", "c,8", "slcd");
//                    HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Name of Party");
//                    HC.GetPrintHeader(IR, "qnty", "double", qdsp, "Bal.Qnty");
//                    HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Av.Rate");
//                    HC.GetPrintHeader(IR, "amt", "double", "n,14,2", "Stock Value");

//                    maxR = tbl.Rows.Count - 1;

//                    string strbrgrpcd = "", stritcd = "";
//                    double gamt = 0;
//                    i = 0;
//                    while (i <= maxR)
//                    {
//                        strbrgrpcd = tbl.Rows[i]["brgrpcd"].ToString();

//                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                        IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["brgrpnm"].ToString();
//                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

//                        double bamt = 0;
//                        while (tbl.Rows[i]["brgrpcd"].ToString() == strbrgrpcd)
//                        {
//                            stritcd = tbl.Rows[i]["itcd"].ToString();
//                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + stritcd + "  " + " </span>" + tbl.Rows[i]["itnm"].ToString();
//                            IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + " </span>" + " [" + tbl.Rows[i]["uomnm"] + "]";
//                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
//                            double iqnty = 0, iamt = 0;
//                            while (tbl.Rows[i]["itcd"].ToString() == stritcd)
//                            {
//                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"];
//                                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().retDateStr();
//                                IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].ToString();
//                                IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
//                                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].ToString();
//                                IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].ToString();
//                                IR.Rows[rNo]["amt"] = tbl.Rows[i]["amt"].ToString();
//                                iqnty = iqnty + Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
//                                iamt = iamt + Convert.ToDouble(tbl.Rows[i]["amt"].ToString());
//                                i++;
//                                if (i > maxR) break;
//                            }
//                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                            IR.Rows[rNo]["slnm"] = "Total of " + tbl.Rows[i - 1]["itnm"].ToString();
//                            IR.Rows[rNo]["qnty"] = iqnty;
//                            IR.Rows[rNo]["amt"] = iamt;
//                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

//                            bamt = bamt + iamt;
//                            if (i > maxR) break;
//                        }
//                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                        IR.Rows[rNo]["slnm"] = "Total of " + tbl.Rows[i - 1]["brgrpnm"].ToString();
//                        IR.Rows[rNo]["amt"] = bamt;
//                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
//                        gamt = gamt + bamt;
//                        //i++;
//                        if (i > maxR) break;
//                    }
//                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                    IR.Rows[rNo]["slnm"] = "Grand Total";
//                    IR.Rows[rNo]["amt"] = gamt;
//                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
//                    #endregion
//                }
//                else if (summary == "S")
//                {
//                    #region Summary
//                    HC.RepStart(IR, 3);
//                    HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
//                    HC.GetPrintHeader(IR, "itcd", "string", "c,10", "itcd");
//                    HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
//                    HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
//                    HC.GetPrintHeader(IR, "qnty", "double", qdsp, "Stk.Qnty");
//                    HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Av.Rate");
//                    HC.GetPrintHeader(IR, "amt", "double", "n,14,2", "Stock Value");
//                    if (ageingperiod >= 1) HC.GetPrintHeader(IR, "stk1qty", "double", "n,14,3", "<= " + due1tDys.ToString() + ";Qty");
//                    if (ageingperiod >= 1) HC.GetPrintHeader(IR, "stk1amt", "double", "n,14,2", "<= " + due1tDys.ToString() + ";Amt");
//                    if (ageingperiod >= 2) HC.GetPrintHeader(IR, "stk2qty", "double", "n,14,3", due2fDys.ToString() + " to " + due2tDys.ToString() + ";Qty");
//                    if (ageingperiod >= 2) HC.GetPrintHeader(IR, "stk2amt", "double", "n,14,2", due2fDys.ToString() + " to " + due2tDys.ToString() + ";Amt");
//                    if (ageingperiod >= 3) HC.GetPrintHeader(IR, "stk3qty", "double", "n,14,3", due3fDys.ToString() + " to " + due3tDys.ToString() + ";Qty");
//                    if (ageingperiod >= 3) HC.GetPrintHeader(IR, "stk3amt", "double", "n,14,2", due3fDys.ToString() + " to " + due3tDys.ToString() + ";Amt");
//                    if (ageingperiod >= 4) HC.GetPrintHeader(IR, "stk4qty", "double", "n,14,3", "> " + due3tDys.ToString() + ";Qty");
//                    if (ageingperiod >= 4) HC.GetPrintHeader(IR, "stk4amt", "double", "n,14,2", "> " + due3tDys.ToString() + ";Amt");
//                    maxR = tbl.Rows.Count - 1;

//                    string strbrgrpcd = "", stritcd = "";
//                    double gamt = 0, gqnty = 0;
//                    i = 0;
//                    Int32 islno = 0;
//                    gdue1Amt = 0; gdue2Amt = 0; gdue3Amt = 0; gdue4Amt = 0;
//                    gdue1Qty = 0; gdue2Qty = 0; gdue3Qty = 0; gdue4Qty = 0;
//                    while (i <= maxR)
//                    {
//                        strbrgrpcd = tbl.Rows[i]["brgrpcd"].ToString();
//                        if (ignoreitems == false)
//                        {
//                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["brgrpnm"].ToString();
//                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
//                        }

//                        double bamt = 0, bqnty = 0;
//                        if (ignoreitems == false) islno = 0;
//                        bdue1Amt = 0; bdue2Amt = 0; bdue3Amt = 0; bdue4Amt = 0;
//                        bdue1Qty = 0; bdue2Qty = 0; bdue3Qty = 0; bdue4Qty = 0;
//                        while (tbl.Rows[i]["brgrpcd"].ToString() == strbrgrpcd)
//                        {
//                            stritcd = tbl.Rows[i]["itcd"].ToString();
//                            double iqnty = 0, iamt = 0;
//                            due1Amt = 0; due2Amt = 0; due3Amt = 0; due4Amt = 0;
//                            due1Qty = 0; due2Qty = 0; due3Qty = 0; due4Qty = 0;
//                            while (tbl.Rows[i]["itcd"].ToString() == stritcd)
//                            {
//                                double days = 0;
//                                TimeSpan TSdys;
//                                if (tbl.Rows[i]["docdt"] == DBNull.Value) days = 0;
//                                else
//                                {
//                                    TSdys = Convert.ToDateTime(asdt) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
//                                    days = TSdys.Days;
//                                }

//                                double _qty = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString()), _amt = Convert.ToDouble(tbl.Rows[i]["amt"].ToString());
//                                if (ageingperiod > 0)
//                                {
//                                    if (days <= due1tDys && due1tDys != 0) { due1Qty = due1Qty + _qty; due1Amt = due1Amt + _amt; }
//                                    else if (days <= due2tDys && due2tDys != 0) { due2Qty = due2Qty + _qty; due2Amt = due2Amt + _amt; }
//                                    else if (days <= due3tDys && due3tDys != 0) { due3Qty = due3Qty + _qty; due3Amt = due3Amt + _amt; }
//                                    else { due4Qty = due4Qty + _qty; due4Amt = due4Amt + _amt; }
//                                }
//                                iqnty = iqnty + _qty;
//                                iamt = iamt + _amt;
//                                i++;
//                                if (i > maxR) break;
//                            }
//                            if (Math.Round(iqnty, 6) != 0)
//                            {
//                                double avrt = 0;
//                                if (iqnty != 0) avrt = iamt / iqnty;
//                                if (ignoreitems == false)
//                                {
//                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                                    islno++;
//                                    IR.Rows[rNo]["slno"] = islno;
//                                    IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itcd"].ToString();
//                                    IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itnm"].ToString();
//                                    IR.Rows[rNo]["uomnm"] = tbl.Rows[i - 1]["uomnm"].ToString();
//                                    IR.Rows[rNo]["qnty"] = iqnty;
//                                    IR.Rows[rNo]["rate"] = avrt;
//                                    IR.Rows[rNo]["amt"] = iamt;

//                                    if (ageingperiod > 0)
//                                    {
//                                        if (due1Qty != 0) { IR.Rows[rNo]["stk1qty"] = due1Qty; IR.Rows[rNo]["stk1amt"] = due1Amt; }
//                                        if (due2Qty != 0) { IR.Rows[rNo]["stk2qty"] = due2Qty; IR.Rows[rNo]["stk2amt"] = due2Amt; }
//                                        if (due3Qty != 0) { IR.Rows[rNo]["stk3qty"] = due3Qty; IR.Rows[rNo]["stk3amt"] = due3Amt; }
//                                        if (due4Qty != 0) { IR.Rows[rNo]["stk4qty"] = due4Qty; IR.Rows[rNo]["stk4amt"] = due4Amt; }
//                                    }
//                                }
//                                bdue1Qty = bdue1Qty + due1Qty; bdue1Amt = bdue1Amt + due1Amt;
//                                bdue2Qty = bdue2Qty + due2Qty; bdue2Amt = bdue2Amt + due2Amt;
//                                bdue3Qty = bdue3Qty + due3Qty; bdue3Amt = bdue3Amt + due3Amt;
//                                bdue4Qty = bdue4Qty + due4Qty; bdue4Amt = bdue4Amt + due4Amt;
//                                bamt = bamt + iamt;
//                                bqnty = bqnty + iqnty;
//                            }

//                            if (i > maxR) break;
//                        }
//                        if (ignoreitems == false)
//                        {
//                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                            IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["brgrpnm"].ToString();
//                            IR.Rows[rNo]["amt"] = bamt;
//                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
//                        }
//                        else
//                        {
//                            if (bqnty != 0)
//                            {
//                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                                islno++;
//                                IR.Rows[rNo]["slno"] = islno;
//                                IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["brgrpcd"].ToString();
//                                IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["brgrpnm"].ToString();
//                                IR.Rows[rNo]["uomnm"] = tbl.Rows[i - 1]["uomnm"].ToString();
//                                IR.Rows[rNo]["qnty"] = bqnty;
//                                IR.Rows[rNo]["amt"] = bamt;
//                            }
//                        }
//                        if (ageingperiod > 0)
//                        {
//                            if (bdue1Qty != 0) { IR.Rows[rNo]["stk1qty"] = bdue1Qty; IR.Rows[rNo]["stk1amt"] = bdue1Amt; gdue1Qty = gdue1Qty + bdue1Qty; gdue1Amt = gdue1Amt + bdue1Amt; }
//                            if (bdue2Qty != 0) { IR.Rows[rNo]["stk2qty"] = bdue2Qty; IR.Rows[rNo]["stk2amt"] = bdue2Amt; gdue2Qty = gdue2Qty + bdue2Qty; gdue2Amt = gdue2Amt + bdue2Amt; }
//                            if (bdue3Qty != 0) { IR.Rows[rNo]["stk3qty"] = bdue3Qty; IR.Rows[rNo]["stk3amt"] = bdue3Amt; gdue3Qty = gdue3Qty + bdue3Qty; gdue3Amt = gdue3Amt + bdue3Amt; }
//                            if (bdue4Qty != 0) { IR.Rows[rNo]["stk4qty"] = bdue4Qty; IR.Rows[rNo]["stk4amt"] = bdue4Amt; gdue4Qty = gdue4Qty + bdue4Qty; gdue4Amt = gdue4Amt + bdue4Amt; }
//                        }
//                        gamt = gamt + bamt;
//                        gqnty = gqnty + bqnty;
//                        //i++;
//                        if (i > maxR) break;
//                    }
//                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                    IR.Rows[rNo]["itnm"] = "Grand Total";
//                    IR.Rows[rNo]["amt"] = gamt;
//                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
//                    if (ageingperiod > 0)
//                    {
//                        if (gdue1Qty != 0) { IR.Rows[rNo]["stk1amt"] = gdue1Amt; }
//                        if (gdue2Qty != 0) { IR.Rows[rNo]["stk2amt"] = gdue2Amt; }
//                        if (gdue3Qty != 0) { IR.Rows[rNo]["stk3amt"] = gdue3Amt; }
//                        if (gdue4Qty != 0) { IR.Rows[rNo]["stk4amt"] = gdue4Amt; }
//                    }
//                    #endregion
//                }
//                else if (summary == "G")
//                {
//                    #region GodownWise
//                    DataView dv = new DataView(tbl);
//                    dv.Sort = "itcd, gocd asc";
//                    tbl = dv.ToTable();
//                    DataTable amtDT = new DataTable("goDT");
//                    string[] amtTBLCOLS = new string[] { "gocd", "gonm" };
//                    amtDT = tbl.DefaultView.ToTable(true, amtTBLCOLS);
//                    amtDT.Columns.Add("goqty", typeof(double));

//                    HC.RepStart(IR, 3);
//                    HC.GetPrintHeader(IR, "slno", "long", "n,4", "Sl#");
//                    HC.GetPrintHeader(IR, "itcd", "string", "c,10", "itcd");
//                    HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name");
//                    HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
//                    foreach (DataRow dr in amtDT.Rows)
//                    {
//                        HC.GetPrintHeader(IR, dr["gocd"].ToString(), "double", "n,14,3", dr["gonm"].ToString());
//                    }
//                    HC.GetPrintHeader(IR, "totalqnty", "double", "n,14,3", "Total Qty");
//                    maxR = tbl.Rows.Count - 1;

//                    string strbrgrpcd = "", stritcd = "", gocd = "";
//                    double gamt = 0, gqnty = 0;
//                    i = 0;
//                    Int32 islno = 0;
//                    gdue1Amt = 0; gdue2Amt = 0; gdue3Amt = 0; gdue4Amt = 0;
//                    gdue1Qty = 0; gdue2Qty = 0; gdue3Qty = 0; gdue4Qty = 0;
//                    double _qty = 0, _amt = 0;
//                    while (i <= maxR)
//                    {
//                        strbrgrpcd = tbl.Rows[i]["brgrpcd"].ToString();

//                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                        IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + strbrgrpcd + "  " + " </span>" + tbl.Rows[i]["brgrpnm"].ToString();
//                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

//                        foreach (DataRow amtdr in amtDT.Rows) amtdr["goqty"] = 0;
//                        double bamt = 0, bqnty = 0;

//                        bdue1Amt = 0; bdue2Amt = 0; bdue3Amt = 0; bdue4Amt = 0;
//                        bdue1Qty = 0; bdue2Qty = 0; bdue3Qty = 0; bdue4Qty = 0; double tqty = 0, tqty_ = 0;
//                        while (tbl.Rows[i]["brgrpcd"].ToString() == strbrgrpcd)
//                        {
//                            stritcd = tbl.Rows[i]["itcd"].ToString();
//                            double iqnty = 0, iamt = 0;
//                            due1Amt = 0; due2Amt = 0; due3Amt = 0; due4Amt = 0;
//                            due1Qty = 0; due2Qty = 0; due3Qty = 0; due4Qty = 0;
//                            tqty_ = 0;

//                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
//                            islno++;
//                            IR.Rows[rNo]["slno"] = islno;
//                            IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"].ToString();
//                            IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].ToString();
//                            IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();

//                            while (tbl.Rows[i]["itcd"].ToString() == stritcd)
//                            {
//                                double avrt = 0;
//                                if (iqnty != 0) avrt = iamt / iqnty;
//                                gocd = tbl.Rows[i]["gocd"].ToString();

//                                foreach (DataRow amtdr in amtDT.Rows)
//                                {
//                                    if (gocd == amtdr["gocd"].retStr())
//                                    {
//                                        amtdr["goqty"] = amtdr["goqty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
//                                        IR.Rows[rNo][amtdr["gocd"].ToString()] = tbl.Rows[i]["qnty"].retDbl();
//                                        tqty_ += tbl.Rows[i]["qnty"].retDbl();
//                                    }
//                                }
//                                i++;
//                                if (i > maxR) break;
//                            }
//                            IR.Rows[rNo]["totalqnty"] = tqty_.retDbl();
//                            if (i > maxR) break;
//                        }
//                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

//                        tqty = 0;
//                        IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["brgrpnm"].ToString();
//                        foreach (DataRow amtdr in amtDT.Rows)
//                        {
//                            IR.Rows[rNo][amtdr["gocd"].ToString()] = amtdr["goqty"].retDbl();
//                            tqty += amtdr["goqty"].retDbl();

//                        }
//                        //tqty += tqty_.retDbl();
//                        IR.Rows[rNo]["totalqnty"] = tqty;
//                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

//                        if (i > maxR) break;
//                    }
//                    #endregion }
//                }

//                pghdr1 = "Stock Valuation (Method - " + calmethod + ") as on " + asdt;
//                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

//                TempData[repname] = PV;
//                TempData[repname + "xxx"] = IR;
//                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
//            }
//            catch (Exception ex)
//            {
//                Cn.SaveException(ex, "");
//                return Content(ex.Message);
//            }
//        }

//        public ActionResult PrintReport()
//        {
//            try
//            {
//                return RedirectToAction("ResponsivePrintViewer", "RPTViewer");
//            }
//            catch (Exception ex)
//            {
//                Cn.SaveException(ex, "");
//                return Content(ex.Message + ex.InnerException);
//            }
//        }
//    }
//}
