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
                    ViewBag.formname = "Bale Report";
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

                    VE.FDT = CommVar.FinStartDate(UNQSNO); VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = true;
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
                string selitcd = "", unselitcd = "", plist = "", selslcd = "", unselslcd = "", selitgrpcd = "", selbrgrpcd = "";
                if (FC.AllKeys.Contains("BaleNoBaleYrcdvalue")) BalenoBaleyr = CommFunc.retSqlformat(FC["BaleNoBaleYrcdvalue"].ToString());

                if (FC.AllKeys.Contains("itcdvalue")) itcd = FC["itcdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                txntag = txntag + txnrettag;
                bool RepeatAllRow = VE.Checkbox1;
                DataTable tbl = Salesfunc.GetBaleStock(tdt, "", BalenoBaleyr, itcd, "", "", selitgrpcd);
                //DataTable tbl = MasterHelp.SQLquery();
                if (tbl.Rows.Count == 0) return Content("no records..");

                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "", chkval2 = "";
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "pblno", "string", "c,20", "Bill No");
                HC.GetPrintHeader(IR, "docdt", "string", "c,16", "Bill Date");
                HC.GetPrintHeader(IR, "styleno", "string", "c,25", "Style No");
                HC.GetPrintHeader(IR, "Shade", "string", "c,25", "Shade");
                HC.GetPrintHeader(IR, "baleno", "string", "c,10", "Bale No");
                HC.GetPrintHeader(IR, "nos", "string", "c,16", "Nos");
                HC.GetPrintHeader(IR, "qnty", "string", "c,16", "Qnty");
                HC.GetPrintHeader(IR, "rate", "double", "c,16,2", "Rate");
                HC.GetPrintHeader(IR, "value", "double", "c,16", "Value");
                HC.GetPrintHeader(IR, "lrno", "string", "c,10", "LR No");
                HC.GetPrintHeader(IR, "docnm", "string", "c,10", "Page No.");

                double qty, amt = 0;
                double tqty = 0, tnos = 0;
                double gtqty = 0, gtnos = 0;

                Int32 rNo = 0;
                string baleno = "";
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int count = 0;
                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["BaleNoBaleYrcd"].ToString();
                    qty = 0; amt = 0;
                    bool balefirst = true;
                    //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    //IR.Rows[rNo]["cd"] = tbl.Rows[i]["cd"].ToString();
                    //IR.Rows[rNo]["nm"] = tbl.Rows[i]["nm"].ToString();
                    //IR.Rows[rNo]["snm"] = tbl.Rows[i]["snm"].ToString();
                    //IR.Rows[rNo]["celldesign"] = "cd=font-weight:bold;font-size:13px;^nm=font-weight:bold;font-size:13px;^snm=font-weight:bold;font-size:13px; ";
                    while (tbl.Rows[i]["BaleNoBaleYrcd"].ToString() == chkval)
                    {
                        bool itemfirst = true;
                        baleno = tbl.Rows[i]["baleno"].ToString();

                        chkval2 = tbl.Rows[i]["itcd"].ToString();
                        while (tbl.Rows[i]["itcd"].ToString() == chkval2)
                        {
                            tnos = tnos + tbl.Rows[i]["qnty"].retDbl();
                            tqty = tqty + tbl.Rows[i]["qnty"].retDbl();

                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                            if (RepeatAllRow == true || itemfirst == true) IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                            IR.Rows[rNo]["pblno"] = tbl.Rows[i]["prefno"].ToString();
                            IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].ToString();
                            IR.Rows[rNo]["docnm"] = tbl.Rows[i]["docnm"].ToString();
                            IR.Rows[rNo]["docdt"] = Convert.ToString(tbl.Rows[i]["docdt"]).Substring(0, 10);
                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                            IR.Rows[rNo]["gonm"] = tbl.Rows[i]["gonm"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                            IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].ToString();
                            IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].ToString();
                            balefirst = false; itemfirst = false;
                            i = i + 1;
                            if (i > maxR) break;
                        }

                        count++;
                        if (i > maxR) break;
                    }
                    if (RepeatAllRow == false)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = "";
                        //IR.Rows[rNo]["baleno"] = "Total of Bale " + baleno + " ";
                        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        //IR.Rows[rNo]["nos"] = tnos;
                        //IR.Rows[rNo]["qnty"] = tqty;
                    }

                    gtqty = gtqty + tnos;
                    gtnos = gtnos + tqty;
                }
                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                string pghdr1 = " Bale History from " + fdt + " to " + tdt;
                string repname = "Bale Report";
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