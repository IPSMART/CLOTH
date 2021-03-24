
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
    public class Rep_Job_regController : Controller
    {
        // GET: Rep_Job_reg
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false; bool showValue = false;
        string modulecode = CommVar.ModuleCode(); string repname = "";
        string pghdr1 = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Job_reg()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Job Work Register";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    VE.TDT = CommVar.CurrDate(UNQSNO);

                    VE.DropDown_list_JOBCD = DropDownHelp.DropDown_JOBCD();

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

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
        public ActionResult Rep_Job_reg(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr();
                tdt = VE.TDT.retDateStr();
                string recdt = VE.TEXTBOX1.retDateStr();
                showValue = VE.Checkbox1;
                string ShowPending = VE.TEXTBOX2.retStr();
                string ReportType = VE.TEXTBOX3.retStr();
                string RepFormat = VE.TEXTBOX4.retStr();
                string jobslcd = "", itcd = "";
                string JOBCD = VE.JOBCD;
                if (FC.AllKeys.Contains("slcdvalue")) jobslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("itcdvalue")) itcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());


                string sql = "";
                sql += " select a.autono, c.slcd, a.slno, c.nos, c.qnty, c.cutlength, i.itnm, i.styleno,h.ourdesign, h.pdesign, i.itgrpcd, j.itgrpnm,k.uomnm, ";
                sql += " c.itremark, c.sample, g.slnm, ptch.docno , ptch.docdt, y.issamt, ";
                sql += " b.autono recautono, rtch.docno recdocno, rtch.docdt recdocdt, b.prefno, b.prefdt, b.doctag,b.nos recnos,b.qnty recqnty,0 balqnty from ";
                sql += "  ";
                sql += " (select a.autono, a.slno, a.autono || a.slno autoslno ";
                sql += " from " + scm + ".t_progmast a, " + scm + ".t_cntrl_hdr b ";
                sql += " where a.autono = b.autono(+) and ";
                sql += " b.compcd = '" + COM + "' and nvl(b.cancel, 'N') = 'N' ";
                if (jobslcd != "") sql += " and a.slcd in(" + jobslcd + ") ";
                if (itcd != "") sql += " and a.slcd in(" + itcd + ") ";
                if (fdt != "") sql += " and b.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
                sql += " and b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ) a, ";
                sql += "  ";
                sql += "  ";
                sql += " (select a.progautono || a.progslno progautoslno,  ";
                sql += " a.nos, a.qnty, a.autono, a.rate, c.prefno, c.prefdt, c.doctag ";
                sql += " from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c ";
                sql += " where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and ";
                sql += " b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  ";
                sql += " b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') ) b, ";
                sql += "  ";
                sql += " (select a.progautono || a.progslno progautoslno, sum(round(a.qnty * a.rate, 2)) issamt ";
                sql += "     from " + scm + ".t_kardtl a, " + scm + ".t_cntrl_hdr b ";
                sql += " where a.autono = b.autono(+) and ";
                sql += " b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and ";
                sql += " b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
                sql += " group by a.progautono || a.progslno ) y, ";
                sql += "  ";
                sql += " " + scm + ".t_progmast c, " + scm + ".t_cntrl_hdr ptch, " + scm + ".t_cntrl_hdr rtch, ";
                sql += " " + scmf + ".m_subleg g, " + scm + ".t_batchmst h, " + scm + ".m_sitem i, " + scm + ".m_group j, " + scmf + ".m_uom k ";
                sql += " where a.autono=c.autono(+) and a.slno=c.slno(+) and a.autoslno = b.progautoslno(+) and a.autoslno = y.progautoslno(+) and ";
                sql += " a.autono = ptch.autono(+) and b.autono = rtch.autono(+) and C.JOBCD='" + JOBCD + "' and ";
                sql += " c.slcd = g.slcd(+) and c.barno = h.barno(+) and h.itcd = i.itcd(+) and i.itgrpcd = j.itgrpcd(+) and i.uomcd = k.uomcd(+) ";
                sql += " order by slnm, slcd, docdt, docno, slno, recdocdt, recdocno ";
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                }
                string lastautono = ""; string nextautono = ""; double totrecqnty = 0;
                int maxR = tbl.Rows.Count - 1;
                for (int i = 0; i <= maxR; i++)
                {
                    string autono = tbl.Rows[i]["autono"].retStr();
                    totrecqnty += tbl.Rows[i]["recqnty"].retDbl();
                    double issqnty = tbl.Rows[i]["qnty"].retDbl();
                    double balqnty = issqnty-totrecqnty;
                    if (maxR >= (i + 1))
                    {
                        if (autono != tbl.Rows[i + 1]["autono"].retStr())
                        {
                            tbl.Rows[i]["balqnty"] = balqnty;
                            totrecqnty = 0;
                        }
                    }
                    else
                    {
                        tbl.Rows[i]["balqnty"] = balqnty;
                    }
                    if (autono != lastautono)
                    {
                        lastautono = autono;
                    }
                }

                repname = "Job Work register".retRepname();
                pghdr1 = "Job Work register " + (recdt != "" ? " Received date: " + recdt + "" : " ") + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;

                if (ReportType == "DETAIL")
                {
                    return Detail(tbl, RepFormat);
                }
                else
                {
                    return Sumarry(tbl);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        private ActionResult Detail(DataTable tbl, string RepFormat)
        {
            DataTable IR = new DataTable("mstrep");
            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();

            HC.RepStart(IR, 2);
            if (RepFormat == "STANDARD")
            {
                HC.GetPrintHeader(IR, "Slnm", "string", "c,25", "Jobber Name.");
            }
            HC.GetPrintHeader(IR, "docdt", "string", "c,12", "Iss. Date.");
            HC.GetPrintHeader(IR, "docno", "string", "c,13", "Iss Doc No");
            HC.GetPrintHeader(IR, "itgrpnm", "string", "c,12", "Group");
            HC.GetPrintHeader(IR, "itnm", "string", "c,10", "Item");
            HC.GetPrintHeader(IR, "styleno", "string", "c,14", "Styleno");
            HC.GetPrintHeader(IR, "uom", "string", "c,10", "Uom");
            HC.GetPrintHeader(IR, "Nos", "string", "c,9", "Nos");
            HC.GetPrintHeader(IR, "cutlength", "double", "c,9", "cut length");
            HC.GetPrintHeader(IR, "qnty", "double", "c,15,3", "Prog.Qnty");
            if (showValue == true)
            {
                HC.GetPrintHeader(IR, "issamt", "double", "c,15,3", "Iss Amt.");
            }
            HC.GetPrintHeader(IR, "itremarks", "string", "c,15", "itremark");
            HC.GetPrintHeader(IR, "recnos", "double", "c,9", "Rec Nos.");
            HC.GetPrintHeader(IR, "recqnty", "double", "c,15,3", "Rec Qnty.");
            HC.GetPrintHeader(IR, "balqnty", "double", "c,15,3", "Bal Qnty.");

            Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
            i = 0; maxR = tbl.Rows.Count - 1;
            string lastslcd = "";
            while (i <= maxR)
            {
                string slcd = tbl.Rows[i]["slcd"].retStr();
                if (RepFormat == "JOBBERWISE" && slcd != lastslcd)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "" + tbl.Rows[i]["slnm"].retStr() + "[" + tbl.Rows[i]["slcd"].retStr() + "]";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    lastslcd = slcd;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                if (RepFormat == "STANDARD")
                {
                    IR.Rows[rNo]["Slnm"] = "" + tbl.Rows[i]["slnm"].retStr() + "[" + tbl.Rows[i]["slcd"].retStr() + "]";
                }
                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
                IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].retStr();
                IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].retStr();
                IR.Rows[rNo]["uom"] = tbl.Rows[i]["uomnm"].retStr();
                IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retStr();
                IR.Rows[rNo]["cutlength"] = tbl.Rows[i]["cutlength"].retDbl();
                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                if (showValue == true)
                {
                    IR.Rows[rNo]["issamt"] = tbl.Rows[i]["issamt"].retDbl();
                }
                IR.Rows[rNo]["itremarks"] = tbl.Rows[i]["itremark"].retStr();
                IR.Rows[rNo]["recnos"] = tbl.Rows[i]["recnos"].retDbl();
                IR.Rows[rNo]["recqnty"] = tbl.Rows[i]["recqnty"].retDbl();
                IR.Rows[rNo]["balqnty"] = tbl.Rows[i]["balqnty"].retDbl();
                i = i + 1;
                if (i > maxR) break;

            }

            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
            TempData[repname] = PV;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        }
        private ActionResult Sumarry(DataTable tbl)
        {
            DataTable IR = new DataTable("mstrep");

            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();

            HC.RepStart(IR, 2);
            HC.GetPrintHeader(IR, "docno", "string", "c,12", "Bill No.");
            HC.GetPrintHeader(IR, "docdt", "string", "c,12", "Bill Date");
            HC.GetPrintHeader(IR, "partynm", "string", "c,25", "Part Name");
            HC.GetPrintHeader(IR, "slarea", "string", "c,10", "Area");
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
                IR.Rows[rNo]["noofcases"] = tbl.Rows[i]["noofcases"].retDbl();
                IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                IR.Rows[rNo]["uomcd"] = tbl.Rows[i]["uomcd"].retStr();
                IR.Rows[rNo]["uomcd"] = tbl.Rows[i]["uomcd"].retStr();
                IR.Rows[rNo]["recqnty"] = tbl.Rows[i]["recqnty"].retStr();

                i = i + 1;
                if (i > maxR) break;

            }

            string pghdr1 = "";
            pghdr1 = repname + (fdt != "" ? " from " + fdt + " to " : "as on ") + tdt;

            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

            TempData[repname] = PV;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });


            return null;
        }
    }
}