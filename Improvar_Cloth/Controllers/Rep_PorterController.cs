﻿
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
    public class Rep_PorterController : Controller
    {
        // GET: Rep_Porter
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Porter()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Porter Register";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("T");
                    VE.TEXTBOX1 = MasterHelp.ComboFill("porter", VE.DropDown_list_SLCD, 0, 1);
                    VE.DropDown_list_DOCCD = DropDownHelp.GetDocCdforSelection("'SBILD','SPSLP'");
                    VE.TEXTBOX2 = MasterHelp.ComboFill("doccd", VE.DropDown_list_DOCCD, 0, 1);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
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
        public ActionResult Rep_Porter(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr() == "" ? VE.TDT.retDateStr() : VE.FDT.retDateStr();
                tdt = VE.TDT.retDateStr();
                string selslcd = "", porter = "", doccd = "";
                if (FC.AllKeys.Contains("doccdvalue")) doccd = FC["doccdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("slcdvalue")) selslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("portervalue")) porter = CommFunc.retSqlformat(FC["portervalue"].ToString());
                string dtlsumm = FC["DtlSumm"].ToString(); string sql = "", descrption = "";
                if (dtlsumm == "D")
                {
                    descrption = "Detail";
                    sql = "select a.docdt,g.docno,a.doccd,a.slcd partycd,e.slnm partynm,c.mutslcd portercd,f.slnm porternm,e.slarea,c.noofcases,b.nos,b.qnty,d.uomcd ";
                    sql += " from " + scm + ".t_txn a ," + scm + ".t_txndtl b, " + scm + ".t_txnoth c, " + scm + ".M_SITEM d, " + scmf + ".M_SUBLEG e, " + scmf + ".M_SUBLEG f," + scm + ".t_cntrl_hdr g  ";
                    sql += " where a.autono = b.autono(+) and a.autono = c.autono(+) and b.itcd = d.itcd and a.slcd = e.slcd(+) and c.mutslcd=f.slcd(+)and  a.autono=g.autono and a.doccd in('SSPSL','SSBIL') ";
                    if (doccd.retStr() != "") sql += "and a.doccd in(" + doccd + ") ";
                    if (fdt.retStr() != "") sql += "and a.docdt >= to_date('" + fdt + "','dd/mm/yyyy')  ";
                    if (tdt.retStr() != "") sql += "and a.docdt <= to_date('" + tdt + "','dd/mm/yyyy')   ";
                    if (selslcd.retStr() != "") sql += "and a.slcd in(" + selslcd + ") ";
                    if (porter.retStr() != "") sql += "and c.mutslcd in(" + porter + ") ";
                    //sql += "order by portercd,a.docdt,a.docno,partycd ";
                    if (CommVar.ClientCode(UNQSNO) == "SNFP") sql += "order by a.docdt,a.docno,portercd,partycd ";
                    else sql += "order by portercd,a.docdt,a.docno,partycd ";
                }
                else {
                    descrption = "Summary";
                    sql = "select docdt, docno, doccd, partycd, partynm, portercd, porternm, slarea, uomcd, noofcases, nos, qnty,docrem from( ";
                    sql += "select a.docdt,g.docno,a.doccd,a.slcd partycd,e.slnm partynm,c.mutslcd portercd,f.slnm porternm,e.slarea,d.uomcd,sum(nvl(c.noofcases,0))noofcases,sum(nvl(b.nos,0))nos,sum(nvl(b.qnty,0))qnty,c.docrem ";
                    sql += " from " + scm + ".t_txn a ," + scm + ".t_txndtl b, " + scm + ".t_txnoth c, " + scm + ".M_SITEM d, " + scmf + ".M_SUBLEG e, " + scmf + ".M_SUBLEG f," + scm + ".t_cntrl_hdr g  ";
                    sql += " where a.autono = b.autono(+) and a.autono = c.autono(+) and b.itcd = d.itcd and a.slcd = e.slcd(+) and c.mutslcd=f.slcd(+)and  a.autono=g.autono and a.doccd in('SSPSL','SSBIL') ";
                    if (doccd.retStr() != "") sql += "and a.doccd in(" + doccd + ") ";
                    if (fdt.retStr() != "") sql += "and a.docdt >= to_date('" + fdt + "','dd/mm/yyyy')  ";
                    if (tdt.retStr() != "") sql += "and a.docdt <= to_date('" + tdt + "','dd/mm/yyyy')   ";
                    if (selslcd.retStr() != "") sql += "and a.slcd in(" + selslcd + ") ";
                    if (porter.retStr() != "") sql += "and c.mutslcd in(" + porter + ") ";
                    sql += "group by a.docdt,g.docno,a.doccd,a.slcd,e.slnm,c.mutslcd,f.slnm,e.slarea,d.uomcd,c.docrem )";
                    //sql += "order by portercd,docdt,docno,partycd ";
                    if (CommVar.ClientCode(UNQSNO) == "SNFP") sql += "order by docdt,docno,portercd,partycd ";
                    else sql += "order by portercd,docdt,docno,partycd ";
                }

                DataTable tbl = MasterHelp.SQLquery(sql);

                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                }
                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 2);
                HC.GetPrintHeader(IR, "docno", "string", "c,12", "Bill No.");
                HC.GetPrintHeader(IR, "docdt", "string", "c,12", "Bill Date");
                HC.GetPrintHeader(IR, "partynm", "string", "c,25", "Party Name");
                HC.GetPrintHeader(IR, "slarea", "string", "c,10", "Area");
                if (dtlsumm == "S") HC.GetPrintHeader(IR, "docrem", "string", "c,35", "Doc. Remarks");
                HC.GetPrintHeader(IR, "porternm", "string", "c,25", "Porter Name");
                HC.GetPrintHeader(IR, "noofcases", "double", "c,15", "NO OF PACKAGE");
                HC.GetPrintHeader(IR, "nos", "double", "c,15", "Nos");
                HC.GetPrintHeader(IR, "qnty", "double", "c,15,3", "QUANTITY");
                HC.GetPrintHeader(IR, "uomcd", "string", "c,15", "Uom");


                Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                    IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                    IR.Rows[rNo]["partynm"] = tbl.Rows[i]["partynm"].retStr();
                    IR.Rows[rNo]["slarea"] = tbl.Rows[i]["slarea"].retStr();
                    IR.Rows[rNo]["porternm"] = tbl.Rows[i]["porternm"].retStr();
                    IR.Rows[rNo]["noofcases"] = tbl.Rows[i]["noofcases"].retDbl();
                    IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                    IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                    IR.Rows[rNo]["uomcd"] = tbl.Rows[i]["uomcd"].retStr();
                    if (dtlsumm == "S") IR.Rows[rNo]["docrem"] = tbl.Rows[i]["docrem"].retStr();
                    i = i + 1;
                    if (i > maxR) break;

                }
                string pghdr1 = "";
                string repname = "Porter Register" + " [" + descrption + "]";
                pghdr1 = repname + (fdt != "" ? " from " + fdt + " to " : "as on ") + tdt;

                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

                TempData[repname] = PV;
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