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
    public class Rep_Bale_StockController : Controller
    {
        // GET: Rep_Bale_Stock
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Bale_Stock()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Godown wise Stock";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    string qry = "select distinct baleno ||'/'|| baleyr BaleNoBaleYr,baleno || baleyr BaleNoBaleYrcd from " + CommVar.CurSchema(UNQSNO) + ".t_txndtl where  baleno is not null and baleyr is not null  ";
                    DataTable tbl = MasterHelp.SQLquery(qry);
                    VE.DropDown_list1 = (from DataRow dr in tbl.Rows select new DropDown_list1() { value = dr["BaleNoBaleYrcd"].retStr(), text = dr["BaleNoBaleYr"].retStr() }).OrderBy(s => s.text).ToList();
                    VE.TEXTBOX1 = MasterHelp.ComboFill("BaleNoBaleYrcd", VE.DropDown_list1, 0, 1);
                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);
                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);
                    VE.DropDown_list_GODOWN = DropDownHelp.GetGocdforSelection();
                    VE.Gonm = MasterHelp.ComboFill("gocd", VE.DropDown_list_GODOWN, 0, 1);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false;
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
        public ActionResult Rep_Bale_Stock(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();


                string txntag = ""; string txnrettag = "", itcd = "";
                string BalenoBaleyr = "";
                string gocd = "", unselitcd = "", plist = "", selslcd = "", unselslcd = "", selitgrpcd = "", selbrgrpcd = "";
                if (FC.AllKeys.Contains("BaleNoBaleYrcdvalue")) BalenoBaleyr = CommFunc.retSqlformat(FC["BaleNoBaleYrcdvalue"].ToString());

                if (FC.AllKeys.Contains("itcdvalue")) itcd = FC["itcdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                if (FC.AllKeys.Contains("gocdvalue")) gocd = CommFunc.retSqlformat(FC["gocdvalue"].ToString());
                txntag = txntag + txnrettag;
                bool RepeatAllRow = VE.Checkbox1;
                DataTable tbl = Salesfunc.GetBaleStock(tdt, gocd, BalenoBaleyr, itcd, "", "", selitgrpcd);
                if (tbl.Rows.Count == 0) return Content("no records..");
                DataView dv = new DataView(tbl);
                dv.Sort = "gocd,baleno,styleno";
                tbl = dv.ToTable();
               
                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "", chkval2 = "",gonm="";
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                if (RepeatAllRow == true) HC.GetPrintHeader(IR, "gonm", "string", "c,16", "Godown");
                HC.GetPrintHeader(IR, "prefno", "string", "c,20", "Bill No");
                HC.GetPrintHeader(IR, "prefdt", "string", "c,16", "Bill Date");
                HC.GetPrintHeader(IR, "styleno", "string", "c,25", "Style No");
                HC.GetPrintHeader(IR, "Shade", "string", "c,10", "Shade");
                HC.GetPrintHeader(IR, "baleno", "string", "c,12", "Bale No");
                HC.GetPrintHeader(IR, "nos", "double", "c,7", "Nos");
                HC.GetPrintHeader(IR, "qnty", "double", "c,16,3", "Qnty");
                HC.GetPrintHeader(IR, "rate", "double", "c,10,2", "Rate");
                HC.GetPrintHeader(IR, "value", "double", "c,16,2", "Value");
                HC.GetPrintHeader(IR, "lrno", "string", "c,14", "LR No");
                HC.GetPrintHeader(IR, "pageno", "string", "c,10", "Page No.");

                double gtqty, gtnos, gtval, flag;
                gtqty = 0; gtnos = 0; gtval = 0; flag = 0 ;
                Int32 rNo = 0;
                string baleno = "", cncat = ""; bool gonmfirst = true;
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int count = 0;
                while (i <= maxR)
                {
                    double tqty, tnos, tval,gdtqty,gdtval;
                    tnos = 0;  gdtqty = 0;gdtval = 0;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    chkval1 = tbl.Rows[i]["gocd"].ToString();
                    if (RepeatAllRow == false || gonmfirst == false) IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    if (RepeatAllRow == false || gonmfirst == false) IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["gocd"].retStr() + "  " + " </span>" + tbl.Rows[i]["gonm"].retStr();
                    if (RepeatAllRow == false || gonmfirst == false) IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    int gcount = 0;
                    while (tbl.Rows[i]["gocd"].ToString() == chkval1)
                    {
                        tval = 0; tqty = 0;
                        bool balefirst = true;
                        gonm = tbl.Rows[i]["gonm"].ToString();
                        chkval = tbl.Rows[i]["BaleNoBaleYrcd"].ToString();
                        gcount++;
                        count++;
                        while (tbl.Rows[i]["gocd"].ToString() == chkval1 && tbl.Rows[i]["BaleNoBaleYrcd"].ToString() == chkval)
                        {                           
                            bool itemfirst = true;
                            baleno = tbl.Rows[i]["baleno"].ToString();
                            chkval2 = tbl.Rows[i]["itcd"].ToString();
                          
                            while (tbl.Rows[i]["gocd"].ToString() == chkval1 && tbl.Rows[i]["BaleNoBaleYrcd"].ToString() == chkval && tbl.Rows[i]["itcd"].ToString() == chkval2)
                            {
                                if (tbl.Rows[i]["pageno"].retStr() != "" && tbl.Rows[i]["pageslno"].retStr() != "") cncat = "/";
                                tnos = tnos + tbl.Rows[i]["nos"].retDbl();
                                tqty = tqty + tbl.Rows[i]["qnty"].retDbl();
                                gdtqty = gdtqty + tbl.Rows[i]["qnty"].retDbl();
                                double value = tbl.Rows[i]["txblval"].retDbl() + tbl.Rows[i]["othramt"].retDbl(); //tbl.Rows[i]["qnty"].retDbl() * tbl.Rows[i]["rate"].retDbl();
                                tval = tval + value;
                                gdtval = gdtval + value;
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                                if (RepeatAllRow == true || itemfirst == true) IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();
                                if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].ToString();
                                IR.Rows[rNo]["prefdt"] = tbl.Rows[i]["prefdt"].retDateStr();
                                IR.Rows[rNo]["shade"] = tbl.Rows[i]["shade"].retStr();
                                IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                                IR.Rows[rNo]["value"] = value.retDbl();
                                IR.Rows[rNo]["pageno"] = tbl.Rows[i]["pageno"].retStr() + cncat + tbl.Rows[i]["pageslno"].retStr();
                                if (RepeatAllRow == true) IR.Rows[rNo]["gonm"] = tbl.Rows[i]["gonm"].retStr();
                                balefirst = false; itemfirst = false; gonmfirst = false; 
                                i = i + 1;
                                if (i > maxR) break;
                            }
                            if (i > maxR) break;
                        }
                        if (RepeatAllRow == false)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["prefno"] = "Total of " + baleno + " ";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            IR.Rows[rNo]["qnty"] = tqty;
                            IR.Rows[rNo]["value"] = tval;
                        }
                        if (i > maxR) break;
                    }
                    if (RepeatAllRow == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = "";
                        IR.Rows[rNo]["prefno"] = "Total (" + gcount.retStr() + ")  in " + gonm + " ";
                        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        IR.Rows[rNo]["qnty"] = gdtqty;
                        IR.Rows[rNo]["value"] = gdtval;
                    }
                    gtqty = gtqty + gdtqty;
                    gtval = gtval + gdtval;
                    flag++; gonmfirst = true;
                    if (i > maxR) break;                
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["prefno"] = "Total " + count.retStr() + " bales  ";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["qnty"] = gtqty;
                IR.Rows[rNo]["value"] = gtval;
                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                string pghdr1 = "Godown wise Stock as on " + tdt;
                string repname = "Godown wise Stock";
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