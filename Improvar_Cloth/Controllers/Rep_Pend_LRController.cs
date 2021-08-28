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
                string slcd = "", lrnoLike = "";
                if (FC.AllKeys.Contains("slcdvalue")) slcd = FC["slcdvalue"].ToString();
                lrnoLike = VE.TEXTBOX1;
                DataTable tbl = Salesfunc.getPendRecfromMutia(tdt, slcd,"","","",lrnoLike);
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
                HC.GetPrintHeader(IR, "slno", "double", "n,8", "Sl No.");
                HC.GetPrintHeader(IR, "lrno", "string", "c,20", "LR No");
                HC.GetPrintHeader(IR, "lrdt", "string", "d,10", "LR Date");
                HC.GetPrintHeader(IR, "baleno", "string", "c,35", "Bale No");
                HC.GetPrintHeader(IR, "sortno", "string", "c,20", "Sort No");
                HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Pcs");
                HC.GetPrintHeader(IR, "Mtrs", "string", "c,10", "Mtrs");
                HC.GetPrintHeader(IR, "prefdt", "string", "d,10", "Entry Date");
                HC.GetPrintHeader(IR, "pageno", "string", "c,20", "Page No");
                Int32 rNo = 0;
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int slno = 0;
                while (i <= maxR)
                {
                    slno++;
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["slno"] = slno;
                    IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].retStr();
                    IR.Rows[rNo]["lrdt"] = tbl.Rows[i]["lrdt"].retDateStr();
                    IR.Rows[rNo]["prefdt"] = tbl.Rows[i]["prefdt"].retDateStr();
                    IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                    IR.Rows[rNo]["sortno"] = tbl.Rows[i]["nos"].retStr();
                    if (tbl.Rows[i]["qnty"].retDbl() != 0) IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                    IR.Rows[rNo]["Mtrs"] = tbl.Rows[i]["uomcd"].retStr();
                    IR.Rows[rNo]["pageno"] = tbl.Rows[i]["pageno"].retStr();
                    i++;
                    if (i > maxR) break;
                }
                string pghdr1 = "Pending Bilty Register from " + fdt + " to " + tdt;
                string repname = "Pending Bilty Register";
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