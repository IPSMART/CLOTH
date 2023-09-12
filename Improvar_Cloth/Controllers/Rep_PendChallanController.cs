using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_PendChallanController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string jobcd = "", jobnm = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_PendChallan
        public ActionResult Rep_PendChallan()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    jobcd = VE.MENU_PARA;
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false; //show summary
                    VE.Checkbox2 = true; //Show Party
                    VE.Checkbox4 = false;
                    VE.TEXTBOX2 = "P"; //Calc on Box/Pcs/Sets;
                    VE.TEXTBOX4 = CommVar.CurrDate(UNQSNO);
                    string comcd = CommVar.Compcd(UNQSNO);
                    string location = CommVar.Loccd(UNQSNO);
                    jobcd = VE.MENU_PARA;
                    jobnm = DB.M_JOBMST.Find(jobcd).JOBNM;

                    //if (jobcd == "DY" || jobcd == "BL") VE.Checkbox4 = true;
                    ViewBag.formname = "Pending Challan";

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    if (VE.MENU_PARA == "IR") VE.Checkbox3 = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                ReportViewinHtml VE = new ReportViewinHtml();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ActionResult GetJobDetails(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.JOBCD_help(DB));
                }
                else
                {
                    var query = (from c in DB.M_JOBMST where (c.JOBCD == val) select c);
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.JOBCD + Cn.GCS() + i.JOBNM;
                        }
                        return Content(str);
                    }
                    else
                    {
                        return Content("0");
                    }

                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }

        [HttpPost]
        public ActionResult Rep_PendChallan(ReportViewinHtml VE, FormCollection FC)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string strslcd = "", linecd = "", stritcd = "", itgrpcd = "", fdt, tdt = "", AsOnDt = "";
                fdt = VE.FDT;
                tdt = VE.TDT;
                AsOnDt = VE.TEXTBOX4;
                jobcd = VE.TEXTBOX1;
                jobnm = jobcd.retStr() == "" ? "" : DB.M_JOBMST.Find(jobcd).JOBNM;
                bool showitem = false, showsumm = VE.Checkbox1, showparty = VE.Checkbox2;

                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    strslcd = FC["slcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itcdvalue"))
                {
                    strslcd = FC["itcdvalue"].ToString().retSqlformat();
                }
                DataTable tbl = Salesfunc.GetPendChallans(jobcd, strslcd, AsOnDt, "", "", "", true, false, "", "", VE.FDT, VE.TDT);
                tbl.DefaultView.Sort = "docdt,docno,slnm, slcd,styleno, itnm, itcd, partnm, partcd";
                tbl = tbl.DefaultView.ToTable();
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    if (stritcd != "")
                    {
                        var ITEM_LIST = stritcd.Split(',');
                        tbl = (from DataRow DR in tbl.Rows where ITEM_LIST.Contains(DR["itcd"].ToString()) select DR).CopyToDataTable();
                    }
                }
                else
                {
                    return Content("No Record Found");
                }

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                string qtydsp = "n,13,0:##,##,##,##0", qtydspo = "n,13,2:###,##,##0.00";
                string stkcalcon = VE.TEXTBOX2, qty1hd = "Box";
                if (stkcalcon == "S") qty1hd = "Sets";
                bool groupondate = false, showothrunit = true, showraka = true;
                if (showitem == false && showparty == false) groupondate = true;
                if (stkcalcon == "P") showothrunit = false;
                if (jobcd == "CT" || jobcd == "DY" || jobcd == "BL" || jobcd == "KT" || jobcd == "FP") qtydsp = "n,13,3:##,##,##0.000";
                if (jobcd == "KT" || jobcd == "DY" || jobcd == "BL" || jobcd == "FP") showraka = false;

                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);

                HC.GetPrintHeader(IR, "slno", "string", "n,3", "Sl");
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Chl;No");
                HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Chl;Date");
                HC.GetPrintHeader(IR, "slcd", "string", "c,10", "Party;Code");
                HC.GetPrintHeader(IR, "slnm", "string", "c,30", "Party;Name");
                HC.GetPrintHeader(IR, "prefno", "string", "c,30", "Party;Bill No.");
                if (VE.TEXTBOX2 == "Details")
                {
                    HC.GetPrintHeader(IR, "itnm", "string", "c,40", "Item Name;[Style No] ");
                }
                HC.GetPrintHeader(IR, "qnty", "double", qtydsp, "Quantity");

                Int32 maxR = 0, i = 0, rNo = 0;
                double TotalQnty = 0;
                i = 0; maxR = tbl.Rows.Count - 1;
                while (i <= maxR)
                {
                    string autono = tbl.Rows[i]["autono"].retStr();
                    while (tbl.Rows[i]["autono"].ToString() == autono)
                    {
                        string slcd = tbl.Rows[i]["slcd"].ToString();
                        while (tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["slcd"].ToString() == slcd)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["slno"] = i + 1;
                            IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Remove(10);
                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                            IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                            IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();
                            if (VE.TEXTBOX2 == "Details")
                            {
                                IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].ToString() + " [ " + tbl.Rows[i]["styleno"].ToString() + " ] ";
                            }
                            double qnty = 0;
                            string fld = VE.TEXTBOX2 == "Details" ? "itcd" : "slcd";
                            string fldval = tbl.Rows[i][fld].ToString();
                            while (tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["slcd"].ToString() == slcd && tbl.Rows[i][fld].ToString() == fldval)
                            {
                                qnty += tbl.Rows[i]["qnty"].retDbl();
                                TotalQnty += tbl.Rows[i]["qnty"].retDbl();
                                i++;
                                if (i > maxR) break;
                            }
                            IR.Rows[rNo]["qnty"] = qnty;
                            if (i > maxR) break;
                        }
                        if (i > maxR) break;
                    }
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["slnm"] = "Grand Totals";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                IR.Rows[rNo]["qnty"] = TotalQnty;
                string pghdr1 = "", repname = CommFunc.retRepname("rep_partyleg");
                pghdr1 = "Pending Challan (" + jobnm + ") from " + fdt + " to " + tdt;

                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
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