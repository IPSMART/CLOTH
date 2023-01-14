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
    public class Rep_Pend_LRController : Controller
    {
        // GET: Rep_Pend_LR
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        public ActionResult Rep_Pend_LR()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Pending Bilty Register";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("T");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
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
        public ActionResult Rep_Pend_LR(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();
                string slcd = "", lrnoLike = "", BltyPending = "";
                if (FC.AllKeys.Contains("slcdvalue")) slcd = FC["slcdvalue"].ToString().retSqlformat();
                lrnoLike = VE.TEXTBOX1;
                BltyPending = FC["BltyPending"].ToString();
                bool ShowOnlyAfterIssue = VE.Checkbox1;
                bool ShowDocDate = VE.Checkbox2;
                DataTable tbl = new DataTable();
                if (BltyPending == "R")
                { tbl = Salesfunc.getPendRecfromMutia(tdt, slcd, "", "", "", lrnoLike, ShowOnlyAfterIssue); }
                else { tbl = Salesfunc.getPendBiltytoIssue(tdt, "", "", "", slcd, lrnoLike); }
                DataView dv = new DataView(tbl);
                DataTable tbl1 = new DataTable();
                if (BltyPending == "R" && ShowDocDate == true)
                {
                    tbl1 = dv.ToTable(true, "lrno", "lrdt", "prefdt", "baleno", "styleno", "qnty", "uomcd", "pageno","docdt", "pageslno", "docno");
                }
                else
                {
                    tbl1 = dv.ToTable(true, "lrno", "lrdt", "prefdt", "baleno", "styleno", "qnty", "uomcd", "pageno", "pageslno");
                }
                tbl1.DefaultView.Sort = "lrno";
                tbl1 = tbl1.DefaultView.ToTable();
                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "", chkval2 = "", gonm = "";
                if (tbl1.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }
                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "slno", "double", "n,8", "Sl No.");
                HC.GetPrintHeader(IR, "lrno", "string", "c,20", "LR No");
                HC.GetPrintHeader(IR, "lrdt", "string", "d,10", "LR Date");
                if (ShowDocDate == true) { HC.GetPrintHeader(IR, "docdt", "string", "d,10", "Doc. Date"); HC.GetPrintHeader(IR, "docno", "string", "c,20", "Doc No"); }
                HC.GetPrintHeader(IR, "baleno", "string", "c,35", "Bale No");
                HC.GetPrintHeader(IR, "sortno", "string", "c,20", "Sort No");
                HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Pcs");
                HC.GetPrintHeader(IR, "Mtrs", "string", "c,10", "Mtrs");
                HC.GetPrintHeader(IR, "prefdt", "string", "d,10", "Entry Date");
                HC.GetPrintHeader(IR, "pageno", "string", "c,20", "Page No");
                HC.GetPrintHeader(IR, "pageslno", "double", "n,8", "Page SlNo.");
                Int32 rNo = 0;
                // Report begins
                i = 0; maxR = tbl1.Rows.Count - 1;
                int slno = 0;
                while (i <= maxR)
                {
                    slno++;
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["slno"] = slno;
                    IR.Rows[rNo]["lrno"] = tbl1.Rows[i]["lrno"].retStr();
                    IR.Rows[rNo]["lrdt"] = tbl1.Rows[i]["lrdt"].retDateStr();
                    if (ShowDocDate == true) { IR.Rows[rNo]["docdt"] = tbl1.Rows[i]["docdt"].retDateStr(); IR.Rows[rNo]["docno"] = tbl1.Rows[i]["docno"].retStr(); }
                    IR.Rows[rNo]["prefdt"] = tbl1.Rows[i]["prefdt"].retDateStr();
                    IR.Rows[rNo]["baleno"] = tbl1.Rows[i]["baleno"].retStr();
                    IR.Rows[rNo]["sortno"] = tbl1.Rows[i]["styleno"].retStr();
                    if (tbl1.Rows[i]["qnty"].retDbl() != 0) IR.Rows[rNo]["qnty"] = tbl1.Rows[i]["qnty"].retDbl();
                    IR.Rows[rNo]["Mtrs"] = tbl1.Rows[i]["uomcd"].retStr();
                    IR.Rows[rNo]["pageno"] = tbl1.Rows[i]["pageno"].retStr();
                    IR.Rows[rNo]["pageslno"] = tbl1.Rows[i]["pageslno"].retDbl();
                    i++;
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["lrno"] = "Total";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                IR.Rows[rNo]["baleno"] = (from DataRow dr in tbl1.Rows select dr["baleno"].retStr()).Distinct().Count().retDbl();
               
                string head = BltyPending == "R" ? "Receive from Mutia" : "Issue to Mutia";
                string pghdr1 = "Pending " + head + " Register from " + fdt + " to " + tdt;
                string repname = ("Pend " + head + " Reg").retRepname();
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