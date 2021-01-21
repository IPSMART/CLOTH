using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Khasra_BaleTrnf_PrintController : Controller
    {
        // GET: Khasra_BaleTrnf_Print
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();

        string UNQSNO = CommVar.getQueryStringUNQSNO();

        public ActionResult Khasra_BaleTrnf_Print()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Document Printing";
                    ReportViewinHtml VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new ReportViewinHtml();
                    }
                    else
                    {
                        VE = (ReportViewinHtml)TempData["printparameter"];
                    }
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //string reptype = "SALEBILL";
                    string reptype = "SALEBILL";
                    if (VE.maxdate == "CHALLAN") reptype = "CHALLAN";
                    DataTable repformat = Salesfunc.getRepFormat(reptype);

                    if (repformat != null)
                    {
                        VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                                             select new DropDown_list1()
                                             {
                                                 text = dr["text"].ToString(),
                                                 value = dr["value"].ToString()
                                             }).ToList();
                    }
                    else
                    {
                        List<DropDown_list1> drplst = new List<DropDown_list1>();
                        VE.DropDown_list1 = drplst;
                    }
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubLedgerDetails(string val, string code)
        {
            try
            {
                var str = masterHelp.SLCD_help(val, code);
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
        public ActionResult GetSubLedgerDetails(ReportViewinHtml VE, FormCollection FC)
        {
            try
            {
                //Cn.getQueryString(VE);

                string fdate = "", tdate = "";
                if (VE.FDT != null)
                {
                    fdate = Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
                }
                if (VE.TDT != null)
                {
                    tdate = Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
                }
                string fdocno = FC["FDOCNO"].ToString();
                string tdocno = FC["TDOCNO"].ToString();
                string doccd = FC["doccd"].ToString();
                string slcd = VE.TEXTBOX3;

                string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                string sql = "select a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, ";
                sql += "g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, ";
                sql += "g.nos, g.qnty, h.styleno||' '||h.itnm  itstyle, listagg(j.shade,',') within group (order by j.autono, j.txnslno) as shade, ";
                sql += "g.pageno, g.pageslno, g.rate, f.prefno, f.prefdt,a.autono,f.gocd hdrgocd,l.goadd1 hdrgoadd1,l.goadd2 hdrgoadd2,l.goadd3 hdrgoadd3,l.gophno hdrgophno,l.goemail hdrgoemail ";
                sql += "from  ( ";
                sql += "select c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr, ";
                sql += "sum(case c.stkdrcr when 'D' then c.qnty when 'C' then c.qnty*-1 end) qnty,a.autono ";
                sql += "from " + scm + ".t_bale a, " + scm + ".t_bale_hdr b, " + scm + ".t_txndtl c, " + scm + ".t_cntrl_hdr d ";
                sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
                sql += "a.autono=c.autono(+) and a.slno=c.slno(+) and c.stkdrcr in ('D','C') and ";
                sql += "d.compcd = '" + COM + "' and nvl(d.cancel, 'N') = 'N' and ";
                sql += "d.loccd='" + LOC + "' and ";
                if (fdate.retStr() != "") sql += "d.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') ";
                if (tdate.retStr() != "") sql += "d.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') ";
                sql += "group by c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno,a.autono ";

                sql += ") a, ";
                sql += "" + scm + ".t_txntrans e, " + scm + ".t_txn f, " + scm + ".t_txndtl g, " + scm + ".m_sitem h, " + scm + ".m_group i, " + scm + ".t_batchdtl j, " + scmf + ".m_godown k, " + scmf + ".m_godown l ";
                sql += "where a.blautono = e.autono(+) and a.blautono = f.autono(+) and ";
                sql += "g.autono=j.autono(+) and g.slno=j.txnslno(+) and a.blautono = g.autono(+) and a.blslno = g.slno(+) and g.itcd = h.itcd(+) and f.gocd = l.gocd(+)  ";
                //if (itgrpcd != "") sql += "and f.itgrpcd in (" + itgrpcd + ")  ";
                //if (itcd != "") sql += "and a.itcd in (" + itcd + ")  ";
                //if (baleno != "") sql += "and a.baleno||baleyr in (" + baleno + ")  ";
                sql += "and h.itgrpcd = i.itgrpcd(+) and a.gocd=k.gocd(+) and nvl(a.qnty, 0) > 0 and a.slno <1000 ";
                sql += "group by a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, ";
                sql += "g.nos, g.qnty, h.styleno||' '||h.itnm, g.pageno, g.pageslno, f.prefno, f.prefdt ";
                sql += "order by a.autono, f.prefno, a.baleno ";


                DataTable restbl = new DataTable("restbl");
                restbl = masterHelp.SQLquery(sql);

                if (restbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "No Records Found !!" });
                }
                DataTable IR = new DataTable();
                IR.Columns.Add("autono", typeof(string), "");
                IR.Columns.Add("docno", typeof(string), "");
                IR.Columns.Add("docdt", typeof(string), "");

                IR.Columns.Add("slno", typeof(double), "");
               
                IR.Columns.Add("nslcd", typeof(string), "");
                IR.Columns.Add("nslnm", typeof(string), "");
                IR.Columns.Add("nadd1", typeof(string), "");
                IR.Columns.Add("nadd2", typeof(string), "");
                IR.Columns.Add("nadd3", typeof(string), "");
                IR.Columns.Add("nadd4", typeof(string), "");
                IR.Columns.Add("nadd5", typeof(string), "");
                IR.Columns.Add("nadd6", typeof(string), "");
                IR.Columns.Add("nadd7", typeof(string), "");
                IR.Columns.Add("ngstno", typeof(string), "");
                IR.Columns.Add("dslcd", typeof(string), "");
                IR.Columns.Add("dslnm", typeof(string), "");
                IR.Columns.Add("dadd1", typeof(string), "");
                IR.Columns.Add("dadd2", typeof(string), "");
                IR.Columns.Add("dadd3", typeof(string), "");
                IR.Columns.Add("dadd4", typeof(string), "");
                IR.Columns.Add("dadd5", typeof(string), "");
                IR.Columns.Add("dadd6", typeof(string), "");
                IR.Columns.Add("dadd7", typeof(string), "");
                IR.Columns.Add("dgstno", typeof(string), "");
                IR.Columns.Add("mslcd", typeof(string), "");
                IR.Columns.Add("mslnm", typeof(string), "");
                IR.Columns.Add("madd1", typeof(string), "");
                IR.Columns.Add("madd2", typeof(string), "");
                IR.Columns.Add("madd3", typeof(string), "");
                IR.Columns.Add("madd4", typeof(string), "");
                IR.Columns.Add("madd5", typeof(string), "");
                IR.Columns.Add("madd6", typeof(string), "");
                IR.Columns.Add("madd7", typeof(string), "");
                IR.Columns.Add("mgstno", typeof(string), "");
                IR.Columns.Add("refno", typeof(string), "");
                IR.Columns.Add("itcd", typeof(string), "");
                IR.Columns.Add("itnm", typeof(string), "");
                IR.Columns.Add("countno", typeof(string), "");
                IR.Columns.Add("qtynos", typeof(string), "");
                IR.Columns.Add("qnty", typeof(string), "");
                IR.Columns.Add("uomcd", typeof(string), "");
                IR.Columns.Add("hsnsaccd", typeof(string), "");
                IR.Columns.Add("packsize", typeof(string), "");
                IR.Columns.Add("rate", typeof(double), "");
                IR.Columns.Add("bname", typeof(string), "");
                IR.Columns.Add("pytrem", typeof(string), "");
                IR.Columns.Add("docrem", typeof(string), "");
                IR.Columns.Add("DELVRYREM", typeof(string), "");
                IR.Columns.Add("PYTDAYS", typeof(string), "");
                IR.Columns.Add("taxgrpcd", typeof(string), "");
                IR.Columns.Add("exmillamt", typeof(double), "");
                IR.Columns.Add("ratetype", typeof(string), "");

                int maxR = restbl.Rows.Count - 1;
                Int32 i = 0;
                int ir1 = 0;
                while (i <= maxR)
                {
                    string autono = restbl.Rows[i]["autono"].ToString();
                    while (restbl.Rows[i]["autono"].ToString() == autono)
                    {
                        DataRow dr1 = IR.NewRow();
                        dr1["cancel"] = restbl.Rows[i]["cancel"].ToString();
                        IR.Rows.Add(dr1);
                        i = i + 1;
                        if (i > maxR) break;
                    }

                    if (i > maxR) break;
                }
                //
                string rptfile = "SaleBill.rpt";
                if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6;
                string rptname = "~/Report/" + rptfile; // "SaleBill.rpt";

                string compaddress;
                string gocd = "";
                compaddress = Salesfunc.retCompAddress(gocd);

                string blhead = "";
                switch (VE.MENU_PARA)
                {
                    case "KHSR":
                        blhead = "Khasra"; break;
                    case "TRFB":
                        blhead = "Sotck Transfer Bale"; break;
                    default: blhead = ""; break;
                }
                ReportDocument reportdocument = new ReportDocument();
                reportdocument.Load(Server.MapPath(rptname));
                reportdocument.SetDataSource(IR);
                reportdocument.SetParameterValue("billheading", blhead);
                reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
                reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
                reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                reportdocument.SetParameterValue("formerlynm", compaddress.retCompValue("formerlynm"));


                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "application/pdf");

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }
    }
}


