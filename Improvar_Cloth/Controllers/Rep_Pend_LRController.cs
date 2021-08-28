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
                    ViewBag.formname = "Pending Bilty";
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
                bool showitems = false;
                bool exldump = false, showdocno = false; bool ShowPendings = VE.Checkbox1; bool viewUom = VE.Checkbox2;
                string slcd = "", selitcd = "";
                if (FC.AllKeys.Contains("slcdvalue")) slcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());

                DataTable tbl = Salesfunc.getPendRecfromMutia(tdt, slcd);
                tbl.DefaultView.Sort = "docdt desc";
                tbl = tbl.DefaultView.ToTable();
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
                HC.GetPrintHeader(IR, "slcd", "string", "c,20", "Party Code");
                HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                HC.GetPrintHeader(IR, "city", "string", "c,20", "City");
                HC.GetPrintHeader(IR, "docdt", "string", "d,10", "Bill Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,20", "Bill No");
                HC.GetPrintHeader(IR, "Pcstype", "string", "c,10", "Pcstype");
                HC.GetPrintHeader(IR, "Qnty", "double", "n,15,3", "Qnty");
                HC.GetPrintHeader(IR, "Rate", "double", "n,15,3", "Rate");
                Int32 rNo = 0;
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int slno = 0;
                while (i <= maxR)
                {
                    slno++;
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["slno"] = slno;
                    IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                    IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                    IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
                    IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr();
                    IR.Rows[rNo]["city"] = tbl.Rows[i]["city"].retStr();
                    if (tbl.Rows[i]["qnty"].retDbl() != 0) IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                    if (tbl.Rows[i]["rate"].retDbl() != 0) IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                    IR.Rows[rNo]["Pcstype"] = tbl.Rows[i]["Pcstype"].retStr();
                    i++;
                    if (i > maxR) break;
                }
                string pghdr1 = "Rate Query Register from " + fdt + " to " + tdt;
                string repname = "Rate Query Register";
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